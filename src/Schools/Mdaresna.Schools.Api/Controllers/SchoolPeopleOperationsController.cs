using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController]
[Route("api/schools/v1/people")]
public sealed class SchoolPeopleOperationsController(ISchoolDbContextFactory dbFactory) : ControllerBase
{
    [Authorize(Policy = SchoolPermissionPolicies.PeopleView)]
    [HttpGet("{userId:guid}/absences")]
    public async Task<IActionResult> Absences(Guid userId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await IsVisible(db, userId, ct)) return NotFound(Failure(404, "people.user_not_found", "User was not found."));
        var rows = await db.StaffAbsences.AsNoTracking().Where(x => x.UserId == userId && x.IsActive)
            .OrderByDescending(x => x.StartsOn).ThenByDescending(x => x.CreatedAtUtc)
            .Select(x => new StaffAbsenceResponse(x.Id, x.UserId, x.Type, x.StartsOn, x.EndsOn,
                x.StartsAt, x.EndsAt, x.Notes, x.SourceType, x.SourceReferenceId, x.CreatedAtUtc))
            .ToArrayAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<StaffAbsenceResponse>>.Success(rows, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.PeopleManage)]
    [HttpPost("{userId:guid}/absences")]
    public async Task<IActionResult> CreateAbsence(Guid userId, [FromBody] SaveStaffAbsenceRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await IsVisible(db, userId, ct)) return NotFound(Failure(404, "people.user_not_found", "User was not found."));
        if (request.StartsOn > request.EndsOn || request.EndsOn.DayNumber - request.StartsOn.DayNumber > 366 ||
            request.Notes?.Length > 1000 || request.StartsAt.HasValue != request.EndsAt.HasValue ||
            (request.StartsAt.HasValue && request.StartsAt >= request.EndsAt))
            return BadRequest(Failure(400, "people.absence_invalid", "Enter a valid absence period."));
        var overlappingDates = await db.StaffAbsences.AsNoTracking().Where(x => x.UserId == userId && x.IsActive &&
                x.StartsOn <= request.EndsOn && x.EndsOn >= request.StartsOn)
            .Select(x => new { x.StartsOn, x.EndsOn, x.StartsAt, x.EndsAt }).ToArrayAsync(ct);
        var overlaps = overlappingDates.Any(x => AbsenceWindowsOverlap(x.StartsOn, x.EndsOn, x.StartsAt, x.EndsAt,
            request.StartsOn, request.EndsOn, request.StartsAt, request.EndsAt));
        if (overlaps) return Conflict(Failure(409, "people.absence_overlap", "An absence already overlaps this period."));
        var now = DateTimeOffset.UtcNow;
        var item = new StaffAbsence { Id = Guid.NewGuid(), UserId = userId, Type = request.Type,
            StartsOn = request.StartsOn, EndsOn = request.EndsOn, StartsAt = request.StartsAt,
            EndsAt = request.EndsAt, Notes = request.Notes?.Trim(), CreatedByUserId = CurrentUserId(),
            CreatedAtUtc = now, UpdatedAtUtc = now };
        db.StaffAbsences.Add(item); await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Success(new { item.Id }, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.PeopleManage)]
    [HttpDelete("absences/{absenceId:guid}")]
    public async Task<IActionResult> DeleteAbsence(Guid absenceId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var item = await db.StaffAbsences.SingleOrDefaultAsync(x => x.Id == absenceId && x.IsActive, ct);
        if (item is null || !await IsVisible(db, item.UserId, ct)) return NotFound(Failure(404, "people.absence_not_found", "Absence was not found."));
        var now = DateTimeOffset.UtcNow; item.IsActive = false; item.IsDeleted = true; item.DeletedAtUtc = now;
        item.DeletedByUserId = CurrentUserId(); item.UpdatedAtUtc = now;
        var reference = absenceId.ToString("D");
        var substitutions = await db.TeacherSubstitutions.Where(x => x.IsActive && x.SourceType == "StaffAbsence" && x.SourceReferenceId == reference)
            .Include(x => x.SubstituteTeacherAssignment).ThenInclude(x => x.TeacherGradeSubjectScope)
            .Include(x => x.WeeklyTimetableSlot).ThenInclude(x => x.ClassSection)
            .Include(x => x.WeeklyTimetableSlot).ThenInclude(x => x.ClassSectionSubject!).ThenInclude(x => x.GradeSubjectOffering)
                .ThenInclude(x => x.CurriculumGradeSubject).ThenInclude(x => x.Subject)
            .ToListAsync(ct);
        var absentName = await db.LocalUsers.Where(x => x.Id == item.UserId).Select(x => x.Person.DisplayName).SingleAsync(ct);
        foreach (var substitution in substitutions)
        {
            substitution.IsActive = false; substitution.IsDeleted = true; substitution.DeletedAtUtc = now;
            substitution.DeletedByUserId = CurrentUserId(); substitution.UpdatedAtUtc = now;
            AddCoverageNotification(db, substitution.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUserId,
                false, substitution.WeeklyTimetableSlot, substitution.LessonDate, absentName, absenceId, now);
        }
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.PeopleView)]
    [HttpGet("absences/{absenceId:guid}/coverage")]
    public async Task<IActionResult> Coverage(Guid absenceId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var absence = await db.StaffAbsences.AsNoTracking().Where(x => x.Id == absenceId && x.IsActive && x.User.Kind == SchoolUserKind.Teacher)
            .Select(x => new { x.Id, x.UserId, x.StartsOn, x.EndsOn, x.StartsAt, x.EndsAt }).SingleOrDefaultAsync(ct);
        if (absence is null || !await IsVisible(db, absence.UserId, ct)) return NotFound(Failure(404, "people.absence_not_found", "Teacher absence was not found."));
        var dates = Enumerable.Range(0, absence.EndsOn.DayNumber - absence.StartsOn.DayNumber + 1).Select(absence.StartsOn.AddDays).ToArray();
        var days = dates.Select(x => x.DayOfWeek).Distinct().ToArray();
        var slots = await db.WeeklyTimetableSlots.AsNoTracking().Where(x => x.IsActive && !x.IsBreak && x.PrimaryTeacherScope != null &&
                x.PrimaryTeacherScope.TeacherUserId == absence.UserId && days.Contains(x.DayOfWeek) && x.ClassSectionSubject != null)
            .Include(x => x.SubstituteTeachers).ThenInclude(x => x.TeacherGradeSubjectScope).ThenInclude(x => x.TeacherUser).ThenInclude(x => x.Person)
            .Include(x => x.ClassSection).Include(x => x.ClassSectionSubject!).ThenInclude(x => x.GradeSubjectOffering)
                .ThenInclude(x => x.CurriculumGradeSubject).ThenInclude(x => x.Subject)
            .OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartsAt).ToListAsync(ct);
        if (absence.StartsAt.HasValue)
            slots = slots.Where(x => x.EndsAt > absence.StartsAt.Value && x.StartsAt < absence.EndsAt!.Value).ToList();
        var candidateUserIds = slots.SelectMany(x => x.SubstituteTeachers.Where(y => y.IsActive)
                .Select(y => y.TeacherGradeSubjectScope.TeacherUserId)).Distinct().ToArray();
        var candidateAbsences = candidateUserIds.Length == 0
            ? []
            : await db.StaffAbsences.AsNoTracking().Where(x => x.IsActive && candidateUserIds.Contains(x.UserId) &&
                    x.StartsOn <= absence.EndsOn && x.EndsOn >= absence.StartsOn)
                .Select(x => new CandidateAbsence(x.UserId, x.StartsOn, x.EndsOn, x.StartsAt, x.EndsAt)).ToArrayAsync(ct);
        var existing = await db.TeacherSubstitutions.AsNoTracking().Where(x => x.IsActive && x.SourceType == "StaffAbsence" &&
                x.SourceReferenceId == absenceId.ToString("D"))
            .Include(x => x.SubstituteTeacherAssignment).ThenInclude(x => x.TeacherGradeSubjectScope).ThenInclude(x => x.TeacherUser).ThenInclude(x => x.Person)
            .ToListAsync(ct);
        var rows = dates.SelectMany(date => slots.Where(x => x.DayOfWeek == date.DayOfWeek).Select(slot =>
        {
            var selected = existing.FirstOrDefault(x => x.WeeklyTimetableSlotId == slot.Id && x.LessonDate == date);
            return new AbsenceCoverageResponse(slot.Id, date, slot.StartsAt, slot.EndsAt, slot.ClassSection.NameAr, slot.ClassSection.NameEn,
                slot.ClassSectionSubject!.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                slot.ClassSectionSubject.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn,
                selected?.SubstituteTeacherAssignment.TeacherGradeSubjectScopeId,
                selected?.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUser.Person.DisplayName,
                slot.SubstituteTeachers.Where(x => x.IsActive && !candidateAbsences.Any(a => a.UserId == x.TeacherGradeSubjectScope.TeacherUserId &&
                    AbsenceCoversLesson(a.StartsOn, a.EndsOn, a.StartsAt, a.EndsAt, date, slot.StartsAt, slot.EndsAt)))
                    .Select(x => new CoverageTeacherOptionResponse(x.TeacherGradeSubjectScopeId,
                    x.TeacherGradeSubjectScope.TeacherUserId, x.TeacherGradeSubjectScope.TeacherUser.Person.DisplayName)).ToArray());
        })).OrderBy(x => x.LessonDate).ThenBy(x => x.StartsAt).ToArray();
        return Ok(ApiResponse<IReadOnlyList<AbsenceCoverageResponse>>.Success(rows, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.PeopleManage)]
    [HttpPut("absences/{absenceId:guid}/coverage")]
    public async Task<IActionResult> SaveCoverage(Guid absenceId, [FromBody] SaveAbsenceCoverageRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var absence = await db.StaffAbsences.Include(x => x.User).ThenInclude(x => x.Person)
            .SingleOrDefaultAsync(x => x.Id == absenceId && x.IsActive && x.User.Kind == SchoolUserKind.Teacher, ct);
        if (absence is null || !await IsVisible(db, absence.UserId, ct)) return NotFound(Failure(404, "people.absence_not_found", "Teacher absence was not found."));
        var slot = await db.WeeklyTimetableSlots.Include(x => x.PrimaryTeacherScope).Include(x => x.ClassSection)
            .Include(x => x.ClassSectionSubject!).ThenInclude(x => x.GradeSubjectOffering)
                .ThenInclude(x => x.CurriculumGradeSubject).ThenInclude(x => x.Subject)
            .SingleOrDefaultAsync(x => x.Id == request.WeeklyTimetableSlotId && x.IsActive && !x.IsBreak, ct);
        if (slot?.PrimaryTeacherScope?.TeacherUserId != absence.UserId || request.LessonDate < absence.StartsOn ||
            request.LessonDate > absence.EndsOn || slot.DayOfWeek != request.LessonDate.DayOfWeek ||
            (absence.StartsAt.HasValue && (slot.EndsAt <= absence.StartsAt.Value || slot.StartsAt >= absence.EndsAt!.Value)))
            return BadRequest(Failure(400, "people.coverage_invalid", "The lesson does not belong to this absence."));
        var reference = absenceId.ToString("D");
        var existing = await db.TeacherSubstitutions.Include(x => x.SubstituteTeacherAssignment)
                .ThenInclude(x => x.TeacherGradeSubjectScope)
            .SingleOrDefaultAsync(x => x.WeeklyTimetableSlotId == slot.Id && x.LessonDate == request.LessonDate, ct);
        var now = DateTimeOffset.UtcNow;
        var previousTeacherId = existing?.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUserId;
        var previousWasActive = existing?.IsActive == true;
        if (!request.SubstituteTeacherScopeId.HasValue)
        {
            if (existing is not null && previousWasActive)
            {
                existing.IsActive = false; existing.IsDeleted = true; existing.DeletedAtUtc = now; existing.DeletedByUserId = CurrentUserId(); existing.UpdatedAtUtc = now;
                AddCoverageNotification(db, previousTeacherId!.Value, false, slot, request.LessonDate,
                    absence.User.Person.DisplayName, absenceId, now);
                await db.SaveChangesAsync(ct);
            }
            return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
        }
        var scopeId = request.SubstituteTeacherScopeId.Value;
        var validCandidate = await db.WeeklyTimetableSlotSubstituteTeachers.AnyAsync(x => x.WeeklyTimetableSlotId == slot.Id && x.TeacherGradeSubjectScopeId == scopeId && x.IsActive, ct);
        if (!validCandidate) return BadRequest(Failure(400, "people.coverage_teacher_invalid", "Select a configured substitute teacher."));
        var candidateUserId = await db.TeacherGradeSubjectScopes.Where(x => x.Id == scopeId && x.IsActive).Select(x => x.TeacherUserId).SingleAsync(ct);
        var candidateAbsent = await db.StaffAbsences.AnyAsync(x => x.IsActive && x.UserId == candidateUserId &&
            x.StartsOn <= request.LessonDate && x.EndsOn >= request.LessonDate &&
            (!x.StartsAt.HasValue || (x.EndsAt > slot.StartsAt && x.StartsAt < slot.EndsAt)), ct);
        var weeklyConflict = await db.WeeklyTimetableSlots.AnyAsync(x => x.Id != slot.Id && x.IsActive && !x.IsBreak && x.DayOfWeek == request.LessonDate.DayOfWeek &&
            x.StartsAt < slot.EndsAt && x.EndsAt > slot.StartsAt && x.PrimaryTeacherScope != null && x.PrimaryTeacherScope.TeacherUserId == candidateUserId, ct);
        var coverageConflict = await db.TeacherSubstitutions.AnyAsync(x => x.IsActive && x.LessonDate == request.LessonDate &&
            x.WeeklyTimetableSlot.StartsAt < slot.EndsAt && x.WeeklyTimetableSlot.EndsAt > slot.StartsAt &&
            x.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUserId == candidateUserId && x.Id != (existing == null ? Guid.Empty : existing.Id), ct);
        if (candidateAbsent) return Conflict(Failure(409, "people.coverage_teacher_absent", "The substitute teacher is absent at this time."));
        if (weeklyConflict || coverageConflict) return Conflict(Failure(409, "people.coverage_teacher_conflict", "The substitute teacher has another lesson at this time."));
        var assignment = await db.ClassSubjectTeacherAssignments.SingleOrDefaultAsync(x => x.ClassSectionSubjectId == slot.ClassSectionSubjectId &&
            x.TeacherGradeSubjectScopeId == scopeId && x.Role == ClassSubjectTeacherRole.Substitute, ct);
        if (assignment is null) { assignment = new ClassSubjectTeacherAssignment { Id = Guid.NewGuid(), ClassSectionSubjectId = slot.ClassSectionSubjectId!.Value,
            TeacherGradeSubjectScopeId = scopeId, Role = ClassSubjectTeacherRole.Substitute, CreatedAtUtc = now, UpdatedAtUtc = now }; db.ClassSubjectTeacherAssignments.Add(assignment); }
        else { assignment.IsActive = true; assignment.IsDeleted = false; assignment.DeletedAtUtc = null; assignment.UpdatedAtUtc = now; }
        if (existing is null) db.TeacherSubstitutions.Add(new TeacherSubstitution { Id = Guid.NewGuid(), WeeklyTimetableSlotId = slot.Id,
            SubstituteTeacherAssignmentId = assignment.Id, LessonDate = request.LessonDate, Reason = absence.Notes,
            SourceType = "StaffAbsence", SourceReferenceId = reference, CreatedAtUtc = now, UpdatedAtUtc = now });
        else { existing.SubstituteTeacherAssignmentId = assignment.Id; existing.Reason = absence.Notes; existing.SourceType = "StaffAbsence";
            existing.SourceReferenceId = reference; existing.IsActive = true; existing.IsDeleted = false; existing.DeletedAtUtc = null; existing.UpdatedAtUtc = now; }
        if (previousWasActive && previousTeacherId != candidateUserId)
            AddCoverageNotification(db, previousTeacherId!.Value, false, slot, request.LessonDate,
                absence.User.Person.DisplayName, absenceId, now);
        if (!previousWasActive || previousTeacherId != candidateUserId)
            AddCoverageNotification(db, candidateUserId, true, slot, request.LessonDate,
                absence.User.Person.DisplayName, absenceId, now);
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private async Task<bool> IsVisible(SchoolsDbContext db, Guid userId, CancellationToken ct)
    {
        var query = await SchoolDepartmentScope.ApplyAsync(User, db, db.LocalUsers.AsNoTracking(), ct);
        return await query.AnyAsync(x => x.Id == userId, ct);
    }
    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct) =>
        await dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private Guid CurrentUserId() => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : Guid.Empty;
    private static bool AbsenceWindowsOverlap(DateOnly leftStart, DateOnly leftEnd, TimeOnly? leftStartsAt, TimeOnly? leftEndsAt,
        DateOnly rightStart, DateOnly rightEnd, TimeOnly? rightStartsAt, TimeOnly? rightEndsAt) =>
        leftStart <= rightEnd && leftEnd >= rightStart &&
        (!leftStartsAt.HasValue || !rightStartsAt.HasValue || leftStartsAt < rightEndsAt && rightStartsAt < leftEndsAt);
    private static bool AbsenceCoversLesson(DateOnly startsOn, DateOnly endsOn, TimeOnly? startsAt, TimeOnly? endsAt,
        DateOnly lessonDate, TimeOnly lessonStartsAt, TimeOnly lessonEndsAt) =>
        startsOn <= lessonDate && endsOn >= lessonDate &&
        (!startsAt.HasValue || startsAt < lessonEndsAt && lessonStartsAt < endsAt);
    private static void AddCoverageNotification(SchoolsDbContext db, Guid recipientUserId, bool assigned,
        WeeklyTimetableSlot slot, DateOnly lessonDate, string absentTeacherName, Guid absenceId, DateTimeOffset now)
    {
        var subject = slot.ClassSectionSubject!.GradeSubjectOffering.CurriculumGradeSubject.Subject;
        var arAction = assigned ? "تم تكليفك بتغطية" : "تم إلغاء تكليفك بتغطية";
        var enAction = assigned ? "You were assigned to cover" : "Your coverage was cancelled for";
        db.SchoolUserNotifications.Add(new SchoolUserNotification
        {
            Id = Guid.NewGuid(), RecipientUserId = recipientUserId,
            Type = assigned ? "LessonCoverageAssigned" : "LessonCoverageCancelled",
            TitleAr = assigned ? "تكليف بتغطية حصة" : "إلغاء تغطية حصة",
            TitleEn = assigned ? "Lesson coverage assigned" : "Lesson coverage cancelled",
            BodyAr = $"{arAction} حصة {subject.NameAr} لفصل {slot.ClassSection.NameAr} بدلًا من {absentTeacherName} يوم {lessonDate:yyyy-MM-dd} من {slot.StartsAt:HH\\:mm} إلى {slot.EndsAt:HH\\:mm}.",
            BodyEn = $"{enAction} {subject.NameEn} for {slot.ClassSection.NameEn} instead of {absentTeacherName} on {lessonDate:yyyy-MM-dd} from {slot.StartsAt:HH\\:mm} to {slot.EndsAt:HH\\:mm}.",
            RelatedEntityType = "StaffAbsence", RelatedEntityId = absenceId.ToString("D"), CreatedAtUtc = now, UpdatedAtUtc = now
        });
    }
    private ApiResponse<object?> Failure(int status, string code, string message) =>
        ApiResponse<object?>.Failure(status, code, message, correlationId: HttpContext.TraceIdentifier);
}

