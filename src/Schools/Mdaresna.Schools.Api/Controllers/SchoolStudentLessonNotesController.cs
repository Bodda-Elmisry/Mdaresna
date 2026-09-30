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
public sealed class SchoolStudentLessonNotesController(ISchoolDbContextFactory dbFactory, SchoolClock clock) : ControllerBase
{
    [HttpGet("class-workspace/{classSectionId:guid}/lesson-notes"), Authorize(Policy = SchoolPermissionPolicies.LessonNotesView)]
    public async Task<IActionResult> List(Guid classSectionId, [FromQuery] DateOnly? date, [FromQuery] Guid? slotId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var school = await db.SchoolInformation.AsNoTracking().SingleOrDefaultAsync(ct);
        if (school is null || string.IsNullOrWhiteSpace(school.TimeZoneId))
            return Conflict(Failure(409, "lesson_notes.time_zone_required", "Configure the school time zone first."));
        SchoolLocalNow local; try { local = clock.Now(school.TimeZoneId); }
        catch { return Conflict(Failure(409, "lesson_notes.time_zone_invalid", "The school time zone is invalid.")); }
        var lessonDate = date ?? local.Date;
        var isAdmin = await IsAdmin(db, ct); var userId = CurrentUserId();
        var managedDepartmentIds = isAdmin ? [] : await SchoolDepartmentScope.ManagedDepartmentIdsAsync(User, db, local.Date, ct);
        if (!await CanAccessClass(db, classSectionId, managedDepartmentIds, local.Date, ct)) return Forbid();
        var slotsQuery = db.WeeklyTimetableSlots.AsNoTracking().Where(x => x.ClassSectionId == classSectionId && x.IsActive && !x.IsDeleted &&
            !x.IsBreak && x.DayOfWeek == lessonDate.DayOfWeek && x.ClassSectionSubjectId.HasValue);
        if (!isAdmin) slotsQuery = slotsQuery.Where(x =>
            db.TeacherSubstitutions.Any(s => s.WeeklyTimetableSlotId == x.Id && s.LessonDate == lessonDate && s.IsActive && !s.IsDeleted &&
                (s.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUserId == userId ||
                 s.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUser.DepartmentMemberships.Any(m =>
                    managedDepartmentIds.Contains(m.DepartmentId) && m.IsActive && !m.IsDeleted && m.StartsOn <= local.Date &&
                    (!m.EndsOn.HasValue || m.EndsOn >= local.Date)))) ||
            !db.TeacherSubstitutions.Any(s => s.WeeklyTimetableSlotId == x.Id && s.LessonDate == lessonDate && s.IsActive && !s.IsDeleted) &&
                x.PrimaryTeacherScope != null && (x.PrimaryTeacherScope.TeacherUserId == userId ||
                x.PrimaryTeacherScope.TeacherUser.DepartmentMemberships.Any(m => managedDepartmentIds.Contains(m.DepartmentId) &&
                    m.IsActive && !m.IsDeleted && m.StartsOn <= local.Date && (!m.EndsOn.HasValue || m.EndsOn >= local.Date))));
        var slotRows = await slotsQuery.OrderBy(x => x.SlotNumber).Select(x => new
        {
            x.Id, x.SlotNumber, x.StartsAt, x.EndsAt,
            SubjectNameAr = x.ClassSectionSubject!.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
            SubjectNameEn = x.ClassSectionSubject.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn,
            TeacherName = db.TeacherSubstitutions.Where(s => s.WeeklyTimetableSlotId == x.Id && s.LessonDate == lessonDate && s.IsActive && !s.IsDeleted)
                .Select(s => s.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUser.Person.DisplayName).FirstOrDefault()
                ?? (x.PrimaryTeacherScope == null ? string.Empty : x.PrimaryTeacherScope.TeacherUser.Person.DisplayName),
            IsSubstitution = db.TeacherSubstitutions.Any(s => s.WeeklyTimetableSlotId == x.Id && s.LessonDate == lessonDate && s.IsActive && !s.IsDeleted),
            CanManage = isAdmin || db.TeacherSubstitutions.Any(s => s.WeeklyTimetableSlotId == x.Id && s.LessonDate == lessonDate && s.IsActive && !s.IsDeleted &&
                    s.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUserId == userId) ||
                !db.TeacherSubstitutions.Any(s => s.WeeklyTimetableSlotId == x.Id && s.LessonDate == lessonDate && s.IsActive && !s.IsDeleted) &&
                    x.PrimaryTeacherScope != null && x.PrimaryTeacherScope.TeacherUserId == userId
        }).ToArrayAsync(ct);
        var slots = slotRows.Select(x => new LessonNoteSlotResponse(x.Id, x.SlotNumber, x.StartsAt, x.EndsAt,
            x.SubjectNameAr, x.SubjectNameEn, x.TeacherName, x.IsSubstitution, x.CanManage)).ToArray();
        var selected = slotId.HasValue && slots.Any(x => x.Id == slotId.Value) ? slotId :
            slots.FirstOrDefault(x => lessonDate == local.Date && x.StartsAt <= local.Time && x.EndsAt >= local.Time)?.Id ?? slots.FirstOrDefault()?.Id;
        var students = await db.StudentEnrollments.AsNoTracking().Where(x => x.ClassSectionId == classSectionId && x.Status == StudentEnrollmentStatus.Active &&
                x.EnrollmentDate <= lessonDate && x.Student.IsActive).OrderBy(x => x.Student.FullNameAr)
            .Select(x => new LessonNoteStudentResponse(x.Id, x.StudentId, x.Student.StudentCode, x.Student.FullNameAr, x.Student.FullNameEn)).ToArrayAsync(ct);
        var notes = selected.HasValue ? await db.StudentLessonNotes.AsNoTracking().Where(x => !x.IsDeleted && x.ClassSectionId == classSectionId &&
                x.LessonDate == lessonDate && x.WeeklyTimetableSlotId == selected.Value).OrderByDescending(x => x.CreatedAtUtc)
            .Select(x => new LessonNoteResponse(x.Id, x.StudentEnrollmentId, x.Category, x.NoteText, x.RatingLevel, x.RequiresFollowUp,
                x.Visibility, x.CreatedByUser.Person.DisplayName, x.CreatedAtUtc, x.UpdatedAtUtc, x.CreatedByUserId == userId || isAdmin)).ToArrayAsync(ct) : [];
        return Ok(ApiResponse<LessonNotesWorkspaceResponse>.Success(new(lessonDate, school.TimeZoneId, selected, slots, students, notes),
            correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("class-workspace/{classSectionId:guid}/lesson-notes"), Authorize(Policy = SchoolPermissionPolicies.LessonNotesManage)]
    public async Task<IActionResult> Create(Guid classSectionId, [FromBody] SaveLessonNoteRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var error = Validate(request); if (error is not null) return error;
        var timeZoneId = await db.SchoolInformation.AsNoTracking().Select(x => x.TimeZoneId).SingleOrDefaultAsync(ct);
        if (string.IsNullOrWhiteSpace(timeZoneId))
            return BadRequest(Failure(400, "lesson_notes.time_zone_required", "Configure the school time zone first."));
        DateOnly schoolDate; try { schoolDate = clock.Now(timeZoneId).Date; }
        catch { return BadRequest(Failure(400, "lesson_notes.time_zone_invalid", "The school time zone is invalid.")); }
        if (request.LessonDate > schoolDate)
            return BadRequest(Failure(400, "lesson_notes.date_invalid", "The lesson date cannot be in the future."));
        if (!await CanManageSlot(db, classSectionId, request.WeeklyTimetableSlotId, request.LessonDate, ct)) return Forbid();
        if (!await ValidEnrollment(db, classSectionId, request.StudentEnrollmentId, request.LessonDate, ct))
            return BadRequest(Failure(400, "lesson_notes.student_invalid", "Select an active student in this class."));
        var now = DateTimeOffset.UtcNow; var userId = CurrentUserId();
        var entity = new StudentLessonNote { Id = Guid.NewGuid(), ClassSectionId = classSectionId, StudentEnrollmentId = request.StudentEnrollmentId,
            WeeklyTimetableSlotId = request.WeeklyTimetableSlotId, LessonDate = request.LessonDate, CreatedByUserId = userId, UpdatedByUserId = userId,
            CreatedAtUtc = now, UpdatedAtUtc = now };
        Apply(entity, request); db.StudentLessonNotes.Add(entity); Audit(db, entity, "Created", userId, now); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(new { entity.Id }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("lesson-notes/{id:guid}"), Authorize(Policy = SchoolPermissionPolicies.LessonNotesManage)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLessonNoteRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await db.StudentLessonNotes.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null) return NotFound(Failure(404, "lesson_notes.not_found", "Lesson note was not found."));
        if (!await CanModify(db, entity, ct)) return Forbid();
        var save = new SaveLessonNoteRequest(entity.StudentEnrollmentId, entity.WeeklyTimetableSlotId, entity.LessonDate,
            request.Category, request.NoteText, request.RatingLevel, request.RequiresFollowUp, request.Visibility);
        var error = Validate(save); if (error is not null) return error;
        Apply(entity, save); entity.UpdatedByUserId = CurrentUserId(); entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
        Audit(db, entity, "Updated", CurrentUserId(), entity.UpdatedAtUtc); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpDelete("lesson-notes/{id:guid}"), Authorize(Policy = SchoolPermissionPolicies.LessonNotesManage)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var entity = await db.StudentLessonNotes.SingleOrDefaultAsync(x => x.Id == id && !x.IsDeleted, ct);
        if (entity is null) return NotFound(Failure(404, "lesson_notes.not_found", "Lesson note was not found."));
        if (!await CanModify(db, entity, ct)) return Forbid();
        var now = DateTimeOffset.UtcNow; entity.IsDeleted = true; entity.DeletedAtUtc = now; entity.DeletedByUserId = CurrentUserId(); entity.UpdatedAtUtc = now;
        Audit(db, entity, "Deleted", CurrentUserId(), now); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private IActionResult? Validate(SaveLessonNoteRequest request)
    {
        if (!Enum.IsDefined(request.Category) || !Enum.IsDefined(request.Visibility) || string.IsNullOrWhiteSpace(request.NoteText) ||
            request.NoteText.Trim().Length > 2000 || !StudentLessonNoteRules.IsValidRating(request.RatingLevel))
            return BadRequest(Failure(400, "lesson_notes.invalid", "Enter a valid category, note and rating from 1 to 5."));
        return null;
    }
    private static void Apply(StudentLessonNote entity, SaveLessonNoteRequest request)
    { entity.Category = request.Category; entity.NoteText = request.NoteText.Trim(); entity.RatingLevel = request.RatingLevel;
      entity.RequiresFollowUp = request.RequiresFollowUp; entity.Visibility = request.Visibility; }
    private static void Audit(SchoolsDbContext db, StudentLessonNote entity, string action, Guid actor, DateTimeOffset now) =>
        db.StudentLessonNoteAudits.Add(new StudentLessonNoteAudit { Id = Guid.NewGuid(), StudentLessonNote = entity, Action = action,
            ActorUserId = actor, CreatedAtUtc = now, SnapshotJson = JsonSerializer.Serialize(new { entity.Category, entity.NoteText,
                entity.RatingLevel, entity.RequiresFollowUp, entity.Visibility }) });
    private Task<bool> ValidEnrollment(SchoolsDbContext db, Guid classId, Guid enrollmentId, DateOnly date, CancellationToken ct) =>
        db.StudentEnrollments.AsNoTracking().AnyAsync(x => x.Id == enrollmentId && x.ClassSectionId == classId &&
            x.Status == StudentEnrollmentStatus.Active && x.EnrollmentDate <= date && x.Student.IsActive, ct);
    private async Task<bool> CanModify(SchoolsDbContext db, StudentLessonNote note, CancellationToken ct) =>
        await IsAdmin(db, ct) || note.CreatedByUserId == CurrentUserId() && await CanManageSlot(db, note.ClassSectionId, note.WeeklyTimetableSlotId, note.LessonDate, ct);
    private async Task<bool> CanManageSlot(SchoolsDbContext db, Guid classId, Guid slotId, DateOnly date, CancellationToken ct)
    {
        if (await IsAdmin(db, ct)) return await db.WeeklyTimetableSlots.AsNoTracking().AnyAsync(x => x.Id == slotId && x.ClassSectionId == classId && x.IsActive && !x.IsDeleted && !x.IsBreak && x.DayOfWeek == date.DayOfWeek, ct);
        var userId = CurrentUserId(); var replacement = await db.TeacherSubstitutions.AsNoTracking().Where(x => x.WeeklyTimetableSlotId == slotId && x.LessonDate == date && x.IsActive && !x.IsDeleted)
            .Select(x => (Guid?)x.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUserId).SingleOrDefaultAsync(ct);
        return await db.WeeklyTimetableSlots.AsNoTracking().AnyAsync(x => x.Id == slotId && x.ClassSectionId == classId && x.IsActive && !x.IsDeleted && !x.IsBreak &&
            x.DayOfWeek == date.DayOfWeek && (replacement.HasValue ? replacement == userId : x.PrimaryTeacherScope != null && x.PrimaryTeacherScope.TeacherUserId == userId), ct);
    }
    private async Task<bool> CanAccessClass(SchoolsDbContext db, Guid classId, Guid[] managedDepartmentIds, DateOnly today, CancellationToken ct) =>
        await IsAdmin(db, ct) || await db.ClassSectionTeacherScopes.AsNoTracking().AnyAsync(x => x.ClassSectionId == classId && x.IsActive && !x.IsDeleted &&
            x.TeacherGradeSubjectScope.IsActive && !x.TeacherGradeSubjectScope.IsDeleted &&
            (x.TeacherGradeSubjectScope.TeacherUserId == CurrentUserId() ||
             x.TeacherGradeSubjectScope.TeacherUser.DepartmentMemberships.Any(m => managedDepartmentIds.Contains(m.DepartmentId) &&
                m.IsActive && !m.IsDeleted && m.StartsOn <= today && (!m.EndsOn.HasValue || m.EndsOn >= today))), ct);
    private Task<bool> IsAdmin(SchoolsDbContext db, CancellationToken ct) => db.LocalUserRoles.AsNoTracking().AnyAsync(x =>
        x.UserId == CurrentUserId() && x.RoleId == SchoolIdentitySeed.SchoolAdminRoleId && x.Role.IsActive, ct);
    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct) => await dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private Guid CurrentUserId() => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : Guid.Empty;
    private ApiResponse<object?> Failure(int status, string code, string message) => ApiResponse<object?>.Failure(status, code, message, correlationId: HttpContext.TraceIdentifier);
}

public sealed record LessonNoteSlotResponse(Guid Id, int SlotNumber, TimeOnly StartsAt, TimeOnly EndsAt, string SubjectNameAr, string SubjectNameEn,
    string TeacherName, bool IsSubstitution, bool CanManage);
public sealed record LessonNoteStudentResponse(Guid StudentEnrollmentId, Guid StudentId, string StudentCode, string FullNameAr, string FullNameEn);
public sealed record LessonNoteResponse(Guid Id, Guid StudentEnrollmentId, StudentLessonNoteCategory Category, string NoteText, int? RatingLevel,
    bool RequiresFollowUp, StudentLessonNoteVisibility Visibility, string CreatedBy, DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc, bool CanModify);
public sealed record LessonNotesWorkspaceResponse(DateOnly LessonDate, string TimeZoneId, Guid? SelectedSlotId,
    IReadOnlyList<LessonNoteSlotResponse> Slots, IReadOnlyList<LessonNoteStudentResponse> Students, IReadOnlyList<LessonNoteResponse> Notes);
public sealed record SaveLessonNoteRequest(Guid StudentEnrollmentId, Guid WeeklyTimetableSlotId, DateOnly LessonDate,
    StudentLessonNoteCategory Category, string NoteText, int? RatingLevel, bool RequiresFollowUp, StudentLessonNoteVisibility Visibility);
public sealed record UpdateLessonNoteRequest(StudentLessonNoteCategory Category, string NoteText, int? RatingLevel,
    bool RequiresFollowUp, StudentLessonNoteVisibility Visibility);
