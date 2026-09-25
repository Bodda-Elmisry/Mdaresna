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

[ApiController, Route("api/schools/v1/teaching-planning")]
public sealed class SchoolTeachingPlanningController(ISchoolDbContextFactory dbFactory) : ControllerBase
{
    private static readonly DayOfWeek[] SchoolWeekOrder =
        [DayOfWeek.Saturday, DayOfWeek.Sunday, DayOfWeek.Monday, DayOfWeek.Tuesday,
         DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday];

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsView)]
    [HttpGet("teacher-scopes/{teacherId:guid}")]
    public async Task<IActionResult> TeacherScopes(Guid teacherId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var visibleTeachers = await SchoolDepartmentScope.ApplyAsync(User, db,
            db.LocalUsers.AsNoTracking().Where(x => x.Kind == SchoolUserKind.Teacher), ct);
        if (!await visibleTeachers.AnyAsync(x => x.Id == teacherId, ct))
            return NotFound(Failure(404, "teaching.teacher_not_found", "Teacher was not found."));
        var selected = await db.TeacherGradeSubjectScopes.AsNoTracking().Where(x => x.TeacherUserId == teacherId && x.IsActive)
            .Select(x => x.GradeSubjectOfferingId).ToArrayAsync(ct);
        var rows = await db.GradeSubjectOfferings.AsNoTracking()
            .Where(x => x.IsActive && x.Status != GradeSubjectOfferingStatus.Closed && x.GradeOffering.IsActive && x.GradeOffering.Status != GradeOfferingStatus.Closed)
            .OrderByDescending(x => x.GradeOffering.ProgramAcademicYear.StartDate)
            .ThenBy(x => x.GradeOffering.GradeLevel.EducationStage.SortOrder)
            .ThenBy(x => x.GradeOffering.GradeLevel.SortOrder)
            .ThenBy(x => x.CurriculumGradeSubject.SortOrder)
            .Select(x => new TeacherScopeOptionResponse(x.Id,
                x.GradeOffering.ProgramAcademicYearId, x.GradeOffering.ProgramAcademicYear.NameAr, x.GradeOffering.ProgramAcademicYear.NameEn,
                x.GradeOffering.ProgramAcademicYear.EducationProgramId, x.GradeOffering.ProgramAcademicYear.EducationProgram.NameAr, x.GradeOffering.ProgramAcademicYear.EducationProgram.NameEn,
                x.GradeOffering.GradeLevel.EducationStageId, x.GradeOffering.GradeLevel.EducationStage.NameAr, x.GradeOffering.GradeLevel.EducationStage.NameEn,
                x.GradeOffering.GradeLevelId, x.GradeOffering.GradeLevel.NameAr, x.GradeOffering.GradeLevel.NameEn,
                x.CurriculumGradeSubject.SubjectId, x.CurriculumGradeSubject.Subject.NameAr, x.CurriculumGradeSubject.Subject.NameEn,
                selected.Contains(x.Id)))
            .ToListAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<TeacherScopeOptionResponse>>.Success(rows, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsManage)]
    [HttpPut("teacher-scopes/{teacherId:guid}")]
    public async Task<IActionResult> SaveTeacherScopes(Guid teacherId, [FromBody] SaveTeacherScopesRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var visibleTeachers = await SchoolDepartmentScope.ApplyAsync(User, db,
            db.LocalUsers.Where(x => x.Kind == SchoolUserKind.Teacher), ct);
        var teacher = await visibleTeachers.SingleOrDefaultAsync(x => x.Id == teacherId, ct);
        if (teacher is null) return NotFound(Failure(404, "teaching.teacher_not_found", "Teacher was not found."));
        if (teacher.Status is LocalUserStatus.Disabled or LocalUserStatus.Suspended)
            return Conflict(Failure(409, "teaching.teacher_inactive", "The teacher is not active."));
        var ids = request.GradeSubjectOfferingIds.Distinct().ToArray();
        if (ids.Length > 500) return BadRequest(Failure(400, "teaching.scope_too_large", "Too many scope items."));
        var valid = await db.GradeSubjectOfferings.Where(x => ids.Contains(x.Id) && x.IsActive &&
                x.Status != GradeSubjectOfferingStatus.Closed && x.GradeOffering.IsActive && x.GradeOffering.Status != GradeOfferingStatus.Closed)
            .Select(x => x.Id).ToArrayAsync(ct);
        if (valid.Length != ids.Length) return BadRequest(Failure(400, "teaching.scope_invalid", "One or more grade subjects are unavailable."));
        var existing = await db.TeacherGradeSubjectScopes.Where(x => x.TeacherUserId == teacherId).ToListAsync(ct);
        var removing = existing.Where(x => x.IsActive && !ids.Contains(x.GradeSubjectOfferingId)).ToArray();
        if (removing.Length > 0)
        {
            var removingIds = removing.Select(x => x.Id).ToArray();
            if (await db.ClassSubjectTeacherAssignments.AnyAsync(x => removingIds.Contains(x.TeacherGradeSubjectScopeId) && x.IsActive, ct) ||
                await db.ClassSectionTeacherScopes.AnyAsync(x => removingIds.Contains(x.TeacherGradeSubjectScopeId) && x.IsActive, ct) ||
                await db.WeeklyTimetableSlots.AnyAsync(x => x.IsActive && x.PrimaryTeacherScopeId.HasValue && removingIds.Contains(x.PrimaryTeacherScopeId.Value), ct) ||
                await db.WeeklyTimetableSlotSubstituteTeachers.AnyAsync(x => x.IsActive && removingIds.Contains(x.TeacherGradeSubjectScopeId), ct))
                return Conflict(Failure(409, "teaching.scope_in_use", "Remove the teacher from the timetable before removing this scope."));
        }
        var now = DateTimeOffset.UtcNow;
        foreach (var item in removing) { item.IsActive = false; item.UpdatedAtUtc = now; }
        foreach (var id in ids)
        {
            var item = existing.FirstOrDefault(x => x.GradeSubjectOfferingId == id);
            if (item is null) db.TeacherGradeSubjectScopes.Add(new TeacherGradeSubjectScope { Id = Guid.NewGuid(), TeacherUserId = teacherId, GradeSubjectOfferingId = id, CreatedAtUtc = now, UpdatedAtUtc = now });
            else { item.IsActive = true; item.UpdatedAtUtc = now; }
        }
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsView)]
    [HttpGet("class-sections/{classSectionId:guid}/teacher-options")]
    public async Task<IActionResult> ClassTeacherOptions(Guid classSectionId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var section = await db.ClassSections.AsNoTracking()
            .Where(x => x.Id == classSectionId && x.IsActive && x.GradeOffering.Status != GradeOfferingStatus.Closed)
            .Select(x => new { x.Id, x.NameAr, x.NameEn, x.GradeOfferingId,
                GradeNameAr = x.GradeOffering.GradeLevel.NameAr, GradeNameEn = x.GradeOffering.GradeLevel.NameEn,
                AcademicYearNameAr = x.GradeOffering.ProgramAcademicYear.NameAr,
                AcademicYearNameEn = x.GradeOffering.ProgramAcademicYear.NameEn })
            .SingleOrDefaultAsync(ct);
        if (section is null) return NotFound(Failure(404, "teaching.section_not_found", "Class section was not found."));

        var offerings = await db.GradeSubjectOfferings.AsNoTracking()
            .Where(x => x.GradeOfferingId == section.GradeOfferingId && x.IsActive && x.Status != GradeSubjectOfferingStatus.Closed)
            .OrderBy(x => x.CurriculumGradeSubject.SortOrder)
            .Select(x => new { x.Id, x.CurriculumGradeSubject.Subject.NameAr, x.CurriculumGradeSubject.Subject.NameEn })
            .ToListAsync(ct);
        var offeringIds = offerings.Select(x => x.Id).ToArray();
        var selected = await db.ClassSectionTeacherScopes.AsNoTracking()
            .Where(x => x.ClassSectionId == classSectionId && x.IsActive)
            .Select(x => x.TeacherGradeSubjectScopeId).ToListAsync(ct);
        var assigned = await db.ClassSubjectTeacherAssignments.AsNoTracking()
            .Where(x => x.IsActive && x.ClassSectionSubject.ClassSectionId == classSectionId)
            .Select(x => x.TeacherGradeSubjectScopeId).ToListAsync(ct);
        assigned.AddRange(await db.WeeklyTimetableSlots.AsNoTracking()
            .Where(x => x.IsActive && x.ClassSectionId == classSectionId && x.PrimaryTeacherScopeId.HasValue)
            .Select(x => x.PrimaryTeacherScopeId!.Value).ToListAsync(ct));
        assigned.AddRange(await db.WeeklyTimetableSlotSubstituteTeachers.AsNoTracking()
            .Where(x => x.IsActive && x.WeeklyTimetableSlot.ClassSectionId == classSectionId)
            .Select(x => x.TeacherGradeSubjectScopeId).ToListAsync(ct));
        var selectedIds = selected.Concat(assigned).ToHashSet();
        var candidates = await db.TeacherGradeSubjectScopes.AsNoTracking()
            .Where(x => offeringIds.Contains(x.GradeSubjectOfferingId) && x.IsActive &&
                x.TeacherUser.Kind == SchoolUserKind.Teacher &&
                (x.TeacherUser.Status == LocalUserStatus.Active || x.TeacherUser.Status == LocalUserStatus.PendingActivation))
            .OrderBy(x => x.TeacherUser.Person.DisplayName)
            .Select(x => new ClassTeacherCandidateResponse(x.Id, x.TeacherUserId, x.GradeSubjectOfferingId,
                x.TeacherUser.Person.DisplayName, x.TeacherUser.UserName, selectedIds.Contains(x.Id)))
            .ToListAsync(ct);
        var subjects = offerings.Select(x => new ClassTeacherSubjectOptionsResponse(x.Id, x.NameAr, x.NameEn,
            candidates.Where(c => c.GradeSubjectOfferingId == x.Id).ToArray())).ToArray();
        return Ok(ApiResponse<ClassTeacherOptionsResponse>.Success(new(section.Id, section.NameAr, section.NameEn,
            section.GradeNameAr, section.GradeNameEn, section.AcademicYearNameAr, section.AcademicYearNameEn, subjects),
            correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsManage)]
    [HttpPut("class-sections/{classSectionId:guid}/teacher-scopes")]
    public async Task<IActionResult> SaveClassTeacherScopes(Guid classSectionId,
        [FromBody] SaveClassTeacherScopesRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var section = await db.ClassSections.AsNoTracking().SingleOrDefaultAsync(x => x.Id == classSectionId && x.IsActive && x.GradeOffering.Status != GradeOfferingStatus.Closed, ct);
        if (section is null) return NotFound(Failure(404, "teaching.section_not_found", "Class section was not found."));
        var ids = request.TeacherGradeSubjectScopeIds.Distinct().ToArray();
        if (ids.Length > 500) return BadRequest(Failure(400, "teaching.class_scope_too_large", "Too many class teacher selections."));
        var valid = await db.TeacherGradeSubjectScopes.AsNoTracking()
            .Where(x => ids.Contains(x.Id) && x.IsActive && x.GradeSubjectOffering.GradeOfferingId == section.GradeOfferingId &&
                x.GradeSubjectOffering.IsActive && x.GradeSubjectOffering.Status != GradeSubjectOfferingStatus.Closed &&
                x.TeacherUser.Kind == SchoolUserKind.Teacher &&
                (x.TeacherUser.Status == LocalUserStatus.Active || x.TeacherUser.Status == LocalUserStatus.PendingActivation))
            .Select(x => x.Id).ToArrayAsync(ct);
        if (valid.Length != ids.Length)
            return BadRequest(Failure(400, "teaching.class_scope_invalid", "One or more teachers are unavailable for this class subject."));

        var existing = await db.ClassSectionTeacherScopes.Where(x => x.ClassSectionId == classSectionId).ToListAsync(ct);
        var removing = existing.Where(x => x.IsActive && !ids.Contains(x.TeacherGradeSubjectScopeId)).ToArray();
        if (removing.Length > 0)
        {
            var removingIds = removing.Select(x => x.TeacherGradeSubjectScopeId).ToArray();
            if (await db.ClassSubjectTeacherAssignments.AnyAsync(x => x.IsActive &&
                    x.ClassSectionSubject.ClassSectionId == classSectionId && removingIds.Contains(x.TeacherGradeSubjectScopeId), ct) ||
                await db.WeeklyTimetableSlots.AnyAsync(x => x.IsActive && x.ClassSectionId == classSectionId &&
                    x.PrimaryTeacherScopeId.HasValue && removingIds.Contains(x.PrimaryTeacherScopeId.Value), ct) ||
                await db.WeeklyTimetableSlotSubstituteTeachers.AnyAsync(x => x.IsActive &&
                    x.WeeklyTimetableSlot.ClassSectionId == classSectionId && removingIds.Contains(x.TeacherGradeSubjectScopeId), ct))
                return Conflict(Failure(409, "teaching.class_scope_in_use", "Remove the teacher from this class timetable first."));
        }
        var now = DateTimeOffset.UtcNow;
        foreach (var item in removing) { item.IsActive = false; item.IsDeleted = true; item.DeletedAtUtc = now; item.UpdatedAtUtc = now; }
        foreach (var id in ids)
        {
            var item = existing.FirstOrDefault(x => x.TeacherGradeSubjectScopeId == id);
            if (item is null) db.ClassSectionTeacherScopes.Add(new ClassSectionTeacherScope { Id = Guid.NewGuid(), ClassSectionId = classSectionId,
                TeacherGradeSubjectScopeId = id, CreatedAtUtc = now, UpdatedAtUtc = now });
            else { item.IsActive = true; item.IsDeleted = false; item.DeletedAtUtc = null; item.DeletedByUserId = null; item.UpdatedAtUtc = now; }
        }
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsView)]
    [HttpGet("timetable/sections")]
    public async Task<IActionResult> Sections(CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var rows = await db.ClassSections.AsNoTracking().Where(x => x.IsActive && x.GradeOffering.Status != GradeOfferingStatus.Closed)
            .OrderByDescending(x => x.GradeOffering.ProgramAcademicYear.StartDate).ThenBy(x => x.GradeOffering.GradeLevel.SortOrder).ThenBy(x => x.NameAr)
            .Select(x => new TimetableSectionResponse(x.Id, x.NameAr, x.NameEn,
                x.GradeOffering.GradeLevelId, x.GradeOffering.GradeLevel.NameAr, x.GradeOffering.GradeLevel.NameEn,
                x.GradeOffering.GradeLevel.EducationStageId, x.GradeOffering.GradeLevel.EducationStage.NameAr,
                x.GradeOffering.GradeLevel.EducationStage.NameEn, x.GradeOffering.GradeLevel.EducationStage.SortOrder,
                x.GradeOffering.ProgramAcademicYear.EducationProgramId,
                x.GradeOffering.ProgramAcademicYear.EducationProgram.NameAr,
                x.GradeOffering.ProgramAcademicYear.EducationProgram.NameEn,
                x.GradeOffering.ProgramAcademicYearId, x.GradeOffering.ProgramAcademicYear.NameAr,
                x.GradeOffering.ProgramAcademicYear.NameEn,
                x.GradeOffering.GradeLevel.EducationStage.DailyLessonCount,
                x.GradeOffering.GradeLevel.EducationStage.DailyBreakCount)).ToListAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<TimetableSectionResponse>>.Success(rows, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsView)]
    [HttpGet("timetable/{classSectionId:guid}")]
    public async Task<IActionResult> Timetable(Guid classSectionId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var section = await db.ClassSections.AsNoTracking().Where(x => x.Id == classSectionId && x.IsActive)
            .Select(x => new
            {
                x.Id, x.NameAr, x.NameEn, x.GradeOfferingId,
                GradeNameAr = x.GradeOffering.GradeLevel.NameAr,
                GradeNameEn = x.GradeOffering.GradeLevel.NameEn,
                StageNameAr = x.GradeOffering.GradeLevel.EducationStage.NameAr,
                StageNameEn = x.GradeOffering.GradeLevel.EducationStage.NameEn,
                ProgramNameAr = x.GradeOffering.ProgramAcademicYear.EducationProgram.NameAr,
                ProgramNameEn = x.GradeOffering.ProgramAcademicYear.EducationProgram.NameEn,
                ProgramId = x.GradeOffering.ProgramAcademicYear.EducationProgramId,
                AcademicYearNameAr = x.GradeOffering.ProgramAcademicYear.NameAr,
                AcademicYearNameEn = x.GradeOffering.ProgramAcademicYear.NameEn,
                AcademicYearStart = x.GradeOffering.ProgramAcademicYear.StartDate,
                AcademicYearEnd = x.GradeOffering.ProgramAcademicYear.EndDate,
                x.GradeOffering.GradeLevel.EducationStage.DailyLessonCount,
                x.GradeOffering.GradeLevel.EducationStage.DailyBreakCount
            }).SingleOrDefaultAsync(ct);
        if (section is null) return NotFound(Failure(404, "teaching.section_not_found", "Class section was not found."));
        if (section.DailyLessonCount < 1 || section.DailyLessonCount + section.DailyBreakCount > 24)
            return Conflict(Failure(409, "teaching.stage_schedule_required", "Configure the daily lessons and breaks for this education stage first."));
        var offerings = await db.GradeSubjectOfferings.AsNoTracking().Where(x => x.GradeOfferingId == section.GradeOfferingId && x.IsActive && x.Status != GradeSubjectOfferingStatus.Closed)
            .OrderBy(x => x.CurriculumGradeSubject.SortOrder).Select(x => new { x.Id, x.CurriculumGradeSubject.WeeklyPeriods, x.CurriculumGradeSubject.Subject.NameAr, x.CurriculumGradeSubject.Subject.NameEn }).ToListAsync(ct);
        var offeringIds = offerings.Select(x => x.Id).ToArray();
        var memberScopeIds = await db.ClassSectionTeacherScopes.AsNoTracking().Where(x => x.ClassSectionId == classSectionId && x.IsActive)
            .Select(x => x.TeacherGradeSubjectScopeId).ToListAsync(ct);
        memberScopeIds.AddRange(await db.WeeklyTimetableSlots.AsNoTracking().Where(x => x.ClassSectionId == classSectionId && x.IsActive && x.PrimaryTeacherScopeId.HasValue)
            .Select(x => x.PrimaryTeacherScopeId!.Value).ToListAsync(ct));
        memberScopeIds.AddRange(await db.WeeklyTimetableSlotSubstituteTeachers.AsNoTracking().Where(x => x.WeeklyTimetableSlot.ClassSectionId == classSectionId && x.IsActive)
            .Select(x => x.TeacherGradeSubjectScopeId).ToListAsync(ct));
        var allowedScopeIds = memberScopeIds.Distinct().ToArray();
        var scopeOptions = await db.TeacherGradeSubjectScopes.AsNoTracking().Where(x => allowedScopeIds.Contains(x.Id) && offeringIds.Contains(x.GradeSubjectOfferingId) && x.IsActive &&
                x.TeacherUser.Kind == SchoolUserKind.Teacher && x.TeacherUser.Status != LocalUserStatus.Disabled && x.TeacherUser.Status != LocalUserStatus.Suspended)
            .OrderBy(x => x.TeacherUser.Person.DisplayName)
            .Select(x => new TeacherScopeChoiceResponse(x.Id, x.GradeSubjectOfferingId, x.TeacherUserId, x.TeacherUser.Person.DisplayName)).ToListAsync(ct);
        var slots = await db.WeeklyTimetableSlots.AsNoTracking().Where(x => x.ClassSectionId == classSectionId && x.IsActive)
            .Include(x => x.ClassSectionSubject).Include(x => x.SubstituteTeachers)
            .OrderBy(x => x.DayOfWeek).ThenBy(x => x.SlotNumber).ToListAsync(ct);
        var defaultRoomId = await db.ClassRoomAssignments.AsNoTracking()
            .Where(x => x.ClassSectionId == classSectionId && x.IsActive && x.IsPrimary && x.Room.IsActive && x.Room.IsSchedulable &&
                x.EffectiveFrom <= section.AcademicYearEnd && x.EffectiveTo >= section.AcademicYearStart)
            .OrderByDescending(x => x.EffectiveFrom).Select(x => (Guid?)x.RoomId).FirstOrDefaultAsync(ct);
        var rooms = await db.SchoolRooms.AsNoTracking().Where(x => x.IsActive && x.IsSchedulable)
            .OrderBy(x => x.NameAr).Select(x => new TimetableRoomChoiceResponse(x.Id, x.NameAr, x.NameEn, x.Capacity,
                x.RoomType.NameAr, x.RoomType.NameEn)).ToArrayAsync(ct);
        var days = await StudyDays(db, section.ProgramId, ct);
        var subjects = offerings.Select(x => new TimetableSubjectResponse(x.Id, x.NameAr, x.NameEn, x.WeeklyPeriods)).ToArray();
        var cells = slots.Select(x => new TimetableCellResponse(x.Id, x.DayOfWeek.ToString(), x.SlotNumber, x.IsBreak,
            x.ClassSectionSubject?.GradeSubjectOfferingId, x.PrimaryTeacherScopeId,
            x.SubstituteTeachers.Where(s => s.IsActive).Select(s => s.TeacherGradeSubjectScopeId).ToArray(),
            x.StartsAt.ToString("HH:mm"), x.EndsAt.ToString("HH:mm"), x.RoomId ?? defaultRoomId,
            x.RoomId == null && defaultRoomId.HasValue, x.AllowRoomSharing)).ToArray();
        var context = new TimetableContextResponse(section.Id, section.NameAr, section.NameEn, section.GradeNameAr,
            section.GradeNameEn, section.StageNameAr, section.StageNameEn, section.ProgramNameAr, section.ProgramNameEn,
            section.AcademicYearNameAr, section.AcademicYearNameEn, section.DailyLessonCount, section.DailyBreakCount, defaultRoomId);
        return Ok(ApiResponse<TimetableResponse>.Success(new(context, days.Select(x => x.ToString()).ToArray(), subjects, scopeOptions, rooms, cells), correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize]
    [HttpGet("teachers/{teacherId:guid}/timetable")]
    public async Task<IActionResult> TeacherTimetable(Guid teacherId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var currentUserId = Guid.TryParse(User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value, out var parsedUserId)
            ? parsedUserId : Guid.Empty;
        if (teacherId != currentUserId && !User.HasClaim(SchoolClaimTypes.Permission, "school.users.view")) return Forbid();
        var teacherQuery = db.LocalUsers.AsNoTracking().Where(x => x.Kind == SchoolUserKind.Teacher);
        var visibleTeachers = teacherId == currentUserId ? teacherQuery : await SchoolDepartmentScope.ApplyAsync(User, db, teacherQuery, ct);
        var teacher = await visibleTeachers
            .Where(x => x.Id == teacherId && x.Kind == SchoolUserKind.Teacher)
            .Select(x => new { x.Id, x.Person.DisplayName, x.UserName }).SingleOrDefaultAsync(ct);
        if (teacher is null) return NotFound(Failure(404, "teaching.teacher_not_found", "Teacher was not found."));

        var rows = await db.WeeklyTimetableSlots.AsNoTracking()
            .Where(x => x.IsActive && !x.IsBreak && x.PrimaryTeacherScope != null &&
                x.PrimaryTeacherScope.TeacherUserId == teacherId && x.ClassSectionSubject != null)
            .OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartsAt)
            .Select(x => new
            {
                x.Id, x.DayOfWeek, x.SlotNumber, x.StartsAt, x.EndsAt,
                ClassSectionId = x.ClassSectionId,
                ClassSectionNameAr = x.ClassSection.NameAr, ClassSectionNameEn = x.ClassSection.NameEn,
                GradeNameAr = x.ClassSection.GradeOffering.GradeLevel.NameAr,
                GradeNameEn = x.ClassSection.GradeOffering.GradeLevel.NameEn,
                SubjectNameAr = x.ClassSectionSubject!.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                SubjectNameEn = x.ClassSectionSubject.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn,
                ProgramId = x.ClassSection.GradeOffering.ProgramAcademicYear.EducationProgramId,
                ProgramNameAr = x.ClassSection.GradeOffering.ProgramAcademicYear.EducationProgram.NameAr,
                ProgramNameEn = x.ClassSection.GradeOffering.ProgramAcademicYear.EducationProgram.NameEn,
                AcademicYearNameAr = x.ClassSection.GradeOffering.ProgramAcademicYear.NameAr,
                AcademicYearNameEn = x.ClassSection.GradeOffering.ProgramAcademicYear.NameEn
            }).ToListAsync(ct);
        var programIds = rows.Select(x => x.ProgramId).Distinct().ToArray();
        var configuredDays = programIds.Length == 0 ? [] : await db.SchoolDaySchedules.AsNoTracking()
            .Where(x => x.IsActive && programIds.Contains(x.EducationProgramId)).Select(x => x.DayOfWeek).Distinct().ToArrayAsync(ct);
        var studyDays = configuredDays.Length > 0 ? SchoolWeekOrder.Where(configuredDays.Contains).ToArray() : SchoolWeekOrder;
        var cells = rows.Select(x => new TeacherTimetableCellResponse(x.Id, x.DayOfWeek.ToString(), x.SlotNumber,
            x.StartsAt.ToString("HH:mm"), x.EndsAt.ToString("HH:mm"), x.ClassSectionId, x.ClassSectionNameAr,
            x.ClassSectionNameEn, x.GradeNameAr, x.GradeNameEn, x.SubjectNameAr, x.SubjectNameEn,
            x.ProgramNameAr, x.ProgramNameEn, x.AcademicYearNameAr, x.AcademicYearNameEn)).ToArray();
        var substitutionRows = await db.TeacherSubstitutions.AsNoTracking()
            .Where(x => x.IsActive && x.SubstituteTeacherAssignment.TeacherGradeSubjectScope.TeacherUserId == teacherId)
            .OrderByDescending(x => x.LessonDate).ThenBy(x => x.WeeklyTimetableSlot.StartsAt).Take(100)
            .Select(x => new { x.Id, x.WeeklyTimetableSlotId, x.LessonDate, x.WeeklyTimetableSlot.DayOfWeek,
                x.WeeklyTimetableSlot.StartsAt, x.WeeklyTimetableSlot.EndsAt, x.WeeklyTimetableSlot.ClassSectionId,
                ClassSectionNameAr = x.WeeklyTimetableSlot.ClassSection.NameAr,
                ClassSectionNameEn = x.WeeklyTimetableSlot.ClassSection.NameEn,
                SubjectNameAr = x.WeeklyTimetableSlot.ClassSectionSubject!.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                SubjectNameEn = x.WeeklyTimetableSlot.ClassSectionSubject.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn,
                OriginalTeacherName = x.WeeklyTimetableSlot.PrimaryTeacherScope!.TeacherUser.Person.DisplayName, x.Reason }).ToArrayAsync(ct);
        var substitutions = substitutionRows.Select(x => new TeacherCoverageResponse(x.Id, x.WeeklyTimetableSlotId,
            x.LessonDate, x.DayOfWeek.ToString(), x.StartsAt.ToString("HH:mm"), x.EndsAt.ToString("HH:mm"),
            x.ClassSectionId, x.ClassSectionNameAr, x.ClassSectionNameEn, x.SubjectNameAr, x.SubjectNameEn,
            x.OriginalTeacherName, x.Reason)).ToArray();
        return Ok(ApiResponse<TeacherTimetableResponse>.Success(new(teacher.Id, teacher.DisplayName, teacher.UserName,
            studyDays.Select(x => x.ToString()).ToArray(), cells, substitutions), correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsView)]
    [HttpGet("rooms/options")]
    public async Task<IActionResult> RoomOptions(CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var rows = await db.SchoolRooms.AsNoTracking().Where(x => x.IsActive && x.IsSchedulable)
            .OrderBy(x => x.NameAr).Select(x => new TimetableRoomChoiceResponse(x.Id, x.NameAr, x.NameEn, x.Capacity,
                x.RoomType.NameAr, x.RoomType.NameEn)).ToArrayAsync(ct);
        return Ok(ApiResponse<IReadOnlyList<TimetableRoomChoiceResponse>>.Success(rows, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsView)]
    [HttpGet("rooms/{roomId:guid}/timetable")]
    public async Task<IActionResult> RoomTimetable(Guid roomId, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var room = await db.SchoolRooms.AsNoTracking().Where(x => x.Id == roomId && x.IsActive)
            .Select(x => new TimetableRoomChoiceResponse(x.Id, x.NameAr, x.NameEn, x.Capacity,
                x.RoomType.NameAr, x.RoomType.NameEn)).SingleOrDefaultAsync(ct);
        if (room is null) return NotFound(Failure(404, "teaching.room_not_found", "Room was not found."));
        var cells = await db.WeeklyTimetableSlots.AsNoTracking().Where(x => x.IsActive && !x.IsBreak && x.ClassSectionSubject != null &&
                (x.RoomId == roomId || (x.RoomId == null && x.ClassSection.RoomAssignments.Any(a => a.IsActive && a.IsPrimary && a.RoomId == roomId &&
                    a.EffectiveFrom <= x.ClassSection.GradeOffering.ProgramAcademicYear.EndDate &&
                    a.EffectiveTo >= x.ClassSection.GradeOffering.ProgramAcademicYear.StartDate))))
            .OrderBy(x => x.DayOfWeek).ThenBy(x => x.StartsAt).ThenBy(x => x.ClassSection.NameAr)
            .Select(x => new RoomTimetableCellResponse(x.Id, x.DayOfWeek.ToString(), x.SlotNumber,
                x.StartsAt.ToString("HH:mm"), x.EndsAt.ToString("HH:mm"), x.AllowRoomSharing,
                x.ClassSectionId, x.ClassSection.NameAr, x.ClassSection.NameEn,
                x.ClassSection.GradeOffering.GradeLevel.NameAr, x.ClassSection.GradeOffering.GradeLevel.NameEn,
                x.ClassSectionSubject!.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                x.ClassSectionSubject.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn,
                x.PrimaryTeacherScope == null ? null : x.PrimaryTeacherScope.TeacherUser.Person.DisplayName)).ToArrayAsync(ct);
        return Ok(ApiResponse<RoomTimetableResponse>.Success(new(room, SchoolWeekOrder.Select(x => x.ToString()).ToArray(), cells),
            correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsView)]
    [HttpGet("temporary-merges")]
    public async Task<IActionResult> TemporaryMerges(CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var rooms = await db.SchoolRooms.AsNoTracking().Where(x => x.IsActive && x.IsSchedulable).OrderBy(x => x.NameAr)
            .Select(x => new TimetableRoomChoiceResponse(x.Id, x.NameAr, x.NameEn, x.Capacity, x.RoomType.NameAr, x.RoomType.NameEn)).ToArrayAsync(ct);
        var sections = await db.ClassSections.AsNoTracking().Where(x => x.IsActive && x.GradeOffering.IsActive && x.GradeOffering.Status != GradeOfferingStatus.Closed)
            .OrderBy(x => x.GradeOffering.GradeLevel.SortOrder).ThenBy(x => x.NameAr)
            .Select(x => new TemporaryMergeSectionChoiceResponse(x.Id, x.NameAr, x.NameEn, x.Capacity,
                x.GradeOffering.GradeLevel.NameAr, x.GradeOffering.GradeLevel.NameEn,
                x.GradeOffering.ProgramAcademicYear.NameAr, x.GradeOffering.ProgramAcademicYear.NameEn)).ToArrayAsync(ct);
        var items = await db.TemporaryClassMerges.AsNoTracking().Where(x => x.IsActive)
            .OrderBy(x => x.MergeDate).ThenBy(x => x.StartsAt)
            .Select(x => new TemporaryMergeResponse(x.Id, x.RoomId, x.Room.NameAr, x.Room.NameEn, x.MergeDate,
                x.StartsAt.ToString("HH:mm"), x.EndsAt.ToString("HH:mm"), x.ExpectedStudentCount, x.Notes,
                x.Sections.Where(s => s.IsActive).Select(s => new TemporaryMergeSectionChoiceResponse(s.ClassSectionId,
                    s.ClassSection.NameAr, s.ClassSection.NameEn, s.ClassSection.Capacity,
                    s.ClassSection.GradeOffering.GradeLevel.NameAr, s.ClassSection.GradeOffering.GradeLevel.NameEn,
                    s.ClassSection.GradeOffering.ProgramAcademicYear.NameAr, s.ClassSection.GradeOffering.ProgramAcademicYear.NameEn)).ToArray()))
            .ToArrayAsync(ct);
        return Ok(ApiResponse<TemporaryMergeDataResponse>.Success(new(rooms, sections, items), correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsManage)]
    [HttpPost("temporary-merges")]
    public async Task<IActionResult> CreateTemporaryMerge([FromBody] SaveTemporaryMergeRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!DateOnly.TryParse(request.MergeDate, out var date) || !TimeOnly.TryParse(request.StartsAt, out var startsAt) ||
            !TimeOnly.TryParse(request.EndsAt, out var endsAt) || startsAt >= endsAt || request.ExpectedStudentCount < 1 ||
            request.SectionIds.Distinct().Count() < 2 || request.Notes?.Length > 500)
            return BadRequest(Failure(400, "teaching.merge_invalid", "Enter a valid date, time, attendance and at least two classes."));
        var ids = request.SectionIds.Distinct().ToArray();
        var room = await db.SchoolRooms.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.RoomId && x.IsActive && x.IsSchedulable, ct);
        if (room is null) return BadRequest(Failure(400, "teaching.room_invalid", "Select an active schedulable room."));
        if (request.ExpectedStudentCount > room.Capacity)
            return Conflict(Failure(409, "teaching.room_capacity", "Expected attendance exceeds the room capacity."));
        var validIds = await db.ClassSections.AsNoTracking().Where(x => ids.Contains(x.Id) && x.IsActive &&
                x.GradeOffering.IsActive && x.GradeOffering.Status != GradeOfferingStatus.Closed &&
                x.GradeOffering.ProgramAcademicYear.StartDate <= date && x.GradeOffering.ProgramAcademicYear.EndDate >= date)
            .Select(x => x.Id).ToArrayAsync(ct);
        if (validIds.Length != ids.Length)
            return BadRequest(Failure(400, "teaching.merge_sections_invalid", "One or more classes are not active on this date."));
        if (await db.TemporaryClassMerges.AnyAsync(x => x.IsActive && x.RoomId == room.Id && x.MergeDate == date && startsAt < x.EndsAt && x.StartsAt < endsAt, ct) ||
            await db.TemporaryClassMergeSections.AnyAsync(x => x.IsActive && ids.Contains(x.ClassSectionId) &&
                x.TemporaryClassMerge.IsActive && x.TemporaryClassMerge.MergeDate == date && startsAt < x.TemporaryClassMerge.EndsAt && x.TemporaryClassMerge.StartsAt < endsAt, ct))
            return Conflict(Failure(409, "teaching.merge_conflict", "A room or class already has another temporary arrangement at this time."));
        if (await db.WeeklyTimetableSlots.AnyAsync(x => x.IsActive && !x.IsBreak &&
                (x.RoomId == room.Id || (x.RoomId == null && x.ClassSection.RoomAssignments.Any(a => a.IsActive && a.IsPrimary && a.RoomId == room.Id &&
                    a.EffectiveFrom <= date && a.EffectiveTo >= date))) && x.DayOfWeek == date.DayOfWeek &&
                !ids.Contains(x.ClassSectionId) && startsAt < x.EndsAt && x.StartsAt < endsAt, ct))
            return Conflict(Failure(409, "teaching.room_conflict", "The room has another weekly booking at this time."));
        var now = DateTimeOffset.UtcNow;
        var merge = new TemporaryClassMerge { Id = Guid.NewGuid(), RoomId = room.Id, MergeDate = date, StartsAt = startsAt,
            EndsAt = endsAt, ExpectedStudentCount = request.ExpectedStudentCount, Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim(),
            CreatedAtUtc = now, UpdatedAtUtc = now };
        foreach (var sectionId in ids) merge.Sections.Add(new TemporaryClassMergeSection { Id = Guid.NewGuid(), ClassSectionId = sectionId,
            CreatedAtUtc = now, UpdatedAtUtc = now });
        db.TemporaryClassMerges.Add(merge); await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(TemporaryMerges), null, ApiResponse<object>.Success(new { merge.Id }, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsManage)]
    [HttpDelete("temporary-merges/{id:guid}")]
    public async Task<IActionResult> DeleteTemporaryMerge(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var item = await db.TemporaryClassMerges.Include(x => x.Sections).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (item is null) return NotFound(Failure(404, "teaching.merge_not_found", "Temporary merge was not found."));
        var now = DateTimeOffset.UtcNow; item.IsActive = false; item.IsDeleted = true; item.DeletedAtUtc = now; item.UpdatedAtUtc = now;
        foreach (var section in item.Sections.Where(x => x.IsActive)) { section.IsActive = false; section.IsDeleted = true; section.DeletedAtUtc = now; section.UpdatedAtUtc = now; }
        await db.SaveChangesAsync(ct); return NoContent();
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsManage)]
    [HttpPut("timetable/{classSectionId:guid}/cells/{dayOfWeek}/{slotNumber:int}")]
    public async Task<IActionResult> SaveTimetableCell(Guid classSectionId, string dayOfWeek, int slotNumber,
        [FromBody] SaveTimetableCellRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!Enum.TryParse<DayOfWeek>(dayOfWeek, true, out var day) || !TimeOnly.TryParse(request.StartsAt, out var startsAt) ||
            !TimeOnly.TryParse(request.EndsAt, out var endsAt) || startsAt >= endsAt)
            return BadRequest(Failure(400, "teaching.cell_invalid", "Enter a valid timetable cell and time range."));
        var section = await db.ClassSections.Where(x => x.Id == classSectionId && x.IsActive).Select(x => new
        {
            Entity = x, x.GradeOfferingId, ProgramId = x.GradeOffering.ProgramAcademicYear.EducationProgramId,
            AcademicYearStart = x.GradeOffering.ProgramAcademicYear.StartDate,
            AcademicYearEnd = x.GradeOffering.ProgramAcademicYear.EndDate,
            x.GradeOffering.GradeLevel.EducationStage.DailyLessonCount,
            x.GradeOffering.GradeLevel.EducationStage.DailyBreakCount
        }).SingleOrDefaultAsync(ct);
        if (section is null) return NotFound(Failure(404, "teaching.section_not_found", "Class section was not found."));
        var totalSlots = section.DailyLessonCount + section.DailyBreakCount;
        if (section.DailyLessonCount < 1 || slotNumber < 1 || slotNumber > totalSlots || !(await StudyDays(db, section.ProgramId, ct)).Contains(day))
            return BadRequest(Failure(400, "teaching.cell_invalid", "The timetable cell is outside the stage schedule."));
        var defaultRoomId = await db.ClassRoomAssignments.AsNoTracking()
            .Where(x => x.ClassSectionId == classSectionId && x.IsActive && x.IsPrimary && x.Room.IsActive && x.Room.IsSchedulable &&
                x.EffectiveFrom <= section.AcademicYearEnd && x.EffectiveTo >= section.AcademicYearStart)
            .OrderByDescending(x => x.EffectiveFrom).Select(x => (Guid?)x.RoomId).FirstOrDefaultAsync(ct);
        var existing = await db.WeeklyTimetableSlots.Include(x => x.SubstituteTeachers)
            .SingleOrDefaultAsync(x => x.ClassSectionId == classSectionId && x.DayOfWeek == day && x.SlotNumber == slotNumber, ct);
        if (await db.WeeklyTimetableSlots.AnyAsync(x => x.IsActive && x.Id != (existing == null ? Guid.Empty : existing.Id) &&
            x.ClassSectionId == classSectionId && x.DayOfWeek == day && startsAt < x.EndsAt && x.StartsAt < endsAt, ct))
            return Conflict(Failure(409, "teaching.class_conflict", "The class already has another slot at this time."));

        ClassSectionSubject? classSubject = null;
        Guid[] substituteIds = [];
        if (request.IsBreak)
        {
            if (section.DailyBreakCount < 1 || await db.WeeklyTimetableSlots.CountAsync(x => x.IsActive && x.Id != (existing == null ? Guid.Empty : existing.Id) &&
                    x.ClassSectionId == classSectionId && x.DayOfWeek == day && x.IsBreak, ct) >= section.DailyBreakCount)
                return Conflict(Failure(409, "teaching.break_limit", "The daily break limit has been reached."));
            if (request.GradeSubjectOfferingId.HasValue || request.PrimaryTeacherScopeId.HasValue || request.RoomId.HasValue ||
                request.AllowRoomSharing || (request.SubstituteTeacherScopeIds?.Count ?? 0) > 0)
                return BadRequest(Failure(400, "teaching.break_payload_invalid", "A break cannot have a subject or teachers."));
        }
        else
        {
            if (!request.GradeSubjectOfferingId.HasValue || !request.PrimaryTeacherScopeId.HasValue)
                return BadRequest(Failure(400, "teaching.lesson_required", "Select the subject and primary teacher."));
            var offering = await db.GradeSubjectOfferings.Include(x => x.CurriculumGradeSubject)
                .SingleOrDefaultAsync(x => x.Id == request.GradeSubjectOfferingId && x.IsActive && x.Status != GradeSubjectOfferingStatus.Closed, ct);
            if (offering is null || offering.GradeOfferingId != section.GradeOfferingId)
                return BadRequest(Failure(400, "teaching.section_subject_invalid", "The subject does not belong to this class section."));
            substituteIds = (request.SubstituteTeacherScopeIds ?? []).Distinct().ToArray();
            if (substituteIds.Contains(request.PrimaryTeacherScopeId.Value))
                return BadRequest(Failure(400, "teaching.teachers_duplicate", "The primary teacher cannot be a substitute for the same lesson."));
            var requestedScopes = substituteIds.Append(request.PrimaryTeacherScopeId.Value).Distinct().ToArray();
            var validScopes = await db.ClassSectionTeacherScopes.AsNoTracking().Where(x => x.IsActive && x.ClassSectionId == classSectionId &&
                    requestedScopes.Contains(x.TeacherGradeSubjectScopeId) && x.TeacherGradeSubjectScope.IsActive &&
                    x.TeacherGradeSubjectScope.GradeSubjectOfferingId == offering.Id)
                .Select(x => x.TeacherGradeSubjectScopeId).ToArrayAsync(ct);
            if (validScopes.Length != requestedScopes.Length)
                return BadRequest(Failure(400, "teaching.teacher_scope_invalid", "Select eligible teachers for this class subject."));
            var subjectId = await db.ClassSectionSubjects.Where(x => x.ClassSectionId == classSectionId && x.GradeSubjectOfferingId == offering.Id)
                .Select(x => (Guid?)x.Id).SingleOrDefaultAsync(ct);
            if (subjectId.HasValue) classSubject = await db.ClassSectionSubjects.SingleAsync(x => x.Id == subjectId.Value, ct);
            else
            {
                var now = DateTimeOffset.UtcNow;
                classSubject = new ClassSectionSubject { Id = Guid.NewGuid(), ClassSectionId = classSectionId,
                    GradeSubjectOfferingId = offering.Id, CreatedAtUtc = now, UpdatedAtUtc = now };
                db.ClassSectionSubjects.Add(classSubject);
            }
            var weeklyCount = await db.WeeklyTimetableSlots.CountAsync(x => x.IsActive && !x.IsBreak &&
                x.Id != (existing == null ? Guid.Empty : existing.Id) && x.ClassSectionId == classSectionId &&
                x.ClassSectionSubject != null && x.ClassSectionSubject.GradeSubjectOfferingId == offering.Id, ct);
            if (weeklyCount >= offering.CurriculumGradeSubject.WeeklyPeriods)
                return Conflict(Failure(409, "teaching.weekly_limit", "The weekly period limit for this subject has been reached."));
            if (await HasTeacherConflict(db, request.PrimaryTeacherScopeId.Value, existing?.Id, day, startsAt, endsAt, ct))
                return Conflict(Failure(409, "teaching.teacher_conflict", "The primary teacher already has another class at this time."));
            var effectiveRoomId = request.RoomId ?? defaultRoomId;
            if (effectiveRoomId.HasValue)
            {
                var room = await db.SchoolRooms.AsNoTracking().SingleOrDefaultAsync(x => x.Id == effectiveRoomId && x.IsActive && x.IsSchedulable, ct);
                if (room is null) return BadRequest(Failure(400, "teaching.room_invalid", "Select an active schedulable room."));
                if (section.Entity.Capacity > room.Capacity)
                    return Conflict(Failure(409, "teaching.room_capacity", "The room capacity is less than the class capacity."));
                var overlaps = await db.WeeklyTimetableSlots.AsNoTracking()
                    .Where(x => x.IsActive && !x.IsBreak &&
                        (x.RoomId == room.Id || (x.RoomId == null && x.ClassSection.RoomAssignments.Any(a => a.IsActive && a.IsPrimary && a.RoomId == room.Id &&
                            a.EffectiveFrom <= x.ClassSection.GradeOffering.ProgramAcademicYear.EndDate &&
                            a.EffectiveTo >= x.ClassSection.GradeOffering.ProgramAcademicYear.StartDate))) && x.DayOfWeek == day &&
                        x.Id != (existing == null ? Guid.Empty : existing.Id) && startsAt < x.EndsAt && x.StartsAt < endsAt)
                    .Select(x => new { x.AllowRoomSharing, x.ClassSectionId, x.ClassSection.Capacity }).ToListAsync(ct);
                if (overlaps.Count > 0 && (!request.AllowRoomSharing || overlaps.Any(x => !x.AllowRoomSharing)))
                    return Conflict(Failure(409, "teaching.room_conflict", "The room is already booked at this time."));
                var occupiedCapacity = overlaps.GroupBy(x => x.ClassSectionId).Sum(x => x.First().Capacity);
                if (overlaps.Count > 0 && occupiedCapacity + section.Entity.Capacity > room.Capacity)
                    return Conflict(Failure(409, "teaching.room_capacity", "The shared room capacity is not enough for all classes."));
            }
        }

        var savedAt = DateTimeOffset.UtcNow;
        if (existing is null)
        {
            existing = new WeeklyTimetableSlot { Id = Guid.NewGuid(), ClassSectionId = classSectionId,
                DayOfWeek = day, SlotNumber = slotNumber, CreatedAtUtc = savedAt };
            db.WeeklyTimetableSlots.Add(existing);
        }
        existing.IsBreak = request.IsBreak; existing.ClassSectionSubject = classSubject;
        existing.ClassSectionSubjectId = classSubject?.Id; existing.PrimaryTeacherScopeId = request.IsBreak ? null : request.PrimaryTeacherScopeId;
        existing.RoomId = request.IsBreak || request.RoomId == defaultRoomId ? null : request.RoomId;
        existing.AllowRoomSharing = !request.IsBreak && (request.RoomId.HasValue || defaultRoomId.HasValue) && request.AllowRoomSharing;
        existing.StartsAt = startsAt; existing.EndsAt = endsAt; existing.IsActive = true; existing.IsDeleted = false;
        existing.DeletedAtUtc = null; existing.DeletedByUserId = null; existing.UpdatedAtUtc = savedAt;
        foreach (var old in existing.SubstituteTeachers.Where(x => x.IsActive && !substituteIds.Contains(x.TeacherGradeSubjectScopeId)))
        { old.IsActive = false; old.IsDeleted = true; old.DeletedAtUtc = savedAt; old.UpdatedAtUtc = savedAt; }
        foreach (var scopeId in substituteIds)
        {
            var old = existing.SubstituteTeachers.FirstOrDefault(x => x.TeacherGradeSubjectScopeId == scopeId);
            if (old is null) existing.SubstituteTeachers.Add(new WeeklyTimetableSlotSubstituteTeacher { Id = Guid.NewGuid(),
                TeacherGradeSubjectScopeId = scopeId, CreatedAtUtc = savedAt, UpdatedAtUtc = savedAt });
            else { old.IsActive = true; old.IsDeleted = false; old.DeletedAtUtc = null; old.DeletedByUserId = null; old.UpdatedAtUtc = savedAt; }
        }
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.AcademicsManage)]
    [HttpDelete("timetable/{classSectionId:guid}/cells/{dayOfWeek}/{slotNumber:int}")]
    public async Task<IActionResult> DeleteTimetableCell(Guid classSectionId, string dayOfWeek, int slotNumber, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!Enum.TryParse<DayOfWeek>(dayOfWeek, true, out var day)) return BadRequest(Failure(400, "teaching.cell_invalid", "Invalid day."));
        var slot = await db.WeeklyTimetableSlots.Include(x => x.SubstituteTeachers)
            .SingleOrDefaultAsync(x => x.ClassSectionId == classSectionId && x.DayOfWeek == day && x.SlotNumber == slotNumber, ct);
        if (slot is null) return NotFound(Failure(404, "teaching.cell_not_found", "Timetable cell was not found."));
        var now = DateTimeOffset.UtcNow; slot.IsActive = false; slot.IsDeleted = true; slot.DeletedAtUtc = now; slot.UpdatedAtUtc = now;
        foreach (var substitute in slot.SubstituteTeachers.Where(x => x.IsActive)) { substitute.IsActive = false; substitute.IsDeleted = true; substitute.DeletedAtUtc = now; substitute.UpdatedAtUtc = now; }
        await db.SaveChangesAsync(ct); return NoContent();
    }

    private static async Task<IReadOnlyList<DayOfWeek>> StudyDays(SchoolsDbContext db, Guid programId, CancellationToken ct)
    {
        var days = await db.SchoolDaySchedules.AsNoTracking().Where(x => x.IsActive && x.EducationProgramId == programId)
            .Select(x => x.DayOfWeek).Distinct().ToListAsync(ct);
        return days.Count > 0 ? SchoolWeekOrder.Where(days.Contains).ToArray() : SchoolWeekOrder;
    }
    private static async Task<bool> HasTeacherConflict(SchoolsDbContext db, Guid primaryScopeId, Guid? currentSlotId,
        DayOfWeek day, TimeOnly startsAt, TimeOnly endsAt, CancellationToken ct)
    {
        var teacherId = await db.TeacherGradeSubjectScopes.Where(x => x.Id == primaryScopeId).Select(x => x.TeacherUserId).SingleAsync(ct);
        return await db.WeeklyTimetableSlots.AsNoTracking().AnyAsync(x => x.IsActive && !x.IsBreak &&
            (!currentSlotId.HasValue || x.Id != currentSlotId.Value) &&
            x.DayOfWeek == day && x.PrimaryTeacherScope != null && x.PrimaryTeacherScope.TeacherUserId == teacherId &&
            startsAt < x.EndsAt && x.StartsAt < endsAt, ct);
    }
    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct) =>
        await dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private ApiResponse<object?> Failure(int status, string code, string message) => ApiResponse<object?>.Failure(status, code, message, correlationId: HttpContext.TraceIdentifier);
}