public sealed record SaveStaffAbsenceRequest(StaffAbsenceType Type, DateOnly StartsOn, DateOnly EndsOn,
    [property: System.Text.Json.Serialization.JsonConverter(typeof(FlexibleNullableTimeOnlyJsonConverter))] TimeOnly? StartsAt,
    [property: System.Text.Json.Serialization.JsonConverter(typeof(FlexibleNullableTimeOnlyJsonConverter))] TimeOnly? EndsAt,
    string? Notes);
public sealed record StaffAbsenceResponse(Guid Id, Guid UserId, StaffAbsenceType Type, DateOnly StartsOn, DateOnly EndsOn,
    TimeOnly? StartsAt, TimeOnly? EndsAt, string? Notes, string SourceType, string? SourceReferenceId, DateTimeOffset CreatedAtUtc);
public sealed record CoverageTeacherOptionResponse(Guid ScopeId, Guid TeacherUserId, string DisplayName);
public sealed record AbsenceCoverageResponse(Guid WeeklyTimetableSlotId, DateOnly LessonDate, TimeOnly StartsAt, TimeOnly EndsAt,
    string ClassNameAr, string ClassNameEn, string SubjectNameAr, string SubjectNameEn,
    Guid? SubstituteTeacherScopeId, string? SubstituteTeacherName, IReadOnlyList<CoverageTeacherOptionResponse> Candidates);
public sealed record SaveAbsenceCoverageRequest(Guid WeeklyTimetableSlotId, DateOnly LessonDate, Guid? SubstituteTeacherScopeId);
file sealed record CandidateAbsence(Guid UserId, DateOnly StartsOn, DateOnly EndsOn, TimeOnly? StartsAt, TimeOnly? EndsAt);
