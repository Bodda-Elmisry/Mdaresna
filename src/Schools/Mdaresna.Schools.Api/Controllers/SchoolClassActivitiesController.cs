using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Time;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.Students;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController, Authorize, Route("api/schools/v1")]
public sealed class SchoolClassActivitiesController(ISchoolDbContextFactory dbFactory, SchoolClock clock) : ControllerBase
{
    [HttpGet("class-workspace/{classSectionId:guid}/activities/subjects"), Authorize(Policy = SchoolPermissionPolicies.ActivitiesView)]
    public async Task<IActionResult> Subjects(Guid classSectionId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await ClassExists(db, classSectionId, ct)) return NotFound(Failure(404, "activities.class_not_found", "Class section was not found."));
        if (!await CanAccessClass(db, classSectionId, ct)) return Forbid();
        var admin = await IsAdmin(db, ct); var userId = CurrentUserId();
        var query = db.ClassSectionSubjects.AsNoTracking().Where(x => x.ClassSectionId == classSectionId && x.IsActive && !x.IsDeleted && x.GradeSubjectOffering.IsActive);
        if (!admin) query = query.Where(x => db.ClassSectionTeacherScopes.Any(scope => scope.ClassSectionId == classSectionId && scope.IsActive && !scope.IsDeleted &&
            scope.TeacherGradeSubjectScope.IsActive && !scope.TeacherGradeSubjectScope.IsDeleted && scope.TeacherGradeSubjectScope.TeacherUserId == userId &&
            scope.TeacherGradeSubjectScope.GradeSubjectOfferingId == x.GradeSubjectOfferingId));
        var rows = await query.OrderBy(x => x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr)
            .Select(x => new ActivitySubjectResponse(x.Id, x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn)).ToArrayAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<ActivitySubjectResponse>>.Success(rows, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("class-workspace/{classSectionId:guid}/activities/students"), Authorize(Policy = SchoolPermissionPolicies.ActivitiesView)]
    public async Task<IActionResult> Students(Guid classSectionId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await ClassExists(db, classSectionId, ct)) return NotFound(Failure(404, "activities.class_not_found", "Class section was not found."));
        if (!await CanAccessClass(db, classSectionId, ct)) return Forbid();
        var rows = await ActiveStudents(db, classSectionId).OrderBy(x => x.Student.FullNameAr)
            .Select(x => new ActivityStudentResponse(x.Id, x.StudentId, x.Student.StudentCode, x.Student.FullNameAr, x.Student.FullNameEn)).ToArrayAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<ActivityStudentResponse>>.Success(rows, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("class-workspace/{classSectionId:guid}/activities"), Authorize(Policy = SchoolPermissionPolicies.ActivitiesView)]
    public async Task<IActionResult> List(Guid classSectionId, [FromQuery] ActivityScope? scope, [FromQuery] Guid? classSectionSubjectId,
        [FromQuery] ClassActivityStatus? status, [FromQuery] ActivityCategory? category, [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (pageNumber < 1 || pageSize is < 1 or > 100) return BadRequest(Failure(400, "activities.paging_invalid", "Invalid paging values."));
        if (!await ClassExists(db, classSectionId, ct)) return NotFound(Failure(404, "activities.class_not_found", "Class section was not found."));
        if (!await CanAccessClass(db, classSectionId, ct)) return Forbid();
        if (from.HasValue && to.HasValue && from > to) return BadRequest(Failure(400, "activities.date_range_invalid", "The date range is invalid."));
        if (classSectionSubjectId.HasValue && !await CanAccessSubject(db, classSectionId, classSectionSubjectId.Value, ct)) return Forbid();

        var admin = await IsAdmin(db, ct); var userId = CurrentUserId();
        // Existing activities remain historical records even when their class-subject link is later soft-deleted.
        var query = db.ClassActivities.IgnoreQueryFilters().AsNoTracking().Where(x => x.ClassSectionId == classSectionId);
        if (!admin) query = query.Where(x => x.Scope == ActivityScope.General || x.ClassSectionSubject != null &&
            db.ClassSectionTeacherScopes.Any(teacher => teacher.ClassSectionId == classSectionId && teacher.IsActive && !teacher.IsDeleted &&
                teacher.TeacherGradeSubjectScope.IsActive && !teacher.TeacherGradeSubjectScope.IsDeleted &&
                teacher.TeacherGradeSubjectScope.TeacherUserId == userId &&
                teacher.TeacherGradeSubjectScope.GradeSubjectOfferingId == x.ClassSectionSubject.GradeSubjectOfferingId));
        if (scope.HasValue) query = query.Where(x => x.Scope == scope);
        if (classSectionSubjectId.HasValue) query = query.Where(x => x.ClassSectionSubjectId == classSectionSubjectId);
        if (status.HasValue) query = query.Where(x => x.Status == status);
        if (category.HasValue) query = query.Where(x => x.Category == category);
        if (from.HasValue) query = query.Where(x => x.ActivityDate >= from);
        if (to.HasValue) query = query.Where(x => x.ActivityDate <= to);
        var total = await query.CountAsync(ct);
        var rows = await query.OrderByDescending(x => x.ActivityDate).ThenByDescending(x => x.CreatedAtUtc).ThenByDescending(x => x.Id)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new ClassActivityListItemResponse(x.Id, x.ClassSectionId, x.ClassSectionSubjectId,
                x.ClassSectionSubject == null ? null : x.ClassSectionSubject.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                x.ClassSectionSubject == null ? null : x.ClassSectionSubject.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn,
                x.Scope, x.Category, x.Status, x.AudienceMode, x.Title, x.ActivityDate, x.StartsAt, x.EndsAt, x.Location,
                x.IsGraded, x.TotalScore, x.Participants.Count, x.Participants.Count(p => p.Status != ActivityParticipationStatus.NotRecorded),
                x.Participants.Count(p => p.Status == ActivityParticipationStatus.Participated),
                x.Participants.Count(p => p.Status == ActivityParticipationStatus.Excused),
                x.Participants.Where(p => p.Score != null).Average(p => p.Score),
                x.Status == ClassActivityStatus.Published && x.AudienceMode == ActivityAudienceMode.AllClass &&
                db.StudentEnrollments.Any(enrollment => enrollment.ClassSectionId == x.ClassSectionId &&
                    enrollment.Status == StudentEnrollmentStatus.Active && enrollment.Student.IsActive &&
                    !x.Participants.Any(participant => participant.StudentEnrollmentId == enrollment.Id)), x.CreatedByUser.Person.DisplayName,
                x.CreatedAtUtc, x.PublishedAtUtc, x.CompletedAtUtc)).ToArrayAsync(ct);
        return Ok(PagedApiResponse<ClassActivityListItemResponse>.Success(rows, total, pageNumber, pageSize, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("class-workspace/{classSectionId:guid}/activities"), Authorize(Policy = SchoolPermissionPolicies.ActivitiesManage)]
    public async Task<IActionResult> Create(Guid classSectionId, [FromBody] SaveClassActivityRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var access = await ValidateAccessAndRequest(db, classSectionId, request, ct); if (access is not null) return access;
        var school = await db.SchoolInformation.AsNoTracking().SingleOrDefaultAsync(ct);
        if (school is null) return NotFound(Failure(404, "activities.school_not_found", "School information was not found."));
        SchoolLocalNow now;
        try { now = clock.Now(school.TimeZoneId ?? string.Empty); }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        { return Conflict(Failure(409, "activities.time_zone_required", "Configure a valid school time zone first.")); }
        var entity = new ClassActivity { Id = Guid.NewGuid(), ClassSectionId = classSectionId, CreatedByUserId = CurrentUserId(),
            CreatedAtUtc = now.UtcNow, UpdatedAtUtc = now.UtcNow, TimeZoneIdSnapshot = now.TimeZoneId };
        Apply(entity, request, now.UtcNow); SetAudience(entity, request.StudentEnrollmentIds);
        db.ClassActivities.Add(entity); AddAudit(db, entity, "Created", now.UtcNow); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(new { entity.Id }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("activities/{id:guid}"), Authorize(Policy = SchoolPermissionPolicies.ActivitiesView)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await Load(db, id, ct); if (entity is null) return NotFound(Failure(404, "activities.not_found", "Class activity was not found."));
        if (!await CanAccessActivity(db, entity, ct)) return Forbid();
        var hasStudentRosterChanges = await HasStudentRosterChanges(db, entity, ct);
        return Ok(ApiResponse<ClassActivityDetailsResponse>.Success(ToDetails(entity, hasStudentRosterChanges), correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("activities/{id:guid}"), Authorize(Policy = SchoolPermissionPolicies.ActivitiesManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveClassActivityRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await Load(db, id, ct); if (entity is null) return NotFound(Failure(404, "activities.not_found", "Class activity was not found."));
        if (!await CanAccessActivity(db, entity, ct)) return Forbid();
        if (entity.Status != ClassActivityStatus.Draft) return Conflict(Failure(409, "activities.not_draft", "Only a draft activity can be edited."));
        var validation = await ValidateAccessAndRequest(db, entity.ClassSectionId, request, ct); if (validation is not null) return validation;
        var now = DateTimeOffset.UtcNow; Apply(entity, request, now); ReconcileAudience(db, entity, request.StudentEnrollmentIds);
        AddAudit(db, entity, "Updated", now); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpDelete("activities/{id:guid}"), Authorize(Policy = SchoolPermissionPolicies.ActivitiesManage)]
    public async Task<IActionResult> DeleteDraft(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await Load(db, id, ct); if (entity is null) return NotFound(Failure(404, "activities.not_found", "Class activity was not found."));
        if (!await CanAccessActivity(db, entity, ct)) return Forbid();
        if (entity.Status != ClassActivityStatus.Draft) return Conflict(Failure(409, "activities.cancel_required", "Published activities must be cancelled instead of deleted."));
        db.ClassActivities.Remove(entity); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("activities/{id:guid}/publish"), Authorize(Policy = SchoolPermissionPolicies.ActivitiesPublish)]
    public async Task<IActionResult> Publish(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await Load(db, id, ct); if (entity is null) return NotFound(Failure(404, "activities.not_found", "Class activity was not found."));
        if (!await CanAccessActivity(db, entity, ct)) return Forbid();
        if (entity.Status != ClassActivityStatus.Draft) return Conflict(Failure(409, "activities.not_draft", "Only a draft activity can be published."));
        Guid[] enrollmentIds;
        if (entity.AudienceMode == ActivityAudienceMode.AllClass)
            enrollmentIds = await ActiveStudents(db, entity.ClassSectionId).Select(x => x.Id).ToArrayAsync(ct);
        else
        {
            enrollmentIds = entity.AudienceStudents.Select(x => x.StudentEnrollmentId).ToArray();
            var validCount = await ActiveStudents(db, entity.ClassSectionId).CountAsync(x => enrollmentIds.Contains(x.Id), ct);
            if (validCount != enrollmentIds.Length) return Conflict(Failure(409, "activities.audience_changed", "One or more selected students are no longer active in this class."));
        }
        if (enrollmentIds.Length == 0) return Conflict(Failure(409, "activities.audience_empty", "The activity must have at least one participant."));
        var now = CurrentUtc(entity); entity.Status = ClassActivityStatus.Published; entity.PublishedAtUtc = now;
        entity.PublishedByUserId = CurrentUserId(); entity.UpdatedAtUtc = now;
        db.ClassActivityParticipants.AddRange(enrollmentIds.Select(enrollmentId => new ClassActivityParticipant { Id = Guid.NewGuid(),
            ClassActivityId = entity.Id, StudentEnrollmentId = enrollmentId, CreatedAtUtc = now, UpdatedAtUtc = now }));
        AddAudit(db, entity, "Published", now, payload: JsonSerializer.Serialize(new { StudentCount = enrollmentIds.Length }));
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { return Conflict(Failure(409, "activities.already_published", "The activity was already published.")); }
        return Ok(ApiResponse<object?>.Success(new { ParticipantCount = enrollmentIds.Length }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("activities/{id:guid}/cancel"), Authorize(Policy = SchoolPermissionPolicies.ActivitiesPublish)]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] ActivityReasonRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await Load(db, id, ct); if (entity is null) return NotFound(Failure(404, "activities.not_found", "Class activity was not found."));
        if (!await CanAccessActivity(db, entity, ct)) return Forbid();
        if (entity.Status != ClassActivityStatus.Published)
            return Conflict(Failure(409, "activities.cancel_not_allowed", "Only a published activity can be cancelled; delete an unwanted draft."));
        if (string.IsNullOrWhiteSpace(request.Reason)) return BadRequest(Failure(400, "activities.reason_required", "Enter a cancellation reason."));
        var now = CurrentUtc(entity); entity.Status = ClassActivityStatus.Cancelled; entity.CancelledAtUtc = now;
        entity.CancelledByUserId = CurrentUserId(); entity.CancellationReason = Clean(request.Reason, 1000); entity.UpdatedAtUtc = now;
        AddAudit(db, entity, "Cancelled", now, request.Reason); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("activities/{id:guid}/students/sync"), Authorize(Policy = SchoolPermissionPolicies.ActivitiesManage)]
    public async Task<IActionResult> SyncStudents(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await Load(db, id, ct); if (entity is null) return NotFound(Failure(404, "activities.not_found", "Class activity was not found."));
        if (!await CanAccessActivity(db, entity, ct)) return Forbid();
        if (entity.Status != ClassActivityStatus.Published) return Conflict(Failure(409, "activities.not_published", "Only a published activity can synchronize students."));
        if (entity.AudienceMode != ActivityAudienceMode.AllClass) return Conflict(Failure(409, "activities.sync_not_allowed", "Selected-student activities keep their published participant snapshot."));
        var existing = entity.Participants.Select(x => x.StudentEnrollmentId).ToArray();
        var missing = await ActiveStudents(db, entity.ClassSectionId).Where(x => !existing.Contains(x.Id)).Select(x => x.Id).ToArrayAsync(ct);
        var now = CurrentUtc(entity); db.ClassActivityParticipants.AddRange(missing.Select(enrollmentId => new ClassActivityParticipant { Id = Guid.NewGuid(),
            ClassActivityId = id, StudentEnrollmentId = enrollmentId, CreatedAtUtc = now, UpdatedAtUtc = now }));
        AddAudit(db, entity, "StudentsSynced", now, payload: JsonSerializer.Serialize(new { AddedCount = missing.Length })); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(new { AddedCount = missing.Length }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("activities/{id:guid}/participants"), Authorize(Policy = SchoolPermissionPolicies.ActivitiesView)]
    public async Task<IActionResult> Participants(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await Load(db, id, ct); if (entity is null) return NotFound(Failure(404, "activities.not_found", "Class activity was not found."));
        if (!await CanAccessActivity(db, entity, ct)) return Forbid();
        var rows = await db.ClassActivityParticipants.AsNoTracking().Where(x => x.ClassActivityId == id)
            .OrderBy(x => x.StudentEnrollment.Student.FullNameAr).Select(x => new ClassActivityParticipantResponse(x.Id,
                x.StudentEnrollmentId, x.StudentEnrollment.StudentId, x.StudentEnrollment.Student.StudentCode,
                x.StudentEnrollment.Student.FullNameAr, x.StudentEnrollment.Student.FullNameEn, x.Status, x.Score, x.Note,
                x.StudentEnrollment.Status == StudentEnrollmentStatus.Active && x.StudentEnrollment.Student.IsActive,
                x.EvaluatedAtUtc, x.EvaluatedByUser == null ? null : x.EvaluatedByUser.Person.DisplayName)).ToArrayAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<ClassActivityParticipantResponse>>.Success(rows, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("activities/{id:guid}/participants"), Authorize(Policy = SchoolPermissionPolicies.ActivitiesGrade)]
    public async Task<IActionResult> SaveParticipants(Guid id, [FromBody] SaveActivityParticipantsRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await Load(db, id, ct); if (entity is null) return NotFound(Failure(404, "activities.not_found", "Class activity was not found."));
        if (!await CanAccessActivity(db, entity, ct)) return Forbid();
        if (entity.Status != ClassActivityStatus.Published) return Conflict(Failure(409, "activities.not_open_for_evaluation", "Only a published activity accepts participation changes."));
        if (request.Participants is null || request.Participants.Count == 0 || request.Participants.Select(x => x.ParticipantId).Distinct().Count() != request.Participants.Count)
            return BadRequest(Failure(400, "activities.participants_invalid", "Submit one valid change per participant."));
        var rows = entity.Participants.Where(x => request.Participants.Select(p => p.ParticipantId).Contains(x.Id)).ToArray();
        if (rows.Length != request.Participants.Count) return BadRequest(Failure(400, "activities.participant_not_found", "One or more participants do not belong to this activity."));
        foreach (var item in request.Participants)
        {
            if (!ValidEvaluation(entity, item)) return BadRequest(Failure(400, "activities.evaluation_invalid", "Check participation states, scores and notes."));
        }
        var now = CurrentUtc(entity); var userId = CurrentUserId();
        foreach (var item in request.Participants)
        {
            var row = rows.Single(x => x.Id == item.ParticipantId); row.Status = item.Status;
            row.Score = entity.IsGraded && item.Status == ActivityParticipationStatus.Participated ? item.Score : null;
            row.Note = Clean(item.Note, 2000); row.EvaluatedByUserId = userId; row.EvaluatedAtUtc = now; row.UpdatedAtUtc = now;
        }
        AddAudit(db, entity, "EvaluationSaved", now, payload: JsonSerializer.Serialize(new { Count = rows.Length })); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(new { SavedCount = rows.Length }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("activities/{id:guid}/complete"), Authorize(Policy = SchoolPermissionPolicies.ActivitiesGrade)]
    public async Task<IActionResult> Complete(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await Load(db, id, ct); if (entity is null) return NotFound(Failure(404, "activities.not_found", "Class activity was not found."));
        if (!await CanAccessActivity(db, entity, ct)) return Forbid();
        if (entity.Status != ClassActivityStatus.Published) return Conflict(Failure(409, "activities.not_open_for_evaluation", "Only a published activity can be completed."));
        if (!entity.HasRecordedEveryParticipant())
            return Conflict(Failure(409, "activities.evaluation_incomplete", "Record a participation state for every participant first."));
        if (!entity.HasEveryRequiredScore())
            return Conflict(Failure(409, "activities.scores_incomplete", "Enter a score for every participating student first."));
        var now = CurrentUtc(entity); entity.Status = ClassActivityStatus.Completed; entity.CompletedAtUtc = now;
        entity.CompletedByUserId = CurrentUserId(); entity.UpdatedAtUtc = now; AddAudit(db, entity, "Completed", now); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("activities/{id:guid}/reopen"), Authorize(Policy = SchoolPermissionPolicies.ActivitiesReopen)]
    public async Task<IActionResult> Reopen(Guid id, [FromBody] ActivityReasonRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await Load(db, id, ct); if (entity is null) return NotFound(Failure(404, "activities.not_found", "Class activity was not found."));
        if (!await CanAccessActivity(db, entity, ct)) return Forbid();
        if (entity.Status != ClassActivityStatus.Completed) return Conflict(Failure(409, "activities.not_completed", "Only a completed activity can be reopened."));
        if (string.IsNullOrWhiteSpace(request.Reason)) return BadRequest(Failure(400, "activities.reason_required", "Enter a reopen reason."));
        var now = CurrentUtc(entity); entity.Status = ClassActivityStatus.Published; entity.CompletedAtUtc = null;
        entity.CompletedByUserId = null; entity.UpdatedAtUtc = now; AddAudit(db, entity, "Reopened", now, request.Reason); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private async Task<IActionResult?> ValidateAccessAndRequest(SchoolsDbContext db, Guid classSectionId, SaveClassActivityRequest request, CancellationToken ct)
    {
        var section = await db.ClassSections.AsNoTracking().Where(x => x.Id == classSectionId && x.IsActive && !x.IsDeleted)
            .Select(x => new { x.GradeOffering.ProgramAcademicYear.StartDate, x.GradeOffering.ProgramAcademicYear.EndDate }).SingleOrDefaultAsync(ct);
        if (section is null) return NotFound(Failure(404, "activities.class_not_found", "Class section was not found."));
        if (!await CanAccessClass(db, classSectionId, ct)) return Forbid();
        if (!Enum.IsDefined(request.Scope) || !Enum.IsDefined(request.Category) || !Enum.IsDefined(request.AudienceMode) ||
            string.IsNullOrWhiteSpace(request.Title) || request.Title.Trim().Length > 200 || string.IsNullOrWhiteSpace(request.Details) ||
            request.Details.Trim().Length > 6000 || request.ActivityDate == default || request.ActivityDate < section.StartDate || request.ActivityDate > section.EndDate ||
            request.Location?.Trim().Length > 300 || request.EndsAt.HasValue && !request.StartsAt.HasValue ||
            request.StartsAt.HasValue && request.EndsAt.HasValue && request.EndsAt <= request.StartsAt ||
            !ClassActivityRules.HasValidScoreDefinition(request.IsGraded, request.TotalScore) || request.StudentEnrollmentIds is null)
            return BadRequest(Failure(400, "activities.invalid", "Enter valid activity data inside the academic year."));
        if (request.Scope == ActivityScope.Subject)
        {
            if (!request.ClassSectionSubjectId.HasValue) return BadRequest(Failure(400, "activities.subject_required", "Select a subject for this activity."));
            if (!await CanAccessSubject(db, classSectionId, request.ClassSectionSubjectId.Value, ct)) return Forbid();
        }
        else if (request.ClassSectionSubjectId.HasValue)
            return BadRequest(Failure(400, "activities.subject_not_allowed", "A general activity cannot reference a subject."));
        var ids = request.StudentEnrollmentIds.Distinct().ToArray();
        if (ids.Length != request.StudentEnrollmentIds.Count) return BadRequest(Failure(400, "activities.students_duplicate", "Do not select the same student more than once."));
        if (request.AudienceMode == ActivityAudienceMode.SelectedStudents)
        {
            if (ids.Length == 0) return BadRequest(Failure(400, "activities.students_required", "Select at least one student."));
            if (await ActiveStudents(db, classSectionId).CountAsync(x => ids.Contains(x.Id), ct) != ids.Length)
                return BadRequest(Failure(400, "activities.students_invalid", "Select active students from this class only."));
        }
        else if (ids.Length != 0) return BadRequest(Failure(400, "activities.students_not_allowed", "Do not submit student ids for an all-class activity."));
        return null;
    }

    private static void Apply(ClassActivity entity, SaveClassActivityRequest request, DateTimeOffset now)
    {
        entity.ClassSectionSubjectId = request.Scope == ActivityScope.Subject ? request.ClassSectionSubjectId : null;
        entity.Scope = request.Scope; entity.Category = request.Category; entity.Title = request.Title.Trim();
        entity.Details = request.Details.Trim(); entity.ActivityDate = request.ActivityDate; entity.StartsAt = request.StartsAt;
        entity.EndsAt = request.EndsAt; entity.Location = Clean(request.Location, 300); entity.IsGraded = request.IsGraded;
        entity.TotalScore = request.IsGraded ? request.TotalScore : null; entity.AudienceMode = request.AudienceMode; entity.UpdatedAtUtc = now;
    }

    private static void SetAudience(ClassActivity entity, IReadOnlyList<Guid> enrollmentIds)
    {
        if (entity.AudienceMode != ActivityAudienceMode.SelectedStudents) return;
        foreach (var enrollmentId in enrollmentIds) entity.AudienceStudents.Add(new ClassActivityAudienceStudent
            { ClassActivity = entity, StudentEnrollmentId = enrollmentId });
    }

    private static void ReconcileAudience(SchoolsDbContext db, ClassActivity entity, IReadOnlyList<Guid> enrollmentIds)
    {
        var changes = ClassActivityRules.CalculateAudienceChanges(entity.AudienceMode,
            entity.AudienceStudents.Select(x => x.StudentEnrollmentId), enrollmentIds);
        var removedIds = changes.RemovedEnrollmentIds.ToHashSet();
        var removed = entity.AudienceStudents.Where(x => removedIds.Contains(x.StudentEnrollmentId)).ToArray();
        db.ClassActivityAudienceStudents.RemoveRange(removed);
        foreach (var enrollmentId in changes.AddedEnrollmentIds)
            entity.AudienceStudents.Add(new ClassActivityAudienceStudent { ClassActivity = entity, StudentEnrollmentId = enrollmentId });
    }

    private static bool ValidEvaluation(ClassActivity entity, SaveActivityParticipantRequest item)
    {
        if (!Enum.IsDefined(item.Status) || item.Note?.Trim().Length > 2000) return false;
        if (item.Status == ActivityParticipationStatus.Participated && entity.IsGraded)
            return !item.Score.HasValue || item.Score is >= 0 && item.Score <= entity.TotalScore;
        return true;
    }

    private ClassActivityDetailsResponse ToDetails(ClassActivity x, bool hasStudentRosterChanges) => new(x.Id, x.ClassSectionId, x.ClassSectionSubjectId,
        x.ClassSectionSubject?.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
        x.ClassSectionSubject?.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn, x.Scope, x.Category, x.Status,
        x.AudienceMode, x.AudienceStudents.Select(a => a.StudentEnrollmentId).ToArray(), x.Title, x.Details, x.ActivityDate,
        x.StartsAt, x.EndsAt, x.Location, x.IsGraded, x.TotalScore, x.TimeZoneIdSnapshot, x.Participants.Count,
        x.Participants.Count(p => p.Status != ActivityParticipationStatus.NotRecorded),
        x.Participants.Count(p => p.Status == ActivityParticipationStatus.Participated),
        x.Participants.Count(p => p.Status == ActivityParticipationStatus.Excused),
        x.Participants.Where(p => p.Score.HasValue).Select(p => p.Score).DefaultIfEmpty().Average(), hasStudentRosterChanges,
        x.CreatedByUser.Person.DisplayName, x.CreatedAtUtc, x.PublishedAtUtc, x.CompletedAtUtc, x.CancelledAtUtc, x.CancellationReason);

    private static Task<ClassActivity?> Load(SchoolsDbContext db, Guid id, CancellationToken ct) => db.ClassActivities
        .IgnoreQueryFilters()
        .Include(x => x.ClassSectionSubject).ThenInclude(x => x!.GradeSubjectOffering).ThenInclude(x => x.CurriculumGradeSubject).ThenInclude(x => x.Subject)
        .Include(x => x.CreatedByUser).ThenInclude(x => x.Person).Include(x => x.AudienceStudents).Include(x => x.Participants)
        .SingleOrDefaultAsync(x => x.Id == id, ct);
    private async Task<bool> CanAccessActivity(SchoolsDbContext db, ClassActivity entity, CancellationToken ct)
    {
        var admin = await IsAdmin(db, ct);
        if (admin) return true;

        var currentOfferings = await db.ClassSectionTeacherScopes.AsNoTracking()
            .Where(x => x.ClassSectionId == entity.ClassSectionId && x.IsActive && !x.IsDeleted &&
                x.TeacherGradeSubjectScope.IsActive && !x.TeacherGradeSubjectScope.IsDeleted &&
                x.TeacherGradeSubjectScope.TeacherUserId == CurrentUserId())
            .Select(x => x.TeacherGradeSubjectScope.GradeSubjectOfferingId).Distinct().ToArrayAsync(ct);

        return ClassActivityRules.CanAccessExistingActivity(entity.Scope, false,
            entity.ClassSectionSubject?.GradeSubjectOfferingId, currentOfferings);
    }
    private static Task<bool> HasStudentRosterChanges(SchoolsDbContext db, ClassActivity entity, CancellationToken ct) =>
        entity.Status == ClassActivityStatus.Published && entity.AudienceMode == ActivityAudienceMode.AllClass
            ? ActiveStudents(db, entity.ClassSectionId).AnyAsync(enrollment =>
                !db.ClassActivityParticipants.Any(participant => participant.ClassActivityId == entity.Id && participant.StudentEnrollmentId == enrollment.Id), ct)
            : Task.FromResult(false);
    private async Task<bool> CanAccessClass(SchoolsDbContext db, Guid classSectionId, CancellationToken ct) => await IsAdmin(db, ct) ||
        await db.ClassSectionTeacherScopes.AsNoTracking().AnyAsync(x => x.ClassSectionId == classSectionId && x.IsActive && !x.IsDeleted &&
            x.TeacherGradeSubjectScope.IsActive && !x.TeacherGradeSubjectScope.IsDeleted && x.TeacherGradeSubjectScope.TeacherUserId == CurrentUserId(), ct);
    private async Task<bool> CanAccessSubject(SchoolsDbContext db, Guid classSectionId, Guid subjectId, CancellationToken ct)
    {
        var subject = await db.ClassSectionSubjects.AsNoTracking().Where(x => x.Id == subjectId && x.ClassSectionId == classSectionId && x.IsActive && !x.IsDeleted)
            .Select(x => new { x.GradeSubjectOfferingId }).SingleOrDefaultAsync(ct);
        if (subject is null) return false;
        return await IsAdmin(db, ct) || await db.ClassSectionTeacherScopes.AsNoTracking().AnyAsync(x => x.ClassSectionId == classSectionId && x.IsActive && !x.IsDeleted &&
            x.TeacherGradeSubjectScope.IsActive && !x.TeacherGradeSubjectScope.IsDeleted && x.TeacherGradeSubjectScope.TeacherUserId == CurrentUserId() &&
            x.TeacherGradeSubjectScope.GradeSubjectOfferingId == subject.GradeSubjectOfferingId, ct);
    }
    private static IQueryable<StudentEnrollment> ActiveStudents(SchoolsDbContext db, Guid classSectionId) => db.StudentEnrollments
        .Where(x => x.ClassSectionId == classSectionId && x.Status == StudentEnrollmentStatus.Active && x.Student.IsActive);
    private static Task<bool> ClassExists(SchoolsDbContext db, Guid classSectionId, CancellationToken ct) => db.ClassSections.AsNoTracking()
        .AnyAsync(x => x.Id == classSectionId && x.IsActive && !x.IsDeleted, ct);
    private Task<bool> IsAdmin(SchoolsDbContext db, CancellationToken ct) => db.LocalUserRoles.AsNoTracking()
        .AnyAsync(x => x.UserId == CurrentUserId() && x.RoleId == SchoolIdentitySeed.SchoolAdminRoleId && x.Role.IsActive, ct);
    private DateTimeOffset CurrentUtc(ClassActivity entity)
    {
        try { return clock.Now(entity.TimeZoneIdSnapshot).UtcNow; }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException) { return DateTimeOffset.UtcNow; }
    }
    private void AddAudit(SchoolsDbContext db, ClassActivity activity, string action, DateTimeOffset now, string? reason = null, string? payload = null) =>
        db.ClassActivityAudits.Add(new ClassActivityAudit { Id = Guid.NewGuid(), ClassActivity = activity, Action = action,
            ActorUserId = CurrentUserId(), Reason = Clean(reason, 1000), PayloadJson = payload, CreatedAtUtc = now });
    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct) => await dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private Guid CurrentUserId() => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : Guid.Empty;
    private ApiResponse<object?> Failure(int status, string code, string message) => ApiResponse<object?>.Failure(status, code, message, correlationId: HttpContext.TraceIdentifier);
    private static string? Clean(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, max)];
}

public sealed record ActivitySubjectResponse(Guid Id, string NameAr, string NameEn);
public sealed record ActivityStudentResponse(Guid StudentEnrollmentId, Guid StudentId, string StudentCode, string FullNameAr, string FullNameEn);
public sealed record ClassActivityListItemResponse(Guid Id, Guid ClassSectionId, Guid? ClassSectionSubjectId, string? SubjectNameAr,
    string? SubjectNameEn, ActivityScope Scope, ActivityCategory Category, ClassActivityStatus Status, ActivityAudienceMode AudienceMode,
    string Title, DateOnly ActivityDate, TimeOnly? StartsAt, TimeOnly? EndsAt, string? Location, bool IsGraded, decimal? TotalScore,
    int ParticipantCount, int RecordedCount, int ParticipatedCount, int ExcusedCount, decimal? AverageScore, bool HasStudentRosterChanges, string CreatedBy,
    DateTimeOffset CreatedAtUtc, DateTimeOffset? PublishedAtUtc, DateTimeOffset? CompletedAtUtc);
public sealed record ClassActivityDetailsResponse(Guid Id, Guid ClassSectionId, Guid? ClassSectionSubjectId, string? SubjectNameAr,
    string? SubjectNameEn, ActivityScope Scope, ActivityCategory Category, ClassActivityStatus Status, ActivityAudienceMode AudienceMode,
    IReadOnlyList<Guid> StudentEnrollmentIds, string Title, string Details, DateOnly ActivityDate, TimeOnly? StartsAt, TimeOnly? EndsAt,
    string? Location, bool IsGraded, decimal? TotalScore, string TimeZoneId, int ParticipantCount, int RecordedCount,
    int ParticipatedCount, int ExcusedCount, decimal? AverageScore, bool HasStudentRosterChanges, string CreatedBy, DateTimeOffset CreatedAtUtc,
    DateTimeOffset? PublishedAtUtc, DateTimeOffset? CompletedAtUtc, DateTimeOffset? CancelledAtUtc, string? CancellationReason);
public sealed record ClassActivityParticipantResponse(Guid Id, Guid StudentEnrollmentId, Guid StudentId, string StudentCode,
    string FullNameAr, string FullNameEn, ActivityParticipationStatus Status, decimal? Score, string? Note, bool IsEnrollmentActive,
    DateTimeOffset? EvaluatedAtUtc, string? EvaluatedBy);
public sealed record SaveClassActivityRequest(Guid? ClassSectionSubjectId, ActivityScope Scope, ActivityCategory Category, string Title,
    string Details, DateOnly ActivityDate, TimeOnly? StartsAt, TimeOnly? EndsAt, string? Location, bool IsGraded, decimal? TotalScore,
    ActivityAudienceMode AudienceMode, IReadOnlyList<Guid> StudentEnrollmentIds);
public sealed record SaveActivityParticipantsRequest(IReadOnlyList<SaveActivityParticipantRequest> Participants);
public sealed record SaveActivityParticipantRequest(Guid ParticipantId, ActivityParticipationStatus Status, decimal? Score, string? Note);
public sealed record ActivityReasonRequest(string Reason);