public sealed record SaveTeacherScopesRequest(IReadOnlyList<Guid> GradeSubjectOfferingIds);
public sealed record SaveClassTeacherScopesRequest(IReadOnlyList<Guid> TeacherGradeSubjectScopeIds);
public sealed record ClassTeacherCandidateResponse(Guid TeacherGradeSubjectScopeId, Guid TeacherUserId,
    Guid GradeSubjectOfferingId, string DisplayName, string UserName, bool Selected);
public sealed record ClassTeacherSubjectOptionsResponse(Guid GradeSubjectOfferingId, string SubjectNameAr,
    string SubjectNameEn, IReadOnlyList<ClassTeacherCandidateResponse> Teachers);
public sealed record ClassTeacherOptionsResponse(Guid ClassSectionId, string ClassSectionNameAr,
    string ClassSectionNameEn, string GradeNameAr, string GradeNameEn, string AcademicYearNameAr,
    string AcademicYearNameEn, IReadOnlyList<ClassTeacherSubjectOptionsResponse> Subjects);
public sealed record TeacherScopeOptionResponse(Guid GradeSubjectOfferingId, Guid ProgramAcademicYearId, string AcademicYearNameAr, string AcademicYearNameEn,
    Guid EducationProgramId, string EducationProgramNameAr, string EducationProgramNameEn, Guid EducationStageId, string EducationStageNameAr, string EducationStageNameEn,
    Guid GradeLevelId, string GradeLevelNameAr, string GradeLevelNameEn, Guid SubjectId, string SubjectNameAr, string SubjectNameEn, bool Selected);
