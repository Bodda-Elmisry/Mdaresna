using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.Students;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController, Authorize, Route("api/schools/v1/operational-classes")]
public sealed class SchoolOperationalClassesController(ISchoolDbContextFactory dbFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        await using var db = await RequireDb(ct);
        if (db is null) return Unauthorized();

        var currentUserId = CurrentUserId();
        if (currentUserId == Guid.Empty) return Unauthorized();
        var isSchoolAdmin = await db.LocalUserRoles.AsNoTracking().AnyAsync(x =>
            x.UserId == currentUserId && x.RoleId == SchoolIdentitySeed.SchoolAdminRoleId && x.Role.IsActive, ct);

        var query = db.ClassSections.AsNoTracking().Where(x =>
            x.IsActive && x.GradeOffering.IsActive && x.GradeOffering.Status != GradeOfferingStatus.Closed);
        if (!isSchoolAdmin)
        {
            query = query.Where(section => db.ClassSectionTeacherScopes.Any(scope =>
                scope.ClassSectionId == section.Id && scope.IsActive &&
                scope.TeacherGradeSubjectScope.IsActive &&
                scope.TeacherGradeSubjectScope.TeacherUserId == currentUserId));
        }

        var rows = await query
            .OrderByDescending(x => x.GradeOffering.ProgramAcademicYear.StartDate)
            .ThenBy(x => x.GradeOffering.ProgramAcademicYear.EducationProgram.NameAr)
            .ThenBy(x => x.GradeOffering.GradeLevel.EducationStage.SortOrder)
            .ThenBy(x => x.GradeOffering.GradeLevel.SortOrder)
            .ThenBy(x => x.NameAr)
            .Select(x => new
            {
                x.Id, x.Code, x.NameAr, x.NameEn, x.Capacity, x.Shift,
                AcademicYearNameAr = x.GradeOffering.ProgramAcademicYear.NameAr,
                AcademicYearNameEn = x.GradeOffering.ProgramAcademicYear.NameEn,
                ProgramNameAr = x.GradeOffering.ProgramAcademicYear.EducationProgram.NameAr,
                ProgramNameEn = x.GradeOffering.ProgramAcademicYear.EducationProgram.NameEn,
                StageNameAr = x.GradeOffering.GradeLevel.EducationStage.NameAr,
                StageNameEn = x.GradeOffering.GradeLevel.EducationStage.NameEn,
                GradeNameAr = x.GradeOffering.GradeLevel.NameAr,
                GradeNameEn = x.GradeOffering.GradeLevel.NameEn,
                EnrolledCount = db.StudentEnrollments.Count(enrollment =>
                    enrollment.ClassSectionId == x.Id && enrollment.Status == StudentEnrollmentStatus.Active),
                RoomNameAr = x.RoomAssignments
                    .Where(assignment => assignment.IsActive && assignment.IsPrimary &&
                        assignment.EffectiveFrom <= x.GradeOffering.ProgramAcademicYear.EndDate &&
                        assignment.EffectiveTo >= x.GradeOffering.ProgramAcademicYear.StartDate)
                    .OrderByDescending(assignment => assignment.EffectiveFrom)
                    .Select(assignment => assignment.Room.NameAr).FirstOrDefault(),
                RoomNameEn = x.RoomAssignments
                    .Where(assignment => assignment.IsActive && assignment.IsPrimary &&
                        assignment.EffectiveFrom <= x.GradeOffering.ProgramAcademicYear.EndDate &&
                        assignment.EffectiveTo >= x.GradeOffering.ProgramAcademicYear.StartDate)
                    .OrderByDescending(assignment => assignment.EffectiveFrom)
                    .Select(assignment => assignment.Room.NameEn).FirstOrDefault()
            }).ToArrayAsync(ct);

        var sectionIds = rows.Select(x => x.Id).ToArray();
        var subjectRows = isSchoolAdmin
            ? await db.ClassSectionSubjects.AsNoTracking()
                .Where(x => sectionIds.Contains(x.ClassSectionId) && x.IsActive && x.GradeSubjectOffering.IsActive)
                .Select(x => new OperationalClassSubjectRow(x.ClassSectionId,
                    x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                    x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn))
                .ToArrayAsync(ct)
            : await db.ClassSectionTeacherScopes.AsNoTracking()
                .Where(x => sectionIds.Contains(x.ClassSectionId) && x.IsActive &&
                    x.TeacherGradeSubjectScope.IsActive && x.TeacherGradeSubjectScope.TeacherUserId == currentUserId)
                .Select(x => new OperationalClassSubjectRow(x.ClassSectionId,
                    x.TeacherGradeSubjectScope.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                    x.TeacherGradeSubjectScope.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn))
                .ToArrayAsync(ct);

        var items = rows.Select(x => new OperationalClassResponse(x.Id, x.Code, x.NameAr, x.NameEn,
            x.AcademicYearNameAr, x.AcademicYearNameEn, x.ProgramNameAr, x.ProgramNameEn,
            x.StageNameAr, x.StageNameEn, x.GradeNameAr, x.GradeNameEn, x.Shift.ToString(),
            x.Capacity, x.EnrolledCount, x.RoomNameAr, x.RoomNameEn,
            subjectRows.Where(subject => subject.ClassSectionId == x.Id)
                .DistinctBy(subject => new { subject.NameAr, subject.NameEn })
                .OrderBy(subject => subject.NameAr)
                .Select(subject => new OperationalClassSubjectResponse(subject.NameAr, subject.NameEn)).ToArray()))
            .ToArray();

        return Ok(ApiResponse<OperationalClassesResponse>.Success(new(isSchoolAdmin, items),
            correlationId: HttpContext.TraceIdentifier));
    }

    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct) =>
        await dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private Guid CurrentUserId() =>
        Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : Guid.Empty;
}

public sealed record OperationalClassesResponse(bool IsSchoolAdmin, IReadOnlyList<OperationalClassResponse> Items);
public sealed record OperationalClassResponse(Guid Id, string Code, string NameAr, string NameEn,
    string AcademicYearNameAr, string AcademicYearNameEn, string ProgramNameAr, string ProgramNameEn,
    string StageNameAr, string StageNameEn, string GradeNameAr, string GradeNameEn, string Shift,
    int Capacity, int EnrolledCount, string? RoomNameAr, string? RoomNameEn,
    IReadOnlyList<OperationalClassSubjectResponse> Subjects);
public sealed record OperationalClassSubjectResponse(string NameAr, string NameEn);
file sealed record OperationalClassSubjectRow(Guid ClassSectionId, string NameAr, string NameEn);
