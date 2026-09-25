using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Domain.Facilities;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.Organization;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController, Authorize, Route("api/schools/v1/departments")]
public sealed class SchoolDepartmentsController(ISchoolDbContextFactory dbFactory) : ControllerBase
{
    [Authorize(Policy = SchoolPermissionPolicies.DepartmentsView)]
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] bool includeDeleted = false, CancellationToken ct = default)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var departmentQuery = includeDeleted ? db.SchoolDepartments.IgnoreQueryFilters() : db.SchoolDepartments;
        var departments = await departmentQuery.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.NameAr).Select(x => new
        {
            x.Id, x.ParentDepartmentId, x.BranchId, x.Code, x.NameAr, x.NameEn, Type = x.Type.ToString(),
            x.SortOrder, x.IsActive, x.IsDeleted, x.DeletedAtUtc,
            Members = x.Memberships.Where(m => m.IsActive).OrderByDescending(m => m.IsPrimary).ThenBy(m => m.User.Person.DisplayName).Select(m => new
            { m.Id, m.UserId, m.User.Person.DisplayName, Kind = m.User.Kind.ToString(), m.TitleAr, m.TitleEn, m.IsPrimary, m.StartsOn, m.EndsOn }),
            Leaderships = x.Leaderships.Where(l => l.IsActive).Select(l => new
            { l.Id, l.UserId, l.User.Person.DisplayName, Role = l.Role.ToString(), l.StartsOn, l.EndsOn }),
            Subjects = x.Subjects.Where(s => s.IsActive).OrderBy(s => s.Subject.NameAr).Select(s => new
            {
                s.Id, s.SubjectId, s.Subject.Code, s.Subject.NameAr, s.Subject.NameEn,
                Coordinators = s.Coordinators.Where(c => c.IsActive).Select(c => new
                { c.Id, c.CoordinatorUserId, c.CoordinatorUser.Person.DisplayName, c.EducationProgramId, c.EducationStageId, c.StartsOn, c.EndsOn })
            })
        }).ToListAsync(ct);
        var users = await db.LocalUsers.AsNoTracking().Where(x => x.Status != LocalUserStatus.Disabled && x.Status != LocalUserStatus.Suspended)
            .OrderBy(x => x.Person.DisplayName).Select(x => new { x.Id, x.Person.DisplayName, x.UserName, Kind = x.Kind.ToString(), Status = x.Status.ToString() }).ToListAsync(ct);
        var branches = await db.SchoolBranches.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.NameAr)
            .Select(x => new { x.Id, x.NameAr, x.NameEn }).ToListAsync(ct);
        var subjects = await db.Subjects.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.NameAr)
            .Select(x => new { x.Id, x.Code, x.NameAr, x.NameEn }).ToListAsync(ct);
        var programs = await db.EducationPrograms.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.NameAr)
            .Select(x => new { x.Id, x.NameAr, x.NameEn }).ToListAsync(ct);
        var stages = await db.EducationStages.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.SortOrder)
            .Select(x => new { x.Id, x.EducationProgramId, x.NameAr, x.NameEn }).ToListAsync(ct);
        return Ok(ApiResponse<object>.Success(new { departments, users, branches, subjects, programs, stages }, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.DepartmentsManage)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveDepartmentRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var validation = await ValidateDepartment(db, request, null, ct); if (validation is not null) return validation;
        var now = DateTimeOffset.UtcNow; var id = Guid.NewGuid();
        db.SchoolDepartments.Add(new SchoolDepartment { Id = id, ParentDepartmentId = request.ParentDepartmentId,
            BranchId = request.BranchId, Code = request.Code.Trim(), NameAr = request.NameAr.Trim(), NameEn = request.NameEn.Trim(),
            Type = Enum.Parse<SchoolDepartmentType>(request.Type, true), SortOrder = request.SortOrder, CreatedAtUtc = now, UpdatedAtUtc = now });
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateException) { return Duplicate(); }
        return StatusCode(201, ApiResponse<object>.Success(new { id }, 201, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.DepartmentsManage)]
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] SaveDepartmentRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var department = await db.SchoolDepartments.SingleOrDefaultAsync(x => x.Id == id, ct); if (department is null) return Missing();
        var validation = await ValidateDepartment(db, request, id, ct); if (validation is not null) return validation;
        if (department.Type == SchoolDepartmentType.Academic && !request.Type.Equals(nameof(SchoolDepartmentType.Academic), StringComparison.OrdinalIgnoreCase) &&
            await db.AcademicDepartmentSubjects.AnyAsync(x => x.DepartmentId == id && x.IsActive, ct))
            return Conflict(Failure(409, "departments.academic_links_exist", "Remove academic subjects before changing the department type."));
        department.ParentDepartmentId = request.ParentDepartmentId; department.BranchId = request.BranchId;
        department.Code = request.Code.Trim(); department.NameAr = request.NameAr.Trim(); department.NameEn = request.NameEn.Trim();
        department.Type = Enum.Parse<SchoolDepartmentType>(request.Type, true); department.SortOrder = request.SortOrder; department.UpdatedAtUtc = DateTimeOffset.UtcNow;
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateException) { return Duplicate(); }
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.DepartmentsManage)]
    [HttpPut("{id:guid}/configuration")]
    public async Task<IActionResult> Configure(Guid id, [FromBody] SaveDepartmentConfigurationRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var department = await db.SchoolDepartments.Include(x => x.Memberships).Include(x => x.Leaderships)
            .Include(x => x.Subjects).ThenInclude(x => x.Coordinators).SingleOrDefaultAsync(x => x.Id == id, ct);
        if (department is null) return Missing();
        var memberInputs = request.Members.GroupBy(x => x.UserId).Select(x => x.First()).ToArray();
        var memberIds = memberInputs.Select(x => x.UserId).ToArray();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        if (memberInputs.Any(x => x.EndsOn.HasValue && x.EndsOn < (x.StartsOn ?? today)) ||
            request.Coordinators.Any(x => x.EndsOn.HasValue && x.EndsOn < (x.StartsOn ?? today)))
            return BadRequest(Failure(400, "departments.period_invalid", "An assignment end date cannot precede its start date."));
        var validUsers = await db.LocalUsers.Where(x => memberIds.Contains(x.Id) && x.Status != LocalUserStatus.Disabled && x.Status != LocalUserStatus.Suspended)
            .Select(x => new { x.Id, x.Kind }).ToListAsync(ct);
        if (validUsers.Count != memberIds.Length || request.HeadUserId.HasValue && !memberIds.Contains(request.HeadUserId.Value))
            return BadRequest(Failure(400, "departments.members_invalid", "Select active members and include the department head among them."));
        var requestedPrimary = memberInputs.Where(x => x.IsPrimary).Select(x => x.UserId).ToArray();
        if (await db.DepartmentMemberships.AnyAsync(x => x.IsActive && x.IsPrimary && x.DepartmentId != id && requestedPrimary.Contains(x.UserId), ct))
            return Conflict(Failure(409, "departments.primary_exists", "A user can have only one primary department."));
        if (department.Type == SchoolDepartmentType.Administrative && (request.Subjects.Count > 0 || request.Coordinators.Count > 0))
            return BadRequest(Failure(400, "departments.academic_only", "Subjects and coordinators are available only for academic departments."));
        var subjectIds = request.Subjects.Distinct().ToArray();
        if (department.Type == SchoolDepartmentType.Academic && await db.Subjects.CountAsync(x => subjectIds.Contains(x.Id) && x.IsActive, ct) != subjectIds.Length)
            return BadRequest(Failure(400, "departments.subjects_invalid", "Select active subjects."));
        var teacherIds = validUsers.Where(x => x.Kind == SchoolUserKind.Teacher).Select(x => x.Id).ToHashSet();
        if (request.Coordinators.Any(x => !subjectIds.Contains(x.SubjectId) || !teacherIds.Contains(x.UserId)))
            return BadRequest(Failure(400, "departments.coordinator_invalid", "A coordinator must be a teacher member of the department and the subject must belong to it."));
        if (request.Coordinators.GroupBy(x => new { x.SubjectId, x.EducationProgramId, x.EducationStageId }).Any(x => x.Count() > 1))
            return BadRequest(Failure(400, "departments.coordinator_duplicate", "Only one coordinator is allowed for each subject scope."));
        if (request.Coordinators.Any(x => x.EducationStageId.HasValue && !x.EducationProgramId.HasValue))
            return BadRequest(Failure(400, "departments.scope_invalid", "Select the education program before the stage."));
        var programIds = request.Coordinators.Where(x => x.EducationProgramId.HasValue).Select(x => x.EducationProgramId!.Value).Distinct().ToArray();
        if (await db.EducationPrograms.CountAsync(x => programIds.Contains(x.Id) && x.IsActive, ct) != programIds.Length)
            return BadRequest(Failure(400, "departments.scope_invalid", "Select an active education program."));
        foreach (var coordinator in request.Coordinators.Where(x => x.EducationStageId.HasValue))
            if (!await db.EducationStages.AnyAsync(x => x.Id == coordinator.EducationStageId && x.EducationProgramId == coordinator.EducationProgramId, ct))
                return BadRequest(Failure(400, "departments.scope_invalid", "The education stage does not belong to the selected program."));

        var now = DateTimeOffset.UtcNow;
        SoftRemove(department.Memberships.Where(x => x.IsActive && !memberIds.Contains(x.UserId)), now, today);
        foreach (var input in memberInputs)
        {
            var member = department.Memberships.FirstOrDefault(x => x.IsActive && x.UserId == input.UserId);
            if (member is null) { member = new DepartmentMembership { Id = Guid.NewGuid(), UserId = input.UserId, StartsOn = input.StartsOn ?? today, CreatedAtUtc = now }; department.Memberships.Add(member); }
            member.TitleAr = Clean(input.TitleAr); member.TitleEn = Clean(input.TitleEn); member.IsPrimary = input.IsPrimary; member.EndsOn = input.EndsOn; member.UpdatedAtUtc = now;
        }
        SoftRemove(department.Leaderships.Where(x => x.IsActive && (x.Role != DepartmentLeadershipRole.Head || x.UserId != request.HeadUserId)), now, today);
        if (request.HeadUserId.HasValue && !department.Leaderships.Any(x => x.IsActive && x.Role == DepartmentLeadershipRole.Head && x.UserId == request.HeadUserId))
            department.Leaderships.Add(new DepartmentLeadership { Id = Guid.NewGuid(), UserId = request.HeadUserId.Value, Role = DepartmentLeadershipRole.Head, StartsOn = today, CreatedAtUtc = now, UpdatedAtUtc = now });
        var removedSubjects = department.Subjects.Where(x => x.IsActive && !subjectIds.Contains(x.SubjectId)).ToArray();
        foreach (var removedSubject in removedSubjects) SoftRemove(removedSubject.Coordinators.Where(x => x.IsActive), now, today);
        SoftRemove(removedSubjects, now, today);
        foreach (var subjectId in subjectIds)
        {
            var link = department.Subjects.FirstOrDefault(x => x.IsActive && x.SubjectId == subjectId);
            if (link is null) { link = new AcademicDepartmentSubject { Id = Guid.NewGuid(), SubjectId = subjectId, CreatedAtUtc = now, UpdatedAtUtc = now }; department.Subjects.Add(link); }
            var requested = request.Coordinators.Where(x => x.SubjectId == subjectId).ToArray();
            SoftRemove(link.Coordinators.Where(x => x.IsActive && !requested.Any(r => SameScope(r, x))), now, today);
            foreach (var input in requested)
            {
                var assignment = link.Coordinators.FirstOrDefault(x => x.IsActive && SameScope(input, x));
                if (assignment is null) { assignment = new SubjectCoordinatorAssignment { Id = Guid.NewGuid(), CoordinatorUserId = input.UserId, EducationProgramId = input.EducationProgramId, EducationStageId = input.EducationStageId, StartsOn = input.StartsOn ?? today, CreatedAtUtc = now }; link.Coordinators.Add(assignment); }
                assignment.EndsOn = input.EndsOn; assignment.UpdatedAtUtc = now;
            }
        }
        department.UpdatedAtUtc = now;
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateException) { return Duplicate(); }
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.DepartmentsManage)]
    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> Status(Guid id, [FromBody] ChangeDepartmentStatusRequest request, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var department = await db.SchoolDepartments.SingleOrDefaultAsync(x => x.Id == id, ct); if (department is null) return Missing();
        if (!request.IsActive && await db.SchoolDepartments.AnyAsync(x => x.ParentDepartmentId == id && x.IsActive, ct))
            return Conflict(Failure(409, "departments.children_active", "Deactivate child departments first."));
        department.IsActive = request.IsActive; department.UpdatedAtUtc = DateTimeOffset.UtcNow; await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.DepartmentsDelete)]
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var department = await db.SchoolDepartments.SingleOrDefaultAsync(x => x.Id == id, ct); if (department is null) return Missing();
        if (await db.SchoolDepartments.AnyAsync(x => x.ParentDepartmentId == id, ct) || await db.DepartmentMemberships.AnyAsync(x => x.DepartmentId == id && x.IsActive, ct) || await db.AcademicDepartmentSubjects.AnyAsync(x => x.DepartmentId == id && x.IsActive, ct))
            return Conflict(Failure(409, "departments.in_use", "Remove child departments, members, and academic subjects first."));
        department.IsActive = false; department.IsDeleted = true; department.DeletedAtUtc = DateTimeOffset.UtcNow; department.DeletedByUserId = CurrentUserId(); department.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct); return NoContent();
    }

    [Authorize(Policy = SchoolPermissionPolicies.DepartmentsRestore)]
    [HttpPost("{id:guid}/restore")]
    public async Task<IActionResult> Restore(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var department = await db.SchoolDepartments.IgnoreQueryFilters().SingleOrDefaultAsync(x => x.Id == id && x.IsDeleted, ct); if (department is null) return Missing();
        if (department.ParentDepartmentId.HasValue && !await db.SchoolDepartments.AnyAsync(x => x.Id == department.ParentDepartmentId && x.IsActive, ct))
            return Conflict(Failure(409, "departments.parent_unavailable", "Restore or activate the parent department first."));
        department.IsDeleted = false; department.DeletedAtUtc = null; department.DeletedByUserId = null; department.UpdatedAtUtc = DateTimeOffset.UtcNow;
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateException) { return Duplicate(); }
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private async Task<IActionResult?> ValidateDepartment(SchoolsDbContext db, SaveDepartmentRequest request, Guid? id, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.Code) || request.Code.Trim().Length > 50 || string.IsNullOrWhiteSpace(request.NameAr) || request.NameAr.Trim().Length > 150 ||
            string.IsNullOrWhiteSpace(request.NameEn) || request.NameEn.Trim().Length > 150 ||
            !Enum.TryParse<SchoolDepartmentType>(request.Type, true, out _) || (id.HasValue && request.ParentDepartmentId == id)) return Invalid();
        if (request.ParentDepartmentId.HasValue)
        {
            var parent = await db.SchoolDepartments.AsNoTracking().SingleOrDefaultAsync(x => x.Id == request.ParentDepartmentId && x.IsActive, ct); if (parent is null) return Invalid();
            if (parent.BranchId.HasValue && request.BranchId != parent.BranchId) return BadRequest(Failure(400, "departments.branch_mismatch", "A child department must use the same branch as its parent."));
            if (id.HasValue && await IsDescendant(db, request.ParentDepartmentId.Value, id.Value, ct)) return BadRequest(Failure(400, "departments.cycle", "A department cannot be moved under one of its descendants."));
        }
        if (request.BranchId.HasValue && !await db.SchoolBranches.AnyAsync(x => x.Id == request.BranchId && x.IsActive, ct)) return Invalid();
        if (id.HasValue && request.BranchId.HasValue && await db.SchoolDepartments.AnyAsync(x => x.ParentDepartmentId == id && x.BranchId != request.BranchId, ct))
            return BadRequest(Failure(400, "departments.branch_mismatch", "Move child departments to the same branch before changing this department scope."));
        return null;
    }
    private static async Task<bool> IsDescendant(SchoolsDbContext db, Guid candidate, Guid departmentId, CancellationToken ct)
    {
        var current = candidate;
        for (var depth = 0; depth < 50; depth++) { if (current == departmentId) return true; var parent = await db.SchoolDepartments.AsNoTracking().Where(x => x.Id == current).Select(x => x.ParentDepartmentId).SingleOrDefaultAsync(ct); if (!parent.HasValue) return false; current = parent.Value; }
        return true;
    }
    private static bool SameScope(SaveCoordinatorRequest request, SubjectCoordinatorAssignment entity) => request.UserId == entity.CoordinatorUserId && request.EducationProgramId == entity.EducationProgramId && request.EducationStageId == entity.EducationStageId;
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private static void SoftRemove<T>(IEnumerable<T> items, DateTimeOffset now, DateOnly today) where T : class, ISoftDeletableSchoolEntity
    {
        foreach (var item in items) { item.IsDeleted = true; item.DeletedAtUtc = now; item.DeletedByUserId = null; if (item is DepartmentMembership m) { m.IsActive = false; m.EndsOn ??= today; m.UpdatedAtUtc = now; } else if (item is DepartmentLeadership l) { l.IsActive = false; l.EndsOn ??= today; l.UpdatedAtUtc = now; } else if (item is AcademicDepartmentSubject s) { s.IsActive = false; s.UpdatedAtUtc = now; } else if (item is SubjectCoordinatorAssignment c) { c.IsActive = false; c.EndsOn ??= today; c.UpdatedAtUtc = now; } }
    }
    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct) => await dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private Guid? CurrentUserId() => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : null;
    private IActionResult Invalid() => BadRequest(Failure(400, "departments.invalid", "Enter valid department data."));
    private IActionResult Missing() => NotFound(Failure(404, "departments.not_found", "Department was not found."));
    private IActionResult Duplicate() => Conflict(Failure(409, "departments.duplicate", "The department or assignment already exists."));
    private ApiResponse<object?> Failure(int status, string code, string message) => ApiResponse<object?>.Failure(status, code, message, correlationId: HttpContext.TraceIdentifier);
}

public sealed record SaveDepartmentRequest(string Code, string NameAr, string NameEn, string Type, Guid? ParentDepartmentId, Guid? BranchId, int SortOrder = 0);
public sealed record SaveDepartmentConfigurationRequest(IReadOnlyList<SaveDepartmentMemberRequest> Members, Guid? HeadUserId, IReadOnlyList<Guid> Subjects, IReadOnlyList<SaveCoordinatorRequest> Coordinators);
public sealed record SaveDepartmentMemberRequest(Guid UserId, string? TitleAr, string? TitleEn, bool IsPrimary, DateOnly? StartsOn, DateOnly? EndsOn);
public sealed record SaveCoordinatorRequest(Guid SubjectId, Guid UserId, Guid? EducationProgramId, Guid? EducationStageId, DateOnly? StartsOn, DateOnly? EndsOn);
public sealed record ChangeDepartmentStatusRequest(bool IsActive);