public sealed record TimetableSectionResponse(Guid Id, string NameAr, string NameEn, Guid GradeLevelId,
    string GradeNameAr, string GradeNameEn, Guid EducationStageId, string StageNameAr, string StageNameEn,
    int StageSortOrder, Guid EducationProgramId, string ProgramNameAr, string ProgramNameEn,
    Guid ProgramAcademicYearId, string AcademicYearNameAr, string AcademicYearNameEn,
    int DailyLessonCount, int DailyBreakCount);
public sealed record TeacherScopeChoiceResponse(Guid ScopeId, Guid GradeSubjectOfferingId, Guid TeacherUserId, string TeacherName);
public sealed record TimetableSubjectResponse(Guid GradeSubjectOfferingId, string SubjectNameAr, string SubjectNameEn, int WeeklyPeriods);
public sealed record TimetableContextResponse(Guid ClassSectionId, string ClassSectionNameAr, string ClassSectionNameEn,
    string GradeNameAr, string GradeNameEn, string StageNameAr, string StageNameEn, string ProgramNameAr,
    string ProgramNameEn, string AcademicYearNameAr, string AcademicYearNameEn, int DailyLessonCount, int DailyBreakCount,
    Guid? DefaultRoomId);
public sealed record TimetableCellResponse(Guid Id, string DayOfWeek, int SlotNumber, bool IsBreak,
    Guid? GradeSubjectOfferingId, Guid? PrimaryTeacherScopeId, IReadOnlyList<Guid> SubstituteTeacherScopeIds,
    string StartsAt, string EndsAt, Guid? RoomId, bool UsesDefaultRoom, bool AllowRoomSharing);
