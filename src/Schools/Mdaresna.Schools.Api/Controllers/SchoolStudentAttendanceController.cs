using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Time;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.Students;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController, Authorize, Route("api/schools/v1/attendance")]
public sealed class SchoolStudentAttendanceController(ISchoolDbContextFactory dbFactory, SchoolClock clock) : ControllerBase
{
    [HttpGet("settings"), Authorize(Policy = SchoolPermissionPolicies.OperationsView)]
    public async Task<IActionResult> GetSettings(CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var school = await db.SchoolInformation.AsNoTracking().SingleOrDefaultAsync(ct);
        if (school is null) return NotFound(Failure(404, "attendance.school_not_found", "School information was not found."));
        var programs = await db.EducationPrograms.AsNoTracking().OrderBy(x => x.NameAr)
            .Select(x => new AttendanceProgramSetting(x.Id, x.NameAr, x.NameEn, x.StudentAttendanceModeOverride)).ToArrayAsync(ct);
        var stages = await db.EducationStages.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.NameAr)
            .Select(x => new AttendanceStageSetting(x.Id, x.EducationProgramId, x.NameAr, x.NameEn, x.StudentAttendanceModeOverride)).ToArrayAsync(ct);
        return Ok(ApiResponse<AttendanceSettingsResponse>.Success(new(school.TimeZoneId,
            school.DefaultStudentAttendanceMode, programs, stages, SchoolClock.GetTimeZones()), correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("settings"), Authorize(Policy = SchoolPermissionPolicies.OperationsManage)]
    public async Task<IActionResult> SaveSettings([FromBody] SaveAttendanceSettingsRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (request.ProgramOverrides is null || request.StageOverrides is null)
            return BadRequest(Failure(400, "attendance.settings_invalid", "Attendance settings are invalid."));
        try { _ = SchoolClock.Resolve(request.TimeZoneId); }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        { return BadRequest(Failure(400, "attendance.time_zone_invalid", "Select a valid IANA time zone.")); }

        var programIds = request.ProgramOverrides.Select(x => x.Id).ToArray();
        var stageIds = request.StageOverrides.Select(x => x.Id).ToArray();
        if (programIds.Length != programIds.Distinct().Count() || stageIds.Length != stageIds.Distinct().Count())
            return BadRequest(Failure(400, "attendance.settings_invalid", "Attendance settings are invalid."));
        var programs = await db.EducationPrograms.Where(x => programIds.Contains(x.Id)).ToArrayAsync(ct);
        var stages = await db.EducationStages.Where(x => stageIds.Contains(x.Id)).ToArrayAsync(ct);
        if (programs.Length != programIds.Length || stages.Length != stageIds.Length)
            return BadRequest(Failure(400, "attendance.settings_invalid", "Attendance settings are invalid."));
        var now = DateTimeOffset.UtcNow;
        var school = await db.SchoolInformation.SingleOrDefaultAsync(ct);
        if (school is null) return NotFound(Failure(404, "attendance.school_not_found", "School information was not found."));
        school.ConfigureStudentAttendance(request.TimeZoneId, request.DefaultMode, now);
        foreach (var item in request.ProgramOverrides)
        {
            var entity = programs.Single(x => x.Id == item.Id); entity.StudentAttendanceModeOverride = item.Mode; entity.UpdatedAtUtc = now;
        }
        foreach (var item in request.StageOverrides)
        {
            var entity = stages.Single(x => x.Id == item.Id); entity.StudentAttendanceModeOverride = item.Mode; entity.UpdatedAtUtc = now;
        }
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("class-sections/{classSectionId:guid}"), Authorize(Policy = SchoolPermissionPolicies.AttendanceView)]
    public async Task<IActionResult> GetClassAttendance(Guid classSectionId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var context = await BuildContext(db, classSectionId, ct);
        if (context.Error is not null) return context.Error;
        var roster = await LoadRoster(db, classSectionId, context.Now!.Date, ct);
        var register = context.UnitKey is null ? null : await db.StudentAttendanceRegisters.AsNoTracking()
            .Include(x => x.Entries).Include(x => x.RecordedByUser).ThenInclude(x => x.Person)
            .SingleOrDefaultAsync(x => x.ClassSectionId == classSectionId && x.AttendanceDate == context.Now.Date && x.UnitKey == context.UnitKey, ct);
        var statuses = register?.Entries.ToDictionary(x => x.StudentEnrollmentId, x => x.Status) ?? [];
        var students = roster.Select(x => new AttendanceStudentResponse(x.EnrollmentId, x.StudentId, x.StudentCode,
            x.FullNameAr, x.FullNameEn, statuses.GetValueOrDefault(x.EnrollmentId, StudentAttendanceStatus.Present))).ToArray();
        var canRecord = context.CanRecord && register?.Status != StudentAttendanceRegisterStatus.Finalized &&
            User.HasClaim(SchoolClaimTypes.Permission, "school.attendance.record");
        return Ok(ApiResponse<ClassAttendanceResponse>.Success(new(context.Class!.Id, context.Class.NameAr, context.Class.NameEn,
            context.Mode, context.Now.TimeZoneId, context.Now.Date, context.Now.Time, context.Slot,
            canRecord, register?.Status == StudentAttendanceRegisterStatus.Finalized, context.DenialCode,
            register is null ? null : new AttendanceRegisterResponse(register.Id, register.Status, register.RecordedAtUtc,
                register.RecordedByUser.Person.DisplayName), students), correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPost("class-sections/{classSectionId:guid}"), Authorize(Policy = SchoolPermissionPolicies.AttendanceRecord)]
    public async Task<IActionResult> Record(Guid classSectionId, [FromBody] RecordAttendanceRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var context = await BuildContext(db, classSectionId, ct);
        if (context.Error is not null) return context.Error;
        if (!context.CanRecord || context.UnitKey is null)
            return Conflict(Failure(409, context.DenialCode ?? "attendance.not_available", "Attendance cannot be recorded now."));
        var roster = await LoadRoster(db, classSectionId, context.Now!.Date, ct);
        if (request.Entries is null || request.Entries.Count != roster.Length || request.Entries.Select(x => x.StudentEnrollmentId).Distinct().Count() != roster.Length ||
            !request.Entries.Select(x => x.StudentEnrollmentId).Order().SequenceEqual(roster.Select(x => x.EnrollmentId).Order()))
            return BadRequest(Failure(400, "attendance.roster_mismatch", "Submit exactly one attendance status for every active student."));
        var userId = CurrentUserId(); var now = context.Now.UtcNow;
        var register = await db.StudentAttendanceRegisters.Include(x => x.Entries)
            .SingleOrDefaultAsync(x => x.ClassSectionId == classSectionId && x.AttendanceDate == context.Now.Date && x.UnitKey == context.UnitKey, ct);
        if (register?.Status == StudentAttendanceRegisterStatus.Finalized)
            return Conflict(Failure(409, "attendance.already_recorded", "Attendance was already recorded for this unit."));
        if (register is null)
        {
            register = new StudentAttendanceRegister { Id = Guid.NewGuid(), ClassSectionId = classSectionId,
                AttendanceDate = context.Now.Date, UnitKey = context.UnitKey, Mode = context.Mode,
                WeeklyTimetableSlotId = context.Slot?.Id, TimeZoneIdSnapshot = context.Now.TimeZoneId,
                RecordedByUserId = userId, RecordedAtUtc = now, CreatedAtUtc = now, UpdatedAtUtc = now };
            db.StudentAttendanceRegisters.Add(register);
        }
        else
        {
            db.StudentAttendanceEntries.RemoveRange(register.Entries);
            register.RecordedByUserId = userId; register.RecordedAtUtc = now; register.UpdatedAtUtc = now;
        }
        register.Status = StudentAttendanceRegisterStatus.Finalized;
        register.FinalizedByUserId = userId; register.FinalizedAtUtc = now;
        var entries = request.Entries.Select(x => new StudentAttendanceEntry { Id = Guid.NewGuid(), Register = register,
            StudentEnrollmentId = x.StudentEnrollmentId, Status = x.Status, ArrivedAt = x.ArrivedAt,
            LeftAt = x.LeftAt, Notes = Clean(x.Notes, 1000), CreatedAtUtc = now, UpdatedAtUtc = now }).ToArray();
        db.StudentAttendanceEntries.AddRange(entries);
        db.StudentAttendanceAudits.Add(new StudentAttendanceAudit { Id = Guid.NewGuid(), Register = register,
            Action = "Finalized", ActorUserId = userId, SnapshotJson = JsonSerializer.Serialize(request.Entries), CreatedAtUtc = now });
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateException) { return Conflict(Failure(409, "attendance.already_recorded", "Attendance was already recorded for this unit.")); }
        return Ok(ApiResponse<object?>.Success(new { register.Id }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("registers/{registerId:guid}/reopen"), Authorize(Policy = SchoolPermissionPolicies.AttendanceReopen)]
    public async Task<IActionResult> Reopen(Guid registerId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var register = await db.StudentAttendanceRegisters.SingleOrDefaultAsync(x => x.Id == registerId, ct);
        if (register is null) return NotFound(Failure(404, "attendance.register_not_found", "Attendance register was not found."));
        if (!await CanAccessClass(db, register.ClassSectionId, ct)) return Forbid();
        if (register.Status == StudentAttendanceRegisterStatus.Draft)
            return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
        var now = DateTimeOffset.UtcNow; var userId = CurrentUserId();
        register.Status = StudentAttendanceRegisterStatus.Draft; register.FinalizedAtUtc = null; register.FinalizedByUserId = null; register.UpdatedAtUtc = now;
        db.StudentAttendanceAudits.Add(new StudentAttendanceAudit { Id = Guid.NewGuid(), RegisterId = register.Id,
            Action = "Reopened", ActorUserId = userId, CreatedAtUtc = now });
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private async Task<AttendanceContextBuild> BuildContext(SchoolsDbContext db, Guid classSectionId, CancellationToken ct)
    {
        if (!await CanAccessClass(db, classSectionId, ct)) return new(null, null, default, null, null, false, null, Forbid());
        var school = await db.SchoolInformation.AsNoTracking().SingleOrDefaultAsync(ct);
        if (school is null) return new(null, null, default, null, null, false, null, NotFound(Failure(404, "attendance.school_not_found", "School information was not found.")));
        if (string.IsNullOrWhiteSpace(school.TimeZoneId))
            return new(null, null, default, null, null, false, "attendance.time_zone_required", Conflict(Failure(409, "attendance.time_zone_required", "Configure the school time zone first.")));
        SchoolLocalNow now;
        try { now = clock.Now(school.TimeZoneId); }
        catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
        { return new(null, null, default, null, null, false, "attendance.time_zone_invalid", Conflict(Failure(409, "attendance.time_zone_invalid", "The configured school time zone is invalid."))); }
        var section = await db.ClassSections.AsNoTracking().Where(x => x.Id == classSectionId && x.IsActive)
            .Select(x => new AttendanceClassRow(x.Id, x.NameAr, x.NameEn,
                x.GradeOffering.ProgramAcademicYear.EducationProgramId, x.GradeOffering.ProgramAcademicYearId,
                x.GradeOffering.ProgramAcademicYear.StartDate, x.GradeOffering.ProgramAcademicYear.EndDate,
                x.GradeOffering.GradeLevel.EducationStage.StudentAttendanceModeOverride,
                x.GradeOffering.ProgramAcademicYear.EducationProgram.StudentAttendanceModeOverride)).SingleOrDefaultAsync(ct);
        if (section is null) return new(null, now, default, null, null, false, null, NotFound(Failure(404, "attendance.class_not_found", "Class section was not found.")));
        var mode = section.StageMode ?? section.ProgramMode ?? school.DefaultStudentAttendanceMode;
        if (now.Date < section.AcademicYearStartsOn || now.Date > section.AcademicYearEndsOn)
            return new(section, now, mode, null, null, false, "attendance.outside_academic_year", null);
        var closed = await db.SchoolCalendarEvents.AsNoTracking().AnyAsync(x => x.IsActive && x.IsSchoolClosed &&
            x.StartDate <= now.Date && x.EndDate >= now.Date &&
            (x.EducationProgramId == null || x.EducationProgramId == section.ProgramId) &&
            (x.ProgramAcademicYearId == null || x.ProgramAcademicYearId == section.ProgramAcademicYearId), ct);
        if (closed) return new(section, now, mode, null, null, false, "attendance.school_closed", null);
        if (mode == StudentAttendanceMode.Daily)
        {
            var isStudyDay = await db.SchoolDaySchedules.AsNoTracking().AnyAsync(x => x.EducationProgramId == section.ProgramId &&
                x.IsActive && x.DayOfWeek == now.DayOfWeek, ct);
            if (!isStudyDay) return new(section, now, mode, null, null, false, "attendance.not_study_day", null);
            return new(section, now, mode, "DAY", null, true, null, null);
        }
        var slot = await db.WeeklyTimetableSlots.AsNoTracking().Where(x => x.ClassSectionId == classSectionId && x.IsActive &&
                x.DayOfWeek == now.DayOfWeek && x.StartsAt <= now.Time && x.EndsAt >= now.Time)
            .OrderBy(x => x.SlotNumber).Select(x => new AttendanceSlotResponse(x.Id, x.SlotNumber, x.IsBreak,
                x.StartsAt, x.EndsAt, x.ClassSectionSubject == null ? null : x.ClassSectionSubject.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                x.ClassSectionSubject == null ? null : x.ClassSectionSubject.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn)).FirstOrDefaultAsync(ct);
        if (slot is null) return new(section, now, mode, null, null, false, "attendance.no_active_session", null);
        if (slot.IsBreak) return new(section, now, mode, null, slot, false, "attendance.break", null);
        var isAdmin = await IsAdmin(db, ct);
        var isAssignedTeacher = await db.WeeklyTimetableSlots.AsNoTracking().AnyAsync(x => x.Id == slot.Id &&
            x.PrimaryTeacherScope != null && x.PrimaryTeacherScope.TeacherUserId == CurrentUserId(), ct);
        var hasSubstitution = await db.TeacherSubstitutions.AsNoTracking().AnyAsync(x => x.WeeklyTimetableSlotId == slot.Id &&
            x.LessonDate == now.Date && x.IsActive, ct);
        var isSubstitute = await db.TeacherSubstitutions.AsNoTracking().AnyAsync(x => x.WeeklyTimetableSlotId == slot.Id &&
            x.LessonDate == now.Date && x.IsActive && x.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUserId == CurrentUserId(), ct);
        var allowed = isAdmin || (hasSubstitution ? isSubstitute : isAssignedTeacher);
        return new(section, now, mode, $"SLOT:{slot.Id:D}", slot, allowed, allowed ? null : "attendance.not_session_teacher", null);
    }

    private async Task<bool> CanAccessClass(SchoolsDbContext db, Guid classSectionId, CancellationToken ct) =>
        await IsAdmin(db, ct) || await db.ClassSectionTeacherScopes.AsNoTracking().AnyAsync(x => x.ClassSectionId == classSectionId &&
            x.IsActive && x.TeacherGradeSubjectScope.IsActive && x.TeacherGradeSubjectScope.TeacherUserId == CurrentUserId(), ct);
    private Task<bool> IsAdmin(SchoolsDbContext db, CancellationToken ct) => db.LocalUserRoles.AsNoTracking().AnyAsync(x =>
        x.UserId == CurrentUserId() && x.RoleId == SchoolIdentitySeed.SchoolAdminRoleId && x.Role.IsActive, ct);
    private static Task<AttendanceRosterRow[]> LoadRoster(SchoolsDbContext db, Guid classSectionId, DateOnly date, CancellationToken ct) =>
        db.StudentEnrollments.AsNoTracking().Where(x => x.ClassSectionId == classSectionId && x.Status == StudentEnrollmentStatus.Active &&
            x.EnrollmentDate <= date && x.Student.IsActive).OrderBy(x => x.Student.FullNameAr)
            .Select(x => new AttendanceRosterRow(x.Id, x.StudentId, x.Student.StudentCode, x.Student.FullNameAr, x.Student.FullNameEn)).ToArrayAsync(ct);
    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct) =>
        await dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private Guid CurrentUserId() => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : Guid.Empty;
    private ApiResponse<object?> Failure(int status, string code, string message) =>
        ApiResponse<object?>.Failure(status, code, message, correlationId: HttpContext.TraceIdentifier);
    private static string? Clean(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Trim()[..Math.Min(value.Trim().Length, max)];
}

public sealed record AttendanceSettingsResponse(string? TimeZoneId, StudentAttendanceMode DefaultMode,
    IReadOnlyList<AttendanceProgramSetting> Programs, IReadOnlyList<AttendanceStageSetting> Stages,
    IReadOnlyList<SchoolTimeZoneOption> TimeZones);
public sealed record AttendanceProgramSetting(Guid Id, string NameAr, string NameEn, StudentAttendanceMode? Mode);
public sealed record AttendanceStageSetting(Guid Id, Guid EducationProgramId, string NameAr, string NameEn, StudentAttendanceMode? Mode);
public sealed record SaveAttendanceSettingsRequest(string TimeZoneId, StudentAttendanceMode DefaultMode,
    IReadOnlyList<AttendanceOverrideRequest> ProgramOverrides, IReadOnlyList<AttendanceOverrideRequest> StageOverrides);
public sealed record AttendanceOverrideRequest(Guid Id, StudentAttendanceMode? Mode);
public sealed record AttendanceSlotResponse(Guid Id, int SlotNumber, bool IsBreak, TimeOnly StartsAt, TimeOnly EndsAt,
    string? SubjectNameAr, string? SubjectNameEn);
public sealed record AttendanceRegisterResponse(Guid Id, StudentAttendanceRegisterStatus Status, DateTimeOffset RecordedAtUtc, string RecordedBy);
public sealed record AttendanceStudentResponse(Guid StudentEnrollmentId, Guid StudentId, string StudentCode, string FullNameAr,
    string FullNameEn, StudentAttendanceStatus Status);
public sealed record ClassAttendanceResponse(Guid ClassSectionId, string ClassNameAr, string ClassNameEn,
    StudentAttendanceMode Mode, string TimeZoneId, DateOnly SchoolLocalDate, TimeOnly SchoolLocalTime,
    AttendanceSlotResponse? CurrentSlot, bool CanRecord, bool IsFinalized, string? DenialCode,
    AttendanceRegisterResponse? Register, IReadOnlyList<AttendanceStudentResponse> Students);
public sealed record RecordAttendanceRequest(IReadOnlyList<RecordAttendanceEntryRequest> Entries);
public sealed record RecordAttendanceEntryRequest(Guid StudentEnrollmentId, StudentAttendanceStatus Status,
    TimeOnly? ArrivedAt, TimeOnly? LeftAt, string? Notes);
internal sealed record AttendanceClassRow(Guid Id, string NameAr, string NameEn, Guid ProgramId, Guid ProgramAcademicYearId,
    DateOnly AcademicYearStartsOn, DateOnly AcademicYearEndsOn,
    StudentAttendanceMode? StageMode, StudentAttendanceMode? ProgramMode);
internal sealed record AttendanceRosterRow(Guid EnrollmentId, Guid StudentId, string StudentCode, string FullNameAr, string FullNameEn);
internal sealed record AttendanceContextBuild(AttendanceClassRow? Class, SchoolLocalNow? Now, StudentAttendanceMode Mode,
    string? UnitKey, AttendanceSlotResponse? Slot, bool CanRecord, string? DenialCode, IActionResult? Error);
