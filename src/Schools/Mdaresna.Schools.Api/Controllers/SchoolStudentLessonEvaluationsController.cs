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
public sealed class SchoolStudentLessonEvaluationsController(ISchoolDbContextFactory dbFactory, SchoolClock clock) : ControllerBase
{
    [HttpGet("class-workspace/{classSectionId:guid}/lesson-evaluations"), Authorize(Policy = SchoolPermissionPolicies.LessonEvaluationsView)]
    public async Task<IActionResult> Get(Guid classSectionId, [FromQuery] DateOnly? date, [FromQuery] Guid? slotId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var local = await LocalNow(db, ct); if (local.Error is not null) return local.Error;
        var localNow = local.Now!;
        var lessonDate = date ?? localNow.Date; var userId = CurrentUserId(); var isAdmin = await IsAdmin(db, ct);
        var managedIds = isAdmin ? [] : await SchoolDepartmentScope.ManagedDepartmentIdsAsync(User, db, localNow.Date, ct);
        if (!await CanAccessClass(db, classSectionId, managedIds, localNow.Date, ct)) return Forbid();

        var query = db.WeeklyTimetableSlots.AsNoTracking().Where(x => x.ClassSectionId == classSectionId && x.IsActive && !x.IsDeleted &&
            !x.IsBreak && x.DayOfWeek == lessonDate.DayOfWeek && x.ClassSectionSubjectId.HasValue);
        if (!isAdmin) query = query.Where(x =>
            db.TeacherSubstitutions.Any(s => s.WeeklyTimetableSlotId == x.Id && s.LessonDate == lessonDate && s.IsActive && !s.IsDeleted &&
                (s.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUserId == userId ||
                 s.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUser.DepartmentMemberships.Any(m => managedIds.Contains(m.DepartmentId) &&
                    m.IsActive && !m.IsDeleted && m.StartsOn <= localNow.Date && (!m.EndsOn.HasValue || m.EndsOn >= localNow.Date)))) ||
            !db.TeacherSubstitutions.Any(s => s.WeeklyTimetableSlotId == x.Id && s.LessonDate == lessonDate && s.IsActive && !s.IsDeleted) &&
                x.PrimaryTeacherScope != null && (x.PrimaryTeacherScope.TeacherUserId == userId ||
                 x.PrimaryTeacherScope.TeacherUser.DepartmentMemberships.Any(m => managedIds.Contains(m.DepartmentId) && m.IsActive && !m.IsDeleted &&
                    m.StartsOn <= localNow.Date && (!m.EndsOn.HasValue || m.EndsOn >= localNow.Date))));

        var slotRows = await query.OrderBy(x => x.SlotNumber).Select(x => new
        {
            x.Id, x.SlotNumber, x.StartsAt, x.EndsAt,
            SubjectNameAr = x.ClassSectionSubject!.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
            SubjectNameEn = x.ClassSectionSubject.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn,
            TeacherName = db.TeacherSubstitutions.Where(s => s.WeeklyTimetableSlotId == x.Id && s.LessonDate == lessonDate && s.IsActive && !s.IsDeleted)
                .Select(s => s.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUser.Person.DisplayName).FirstOrDefault()
                ?? (x.PrimaryTeacherScope == null ? string.Empty : x.PrimaryTeacherScope.TeacherUser.Person.DisplayName),
            IsSubstitution = db.TeacherSubstitutions.Any(s => s.WeeklyTimetableSlotId == x.Id && s.LessonDate == lessonDate && s.IsActive && !s.IsDeleted),
            IsOwner = isAdmin || db.TeacherSubstitutions.Any(s => s.WeeklyTimetableSlotId == x.Id && s.LessonDate == lessonDate && s.IsActive && !s.IsDeleted &&
                    s.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUserId == userId) ||
                !db.TeacherSubstitutions.Any(s => s.WeeklyTimetableSlotId == x.Id && s.LessonDate == lessonDate && s.IsActive && !s.IsDeleted) &&
                    x.PrimaryTeacherScope != null && x.PrimaryTeacherScope.TeacherUserId == userId
        }).ToArrayAsync(ct);
        var slots = slotRows.Select(x => new LessonEvaluationSlotResponse(x.Id, x.SlotNumber, x.StartsAt, x.EndsAt, x.SubjectNameAr,
            x.SubjectNameEn, x.TeacherName, x.IsSubstitution, x.IsOwner && HasStarted(lessonDate, x.StartsAt, localNow))).ToArray();
        var selectedId = slotId.HasValue && slots.Any(x => x.Id == slotId) ? slotId :
            slots.FirstOrDefault(x => lessonDate == localNow.Date && x.StartsAt <= localNow.Time && x.EndsAt >= localNow.Time)?.Id ?? slots.FirstOrDefault()?.Id;

        var roster = await db.StudentEnrollments.AsNoTracking().Where(x => x.ClassSectionId == classSectionId && x.Status == StudentEnrollmentStatus.Active &&
                x.EnrollmentDate <= lessonDate && x.Student.IsActive).OrderBy(x => x.Student.FullNameAr)
            .Select(x => new EvaluationRosterRow(x.Id, x.StudentId, x.Student.StudentCode, x.Student.FullNameAr, x.Student.FullNameEn)).ToArrayAsync(ct);
        StudentLessonEvaluationRegister? register = null; HashSet<Guid> absentIds = [];
        if (selectedId.HasValue)
        {
            register = await db.StudentLessonEvaluationRegisters.AsNoTracking().Include(x => x.Entries)
                .Include(x => x.EvaluatedByUser).ThenInclude(x => x.Person)
                .SingleOrDefaultAsync(x => x.ClassSectionId == classSectionId && x.LessonDate == lessonDate && x.WeeklyTimetableSlotId == selectedId, ct);
            absentIds = (await AbsentEnrollmentIds(db, classSectionId, selectedId.Value, lessonDate, ct)).ToHashSet();
        }
        var entryMap = register?.Entries.ToDictionary(x => x.StudentEnrollmentId) ?? [];
        var students = roster.Select(x =>
        {
            entryMap.TryGetValue(x.EnrollmentId, out var entry);
            return new LessonEvaluationStudentResponse(x.EnrollmentId, x.StudentId, x.StudentCode, x.FullNameAr, x.FullNameEn,
                absentIds.Contains(x.EnrollmentId), entry?.Status, entry?.FocusRating, entry?.BehaviorRating);
        }).ToArray();
        var selectedSlot = slots.FirstOrDefault(x => x.Id == selectedId);
        var canSubmit = User.HasClaim(SchoolClaimTypes.Permission, "school.lesson_evaluations.manage") && selectedSlot?.CanManage == true &&
            register?.Status != StudentLessonEvaluationRegisterStatus.Finalized;
        var responseRegister = register is null ? null : new LessonEvaluationRegisterResponse(register.Id, register.Status, register.Revision,
            register.EvaluatedAtUtc, register.EvaluatedByUser.Person.DisplayName);
        return Ok(ApiResponse<LessonEvaluationWorkspaceResponse>.Success(new(lessonDate, localNow.TimeZoneId, selectedId, slots, students,
            responseRegister, canSubmit), correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("class-workspace/{classSectionId:guid}/lesson-evaluations"), Authorize(Policy = SchoolPermissionPolicies.LessonEvaluationsManage)]
    public async Task<IActionResult> Submit(Guid classSectionId, [FromBody] SubmitLessonEvaluationRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var local = await LocalNow(db, ct); if (local.Error is not null) return local.Error;
        var localNow = local.Now!;
        if (request.LessonDate > localNow.Date || !await CanManageSlot(db, classSectionId, request.WeeklyTimetableSlotId, request.LessonDate, localNow, ct))
            return Conflict(Failure(409, "lesson_evaluations.not_available", "This lesson cannot be evaluated by the current user now."));
        var roster = await db.StudentEnrollments.AsNoTracking().Where(x => x.ClassSectionId == classSectionId && x.Status == StudentEnrollmentStatus.Active &&
                x.EnrollmentDate <= request.LessonDate && x.Student.IsActive).Select(x => x.Id).ToArrayAsync(ct);
        if (request.Entries is null || request.Entries.Count != roster.Length || request.Entries.Select(x => x.StudentEnrollmentId).Distinct().Count() != roster.Length ||
            !request.Entries.Select(x => x.StudentEnrollmentId).Order().SequenceEqual(roster.Order()))
            return BadRequest(Failure(400, "lesson_evaluations.roster_mismatch", "Submit exactly one evaluation for every active student."));
        var absentIds = (await AbsentEnrollmentIds(db, classSectionId, request.WeeklyTimetableSlotId, request.LessonDate, ct)).ToHashSet();
        foreach (var entry in request.Entries)
        {
            if (!Enum.IsDefined(entry.Status) || !StudentLessonEvaluationRules.IsValidEntry(entry.Status, entry.FocusRating, entry.BehaviorRating) ||
                absentIds.Contains(entry.StudentEnrollmentId) != (entry.Status == StudentLessonEvaluationEntryStatus.Absent))
                return BadRequest(Failure(400, "lesson_evaluations.entry_invalid", "Enter valid focus and behavior ratings, or select not evaluated. Absent students are excluded automatically."));
        }
        var register = await db.StudentLessonEvaluationRegisters.Include(x => x.Entries).SingleOrDefaultAsync(x =>
            x.ClassSectionId == classSectionId && x.LessonDate == request.LessonDate && x.WeeklyTimetableSlotId == request.WeeklyTimetableSlotId, ct);
        if (register?.Status == StudentLessonEvaluationRegisterStatus.Finalized)
            return Conflict(Failure(409, "lesson_evaluations.already_finalized", "This lesson was already evaluated."));
        var now = DateTimeOffset.UtcNow; var userId = CurrentUserId();
        if (register is null)
        {
            register = new StudentLessonEvaluationRegister { Id = Guid.NewGuid(), ClassSectionId = classSectionId,
                WeeklyTimetableSlotId = request.WeeklyTimetableSlotId, LessonDate = request.LessonDate, TimeZoneIdSnapshot = localNow.TimeZoneId,
                CreatedAtUtc = now, Revision = 1 };
            db.StudentLessonEvaluationRegisters.Add(register);
        }
        else db.StudentLessonEvaluationEntries.RemoveRange(register.Entries);
        register.Status = StudentLessonEvaluationRegisterStatus.Finalized; register.EvaluatedByUserId = userId; register.EvaluatedAtUtc = now;
        register.FinalizedByUserId = userId; register.FinalizedAtUtc = now; register.UpdatedAtUtc = now;
        var entries = request.Entries.Select(x => new StudentLessonEvaluationEntry { Id = Guid.NewGuid(), Register = register,
            StudentEnrollmentId = x.StudentEnrollmentId, Status = x.Status, FocusRating = x.FocusRating,
            BehaviorRating = x.BehaviorRating, CreatedAtUtc = now, UpdatedAtUtc = now }).ToArray();
        db.StudentLessonEvaluationEntries.AddRange(entries);
        db.StudentLessonEvaluationAudits.Add(new StudentLessonEvaluationAudit { Id = Guid.NewGuid(), Register = register,
            Action = "Finalized", ActorUserId = userId, SnapshotJson = JsonSerializer.Serialize(request.Entries), CreatedAtUtc = now });
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { return Conflict(Failure(409, "lesson_evaluations.already_finalized", "This lesson was already evaluated.")); }
        return Ok(ApiResponse<object?>.Success(new { register.Id }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("lesson-evaluations/{registerId:guid}/reopen"), Authorize(Policy = SchoolPermissionPolicies.LessonEvaluationsReopen)]
    public async Task<IActionResult> Reopen(Guid registerId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var register = await db.StudentLessonEvaluationRegisters.SingleOrDefaultAsync(x => x.Id == registerId, ct);
        if (register is null) return NotFound(Failure(404, "lesson_evaluations.not_found", "Lesson evaluation was not found."));
        var local = await LocalNow(db, ct); if (local.Error is not null) return local.Error;
        var localNow = local.Now!;
        var isAdmin = await IsAdmin(db, ct);
        var managedIds = isAdmin ? [] : await SchoolDepartmentScope.ManagedDepartmentIdsAsync(User, db, localNow.Date, ct);
        if (!await CanAccessClass(db, register.ClassSectionId, managedIds, localNow.Date, ct)) return Forbid();
        if (register.Status == StudentLessonEvaluationRegisterStatus.Draft)
            return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
        register.Status = StudentLessonEvaluationRegisterStatus.Draft; register.FinalizedAtUtc = null; register.FinalizedByUserId = null;
        register.Revision++; register.UpdatedAtUtc = DateTimeOffset.UtcNow;
        db.StudentLessonEvaluationAudits.Add(new StudentLessonEvaluationAudit { Id = Guid.NewGuid(), RegisterId = register.Id,
            Action = "Reopened", ActorUserId = CurrentUserId(), SnapshotJson = "{}", CreatedAtUtc = register.UpdatedAtUtc });
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private async Task<(SchoolLocalNow? Now, IActionResult? Error)> LocalNow(SchoolsDbContext db, CancellationToken ct)
    {
        var zone = await db.SchoolInformation.AsNoTracking().Select(x => x.TimeZoneId).SingleOrDefaultAsync(ct);
        if (string.IsNullOrWhiteSpace(zone)) return (null, Conflict(Failure(409, "lesson_evaluations.time_zone_required", "Configure the school time zone first.")));
        try { return (clock.Now(zone), null); }
        catch { return (null, Conflict(Failure(409, "lesson_evaluations.time_zone_invalid", "The school time zone is invalid."))); }
    }
    private static bool HasStarted(DateOnly date, TimeOnly startsAt, SchoolLocalNow now) => date < now.Date || date == now.Date && startsAt <= now.Time;
    private async Task<bool> CanManageSlot(SchoolsDbContext db, Guid classId, Guid slotId, DateOnly date, SchoolLocalNow now, CancellationToken ct)
    {
        var slot = await db.WeeklyTimetableSlots.AsNoTracking().Where(x => x.Id == slotId && x.ClassSectionId == classId && x.IsActive && !x.IsDeleted &&
                !x.IsBreak && x.ClassSectionSubjectId.HasValue && x.DayOfWeek == date.DayOfWeek)
            .Select(x => new { x.StartsAt, PrimaryUserId = x.PrimaryTeacherScope == null ? (Guid?)null : x.PrimaryTeacherScope.TeacherUserId }).SingleOrDefaultAsync(ct);
        if (slot is null || !HasStarted(date, slot.StartsAt, now)) return false;
        if (await IsAdmin(db, ct)) return true;
        var replacement = await db.TeacherSubstitutions.AsNoTracking().Where(x => x.WeeklyTimetableSlotId == slotId && x.LessonDate == date && x.IsActive && !x.IsDeleted)
            .Select(x => (Guid?)x.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUserId).FirstOrDefaultAsync(ct);
        return replacement.HasValue ? replacement == CurrentUserId() : slot.PrimaryUserId == CurrentUserId();
    }
    private static Task<Guid[]> AbsentEnrollmentIds(SchoolsDbContext db, Guid classId, Guid slotId, DateOnly date, CancellationToken ct) =>
        db.StudentAttendanceEntries.AsNoTracking().Where(x => x.Register.ClassSectionId == classId && x.Register.AttendanceDate == date &&
            x.Register.Status == StudentAttendanceRegisterStatus.Finalized &&
            (x.Register.WeeklyTimetableSlotId == slotId || x.Register.Mode == StudentAttendanceMode.Daily) &&
            (x.Status == StudentAttendanceStatus.Absent || x.Status == StudentAttendanceStatus.ExcusedAbsent))
            .Select(x => x.StudentEnrollmentId).Distinct().ToArrayAsync(ct);
    private async Task<bool> CanAccessClass(SchoolsDbContext db, Guid classId, Guid[] managedIds, DateOnly today, CancellationToken ct) =>
        await IsAdmin(db, ct) || await db.ClassSectionTeacherScopes.AsNoTracking().AnyAsync(x => x.ClassSectionId == classId && x.IsActive && !x.IsDeleted &&
            x.TeacherGradeSubjectScope.IsActive && !x.TeacherGradeSubjectScope.IsDeleted &&
            (x.TeacherGradeSubjectScope.TeacherUserId == CurrentUserId() || x.TeacherGradeSubjectScope.TeacherUser.DepartmentMemberships.Any(m =>
                managedIds.Contains(m.DepartmentId) && m.IsActive && !m.IsDeleted && m.StartsOn <= today && (!m.EndsOn.HasValue || m.EndsOn >= today))), ct);
    private Task<bool> IsAdmin(SchoolsDbContext db, CancellationToken ct) => db.LocalUserRoles.AsNoTracking().AnyAsync(x =>
        x.UserId == CurrentUserId() && x.RoleId == SchoolIdentitySeed.SchoolAdminRoleId && x.Role.IsActive, ct);
    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct) => await dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private Guid CurrentUserId() => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : Guid.Empty;
    private ApiResponse<object?> Failure(int status, string code, string message) => ApiResponse<object?>.Failure(status, code, message, correlationId: HttpContext.TraceIdentifier);
}

public sealed record LessonEvaluationSlotResponse(Guid Id, int SlotNumber, TimeOnly StartsAt, TimeOnly EndsAt, string SubjectNameAr,
    string SubjectNameEn, string TeacherName, bool IsSubstitution, bool CanManage);
public sealed record LessonEvaluationStudentResponse(Guid StudentEnrollmentId, Guid StudentId, string StudentCode, string FullNameAr,
    string FullNameEn, bool IsAbsent, StudentLessonEvaluationEntryStatus? Status, int? FocusRating, int? BehaviorRating);
public sealed record LessonEvaluationRegisterResponse(Guid Id, StudentLessonEvaluationRegisterStatus Status, int Revision,
    DateTimeOffset EvaluatedAtUtc, string EvaluatedBy);
public sealed record LessonEvaluationWorkspaceResponse(DateOnly LessonDate, string TimeZoneId, Guid? SelectedSlotId,
    IReadOnlyList<LessonEvaluationSlotResponse> Slots, IReadOnlyList<LessonEvaluationStudentResponse> Students,
    LessonEvaluationRegisterResponse? Register, bool CanSubmit);
public sealed record SubmitLessonEvaluationRequest(Guid WeeklyTimetableSlotId, DateOnly LessonDate,
    IReadOnlyList<SubmitLessonEvaluationEntryRequest> Entries);
public sealed record SubmitLessonEvaluationEntryRequest(Guid StudentEnrollmentId, StudentLessonEvaluationEntryStatus Status,
    int? FocusRating, int? BehaviorRating);
file sealed record EvaluationRosterRow(Guid EnrollmentId, Guid StudentId, string StudentCode, string FullNameAr, string FullNameEn);