public sealed record TimetableRoomChoiceResponse(Guid Id, string NameAr, string NameEn, int Capacity,
    string RoomTypeNameAr, string RoomTypeNameEn);
public sealed record TimetableResponse(TimetableContextResponse Context, IReadOnlyList<string> StudyDays,
    IReadOnlyList<TimetableSubjectResponse> Subjects, IReadOnlyList<TeacherScopeChoiceResponse> TeacherScopes,
    IReadOnlyList<TimetableRoomChoiceResponse> Rooms, IReadOnlyList<TimetableCellResponse> Cells);
public sealed record TeacherTimetableCellResponse(Guid Id, string DayOfWeek, int SlotNumber, string StartsAt,
    string EndsAt, Guid ClassSectionId, string ClassSectionNameAr, string ClassSectionNameEn,
    string GradeNameAr, string GradeNameEn, string SubjectNameAr, string SubjectNameEn,
    string ProgramNameAr, string ProgramNameEn, string AcademicYearNameAr, string AcademicYearNameEn);
public sealed record TeacherTimetableResponse(Guid TeacherId, string DisplayName, string UserName,
    IReadOnlyList<string> StudyDays, IReadOnlyList<TeacherTimetableCellResponse> Cells,
    IReadOnlyList<TeacherCoverageResponse> Substitutions);
public sealed record TeacherCoverageResponse(Guid Id, Guid WeeklyTimetableSlotId, DateOnly LessonDate,
    string DayOfWeek, string StartsAt, string EndsAt, Guid ClassSectionId, string ClassSectionNameAr,
    string ClassSectionNameEn, string SubjectNameAr, string SubjectNameEn, string OriginalTeacherName, string? Reason);
public sealed record RoomTimetableCellResponse(Guid Id, string DayOfWeek, int SlotNumber, string StartsAt,
    string EndsAt, bool AllowRoomSharing, Guid ClassSectionId, string ClassSectionNameAr,
    string ClassSectionNameEn, string GradeNameAr, string GradeNameEn, string SubjectNameAr,
    string SubjectNameEn, string? PrimaryTeacherName);
public sealed record RoomTimetableResponse(TimetableRoomChoiceResponse Room, IReadOnlyList<string> StudyDays,
    IReadOnlyList<RoomTimetableCellResponse> Cells);
public sealed record TemporaryMergeSectionChoiceResponse(Guid Id, string NameAr, string NameEn, int Capacity,
    string GradeNameAr, string GradeNameEn, string AcademicYearNameAr, string AcademicYearNameEn);
public sealed record TemporaryMergeResponse(Guid Id, Guid RoomId, string RoomNameAr, string RoomNameEn,
    DateOnly MergeDate, string StartsAt, string EndsAt, int ExpectedStudentCount, string? Notes,
    IReadOnlyList<TemporaryMergeSectionChoiceResponse> Sections);
public sealed record TemporaryMergeDataResponse(IReadOnlyList<TimetableRoomChoiceResponse> Rooms,
    IReadOnlyList<TemporaryMergeSectionChoiceResponse> Sections, IReadOnlyList<TemporaryMergeResponse> Items);
public sealed record SaveTemporaryMergeRequest(Guid RoomId, string MergeDate, string StartsAt, string EndsAt,
    int ExpectedStudentCount, string? Notes, IReadOnlyList<Guid> SectionIds);
public sealed record SaveTimetableCellRequest(bool IsBreak, Guid? GradeSubjectOfferingId, Guid? PrimaryTeacherScopeId,
    IReadOnlyList<Guid>? SubstituteTeacherScopeIds, string StartsAt, string EndsAt, Guid? RoomId, bool AllowRoomSharing);
