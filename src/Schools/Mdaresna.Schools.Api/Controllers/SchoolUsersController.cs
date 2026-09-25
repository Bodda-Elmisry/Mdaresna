using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.Organization;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController]
[Route("api/schools/v1/users")]
public sealed class SchoolUsersController(ISchoolDbContextFactory dbFactory, ISchoolUserIdentityGateway identityGateway) : ControllerBase
{
    [Authorize(Policy = SchoolPermissionPolicies.UsersView)]
    [HttpGet("role-options")]
    public async Task<IActionResult> RoleOptions(CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var roles = await db.LocalRoles.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Code)
            .Select(x => new SchoolUserRoleResponse(x.Id, x.Code, x.DisplayNameAr, x.DisplayNameEn))
            .ToListAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SchoolUserRoleResponse>>.Success(roles,
            correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.UsersView)]
    [HttpGet]
    public async Task<IActionResult> List([FromQuery] SchoolUserKind kind = SchoolUserKind.Employee,
        [FromQuery] string? search = null, [FromQuery] LocalUserStatus? status = null,
        [FromQuery] Guid? roleId = null, [FromQuery] Guid? departmentId = null,
        [FromQuery] string? name = null, [FromQuery] string? phone = null, [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100) return BadRequest(Failure(400, "users.paging_invalid", "Invalid paging values."));
        var term = search?.Trim();
        var nameTerm = name?.Trim();
        var phoneTerm = phone?.Trim();
        if (term?.Length > 100) return BadRequest(Failure(400, "users.search_invalid", "Search cannot exceed 100 characters."));
        if (nameTerm?.Length > 100 || phoneTerm?.Length > 20)
            return BadRequest(Failure(400, "users.search_invalid", "The supplied user filters are invalid."));
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var query = await SchoolDepartmentScope.ApplyAsync(User, db,
            db.LocalUsers.AsNoTracking().Where(x => x.Kind == kind), cancellationToken);
        if (!string.IsNullOrWhiteSpace(term))
        {
            var normalized = term.ToUpperInvariant();
            query = query.Where(x => x.NormalizedUserName.Contains(normalized) ||
                x.Person.DisplayName.ToUpper().Contains(normalized) ||
                x.Person.Contacts.Any(c => c.Type == PersonContactType.Phone && c.Value.Contains(term)));
        }
        if (status.HasValue) query = query.Where(x => x.Status == status.Value);
        if (roleId.HasValue && roleId.Value != Guid.Empty)
            query = query.Where(x => x.Roles.Any(r => r.RoleId == roleId.Value));
        if (departmentId.HasValue && departmentId.Value != Guid.Empty)
            query = query.Where(x => x.DepartmentMemberships.Any(m => m.DepartmentId == departmentId.Value &&
                m.IsActive && m.StartsOn <= today && (!m.EndsOn.HasValue || m.EndsOn >= today)));
        if (!string.IsNullOrWhiteSpace(nameTerm))
            query = query.Where(x => x.Person.DisplayName.ToUpper().Contains(nameTerm.ToUpperInvariant()) ||
                x.NormalizedUserName.Contains(nameTerm.ToUpperInvariant()));
        if (!string.IsNullOrWhiteSpace(phoneTerm))
            query = query.Where(x => x.Person.Contacts.Any(c => c.Type == PersonContactType.Phone && c.Value.Contains(phoneTerm)));
        var total = await query.CountAsync(cancellationToken);
        var users = await query.Include(x => x.Person).ThenInclude(x => x.Contacts)
            .Include(x => x.Roles).ThenInclude(x => x.Role).OrderBy(x => x.UserName)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new SchoolUserResponse(x.Id, x.PersonId, x.UserName, x.Person.DisplayName,
                x.Person.ProfileImage != null,
                x.Person.Contacts.Where(c => c.Type == PersonContactType.Phone && c.IsPrimary).Select(c => c.Value).FirstOrDefault(),
                x.Kind.ToString(), x.Status.ToString(), x.PermissionsVersion,
                x.Roles.Select(r => new SchoolUserRoleResponse(r.RoleId, r.Role.Code, r.Role.DisplayNameAr, r.Role.DisplayNameEn)).ToArray(),
                x.DepartmentMemberships.Where(m => m.IsActive && m.StartsOn <= today && (!m.EndsOn.HasValue || m.EndsOn >= today))
                    .OrderByDescending(m => m.IsPrimary).ThenBy(m => m.Department.SortOrder)
                    .Select(m => new SchoolUserDepartmentResponse(m.DepartmentId, m.Department.NameAr, m.Department.NameEn,
                        m.Department.Type, m.IsPrimary, m.TitleAr, m.TitleEn)).ToArray()))
            .ToListAsync(cancellationToken);
        return Ok(PagedApiResponse<SchoolUserResponse>.Success(users, total, pageNumber, pageSize,
            correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.UsersView)]
    [HttpGet("department-options")]
    public async Task<IActionResult> DepartmentOptions([FromQuery] SchoolUserKind kind = SchoolUserKind.Employee,
        CancellationToken cancellationToken = default)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var visibleUsers = await SchoolDepartmentScope.ApplyAsync(User, db,
            db.LocalUsers.AsNoTracking().Where(x => x.Kind == kind), cancellationToken);
        var departmentRows = await visibleUsers.SelectMany(x => x.DepartmentMemberships)
            .Where(x => x.IsActive && x.Department.IsActive && x.StartsOn <= today &&
                (!x.EndsOn.HasValue || x.EndsOn >= today))
            .Select(x => new { x.DepartmentId, x.Department.NameAr, x.Department.NameEn,
                x.Department.Type, x.Department.SortOrder })
            .ToListAsync(cancellationToken);
        var departments = departmentRows.GroupBy(x => x.DepartmentId).Select(x => x.First())
            .OrderBy(x => x.SortOrder).ThenBy(x => x.NameAr)
            .Select(x => new SchoolUserDepartmentOptionResponse(x.DepartmentId,
                x.NameAr, x.NameEn, x.Type, x.SortOrder)).ToList();
        return Ok(ApiResponse<IReadOnlyList<SchoolUserDepartmentOptionResponse>>.Success(departments,
            correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.UsersView)]
    [HttpGet("{userId:guid}/profile-image")]
    public async Task<IActionResult> ProfileImage(Guid userId, CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var imageQuery = await SchoolDepartmentScope.ApplyAsync(User, db, db.LocalUsers.AsNoTracking(), cancellationToken);
        var image = await imageQuery.Where(x => x.Id == userId && x.Person.ProfileImage != null)
            .Select(x => new { x.Person.ProfileImage!.Content, x.Person.ProfileImage.ContentType })
            .SingleOrDefaultAsync(cancellationToken);
        return image is null ? NotFound() : File(image.Content, image.ContentType);
    }

    [Authorize(Policy = SchoolPermissionPolicies.UsersView)]
    [HttpGet("{userId:guid}/details")]
    public async Task<IActionResult> Details(Guid userId, CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var detailsQuery = await SchoolDepartmentScope.ApplyAsync(User, db, db.LocalUsers.AsNoTracking(), cancellationToken);
        var user = await detailsQuery
            .Include(x => x.Person).ThenInclude(x => x.Contacts)
            .Include(x => x.Person).ThenInclude(x => x.ProfileImage)
            .Include(x => x.Roles).ThenInclude(x => x.Role)
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null) return NotFound(Failure(404, "users.not_found", "User was not found."));

        var contacts = user.Person.Contacts
            .Select(x => new SchoolContactInformationResponse(x.Id, ContactType(x.Type), x.Value,
                x.Type == PersonContactType.Address ? null : x.IsVerified, x.IsPrimary, false))
            .ToList();
        if (user.PlatformAccountId.HasValue)
        {
            var school = await db.SchoolInformation.AsNoTracking()
                .Select(x => new { x.PlatformSchoolReferenceId }).SingleOrDefaultAsync(cancellationToken);
            if (school is not null)
            {
                SchoolUserPrimaryContact? primary = null;
                try
                {
                    primary = await identityGateway.GetPrimaryContactAsync(school.PlatformSchoolReferenceId,
                        user.PlatformAccountId.Value, cancellationToken);
                }
                catch (InvalidOperationException) { /* Keep the locally synchronized primary contact. */ }
                if (primary is not null)
                {
                    contacts.RemoveAll(x => x.Type == "phone" && x.IsPrimary);
                    contacts.Insert(0, new(primary.Id, "phone", primary.PrimaryPhone,
                        primary.PhoneVerified, true, false));
                }
            }
        }

        var scopes = user.Kind == SchoolUserKind.Teacher
            ? await db.TeacherGradeSubjectScopes.AsNoTracking()
                .Where(x => x.TeacherUserId == userId && x.IsActive)
                .OrderByDescending(x => x.GradeSubjectOffering.GradeOffering.ProgramAcademicYear.StartDate)
                .ThenBy(x => x.GradeSubjectOffering.GradeOffering.GradeLevel.SortOrder)
                .ThenBy(x => x.GradeSubjectOffering.CurriculumGradeSubject.SortOrder)
                .Select(x => new TeacherAcademicScopeDetailsResponse(
                    x.GradeSubjectOffering.GradeOffering.ProgramAcademicYear.NameAr,
                    x.GradeSubjectOffering.GradeOffering.ProgramAcademicYear.NameEn,
                    x.GradeSubjectOffering.GradeOffering.ProgramAcademicYear.EducationProgram.NameAr,
                    x.GradeSubjectOffering.GradeOffering.ProgramAcademicYear.EducationProgram.NameEn,
                    x.GradeSubjectOffering.GradeOffering.GradeLevel.EducationStage.NameAr,
                    x.GradeSubjectOffering.GradeOffering.GradeLevel.EducationStage.NameEn,
                    x.GradeSubjectOffering.GradeOffering.GradeLevel.NameAr,
                    x.GradeSubjectOffering.GradeOffering.GradeLevel.NameEn,
                    x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                    x.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn))
                .ToListAsync(cancellationToken)
            : [];

        var timetableSlots = user.Kind == SchoolUserKind.Teacher
            ? await db.WeeklyTimetableSlots.AsNoTracking()
                .Where(x => x.IsActive && !x.IsBreak && x.ClassSectionSubject != null &&
                    ((x.PrimaryTeacherScope != null && x.PrimaryTeacherScope.TeacherUserId == userId) ||
                     x.SubstituteTeachers.Any(s => s.IsActive && s.TeacherGradeSubjectScope.TeacherUserId == userId)))
                .Include(x => x.PrimaryTeacherScope)
                .Include(x => x.SubstituteTeachers).ThenInclude(x => x.TeacherGradeSubjectScope)
                .Include(x => x.ClassSection)
                .Include(x => x.ClassSectionSubject!).ThenInclude(x => x.GradeSubjectOffering)
                    .ThenInclude(x => x.CurriculumGradeSubject).ThenInclude(x => x.Subject)
                .Include(x => x.ClassSectionSubject!).ThenInclude(x => x.GradeSubjectOffering)
                    .ThenInclude(x => x.GradeOffering).ThenInclude(x => x.GradeLevel)
                .Include(x => x.ClassSectionSubject!).ThenInclude(x => x.GradeSubjectOffering)
                    .ThenInclude(x => x.GradeOffering).ThenInclude(x => x.ProgramAcademicYear)
                .ToListAsync(cancellationToken)
            : [];
        var assignmentRows = timetableSlots.SelectMany(slot =>
        {
            var roles = new List<string>();
            if (slot.PrimaryTeacherScope?.TeacherUserId == userId) roles.Add(ClassSubjectTeacherRole.Primary.ToString());
            if (slot.SubstituteTeachers.Any(x => x.IsActive && x.TeacherGradeSubjectScope.TeacherUserId == userId))
                roles.Add(ClassSubjectTeacherRole.Substitute.ToString());
            var subject = slot.ClassSectionSubject!;
            return roles.Select(role => new TeacherClassAssignmentDetailsResponse(role,
                slot.ClassSection.NameAr, slot.ClassSection.NameEn,
                subject.GradeSubjectOffering.GradeOffering.GradeLevel.NameAr,
                subject.GradeSubjectOffering.GradeOffering.GradeLevel.NameEn,
                subject.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                subject.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn,
                subject.GradeSubjectOffering.GradeOffering.ProgramAcademicYear.NameAr,
                subject.GradeSubjectOffering.GradeOffering.ProgramAcademicYear.NameEn,
                [new TeacherTimetableSlotDetailsResponse(slot.DayOfWeek.ToString(),
                    slot.StartsAt.ToString("HH:mm"), slot.EndsAt.ToString("HH:mm"))]));
        });
        var assignments = assignmentRows.GroupBy(x => new { x.Role, x.ClassSectionNameAr, x.ClassSectionNameEn,
                x.GradeLevelNameAr, x.GradeLevelNameEn, x.SubjectNameAr, x.SubjectNameEn,
                x.AcademicYearNameAr, x.AcademicYearNameEn })
            .Select(x => new TeacherClassAssignmentDetailsResponse(x.Key.Role, x.Key.ClassSectionNameAr,
                x.Key.ClassSectionNameEn, x.Key.GradeLevelNameAr, x.Key.GradeLevelNameEn,
                x.Key.SubjectNameAr, x.Key.SubjectNameEn, x.Key.AcademicYearNameAr, x.Key.AcademicYearNameEn,
                x.SelectMany(y => y.Slots).OrderBy(y => y.DayOfWeek).ThenBy(y => y.StartsAt).ToArray()))
            .ToArray();
        var classScopes = user.Kind == SchoolUserKind.Teacher
            ? await db.ClassSectionTeacherScopes.AsNoTracking()
                .Where(x => x.IsActive && x.TeacherGradeSubjectScope.TeacherUserId == userId)
                .OrderByDescending(x => x.ClassSection.GradeOffering.ProgramAcademicYear.StartDate)
                .ThenBy(x => x.ClassSection.NameAr)
                .Select(x => new TeacherClassScopeDetailsResponse(x.ClassSection.NameAr, x.ClassSection.NameEn,
                    x.ClassSection.GradeOffering.GradeLevel.NameAr, x.ClassSection.GradeOffering.GradeLevel.NameEn,
                    x.TeacherGradeSubjectScope.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameAr,
                    x.TeacherGradeSubjectScope.GradeSubjectOffering.CurriculumGradeSubject.Subject.NameEn,
                    x.ClassSection.GradeOffering.ProgramAcademicYear.NameAr,
                    x.ClassSection.GradeOffering.ProgramAcademicYear.NameEn))
                .ToListAsync(cancellationToken)
            : [];

        var response = new SchoolUserDetailsResponse(user.Id, user.PersonId, user.UserName,
            user.Person.DisplayName, user.Person.ProfileImage is not null, user.Kind.ToString(),
            user.Status.ToString(), user.Person.DateOfBirth, user.Person.GenderCode,
            user.CreatedAtUtc, user.LastLoginAtUtc, contacts,
            user.Roles.OrderBy(x => x.Role.Code).Select(x => new SchoolUserRoleResponse(x.RoleId,
                x.Role.Code, x.Role.DisplayNameAr, x.Role.DisplayNameEn)).ToArray(), scopes, classScopes, assignments);
        return Ok(ApiResponse<SchoolUserDetailsResponse>.Success(response,
            correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.UsersManage)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSchoolUserRequest request, CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            return BadRequest(Failure(400, "users.display_name_required", "Display name is required."));
        var username = request.UserName.Trim(); var normalized = username.ToUpperInvariant();
        if (username.Contains('@') || await db.LocalUsers.AnyAsync(x => x.NormalizedUserName == normalized, cancellationToken))
            return Conflict(Failure(409, "users.username_unavailable", "Username is unavailable."));
        var roles = await RequireRoles(db, request.RoleIds, cancellationToken);
        if (roles is null) return BadRequest(Failure(400, "users.roles_invalid", "One or more roles are invalid."));
        var school = await db.SchoolInformation.AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        if (school is null) return Conflict(Failure(409, "school.profile_not_found", "School information was not found."));
        var identity = await identityGateway.ResolveAsync(school.PlatformSchoolReferenceId, request.Phone, request.DisplayName, cancellationToken);
        if (identity is null) return Conflict(Failure(409, "users.identity_unavailable",
            "The phone must be the primary phone of an available Identity account."));
        if (await db.LocalUsers.AnyAsync(x => x.PlatformAccountId == identity.AccountId, cancellationToken))
            return Conflict(Failure(409, "users.identity_exists", "This Identity account already belongs to a school user."));

        var now = DateTimeOffset.UtcNow;
        var person = new Person { Id = Guid.NewGuid(), DisplayName = request.DisplayName.Trim(), Status = PersonStatus.Active,
            CreatedAtUtc = now, UpdatedAtUtc = now, Contacts = [new PersonContact { Id = Guid.NewGuid(),
                Type = PersonContactType.Phone, Value = identity.PrimaryPhone, NormalizedValue = identity.PrimaryPhone,
                IsPrimary = true, IsVerified = identity.PhoneVerified, CreatedAtUtc = now, UpdatedAtUtc = now }] };
        var user = new LocalUserAccount { Id = Guid.NewGuid(), PersonId = person.Id, Person = person,
            PlatformAccountId = identity.AccountId, UserName = username, NormalizedUserName = normalized, Kind = request.Kind,
            Status = LocalUserStatus.PendingActivation, PermissionsVersion = 1, CreatedAtUtc = now, UpdatedAtUtc = now };
        user.Roles = roles.Select(role => new LocalUserRole { UserId = user.Id, RoleId = role.Id, User = user,
            Role = role, AssignedAtUtc = now, AssignedByUserId = CurrentUserId() }).ToList();
        db.Persons.Add(person); db.LocalUsers.Add(user); await db.SaveChangesAsync(cancellationToken);
        var fullLogin = $"{username}@{school.Code}";
        var delivered = await identityGateway.SendInvitationAsync(school.PlatformSchoolReferenceId, identity.AccountId,
            fullLogin, cancellationToken);
        return CreatedAtAction(nameof(List), null, ApiResponse<CreateSchoolUserResponse>.Success(
            new(user.Id, fullLogin, delivered), statusCode: 201, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.UsersManage)]
    [HttpPut("{userId:guid}")]
    public async Task<IActionResult> Update(Guid userId, [FromBody] UpdateSchoolUserRequest request, CancellationToken cancellationToken)
    {
        if (userId == CurrentUserId())
            return Conflict(Failure(409, "users.self_update", "Manage your personal data from your profile."));
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.DisplayName))
            return BadRequest(Failure(400, "users.display_name_required", "Display name is required."));
        var updateQuery = await SchoolDepartmentScope.ApplyAsync(User, db, db.LocalUsers.Include(x => x.Person).Include(x => x.Roles), cancellationToken);
        var user = await updateQuery
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null) return NotFound(Failure(404, "users.not_found", "User was not found."));
        var username = request.UserName.Trim(); var normalized = username.ToUpperInvariant();
        if (user.Status == LocalUserStatus.PendingActivation && user.NormalizedUserName != normalized)
            return Conflict(Failure(409, "users.pending_username",
                "Complete account activation before changing the username."));
        if (username.Contains('@') || await db.LocalUsers.AnyAsync(x => x.Id != userId && x.NormalizedUserName == normalized, cancellationToken))
            return Conflict(Failure(409, "users.username_unavailable", "Username is unavailable."));
        var roles = await RequireRoles(db, request.RoleIds, cancellationToken);
        if (roles is null) return BadRequest(Failure(400, "users.roles_invalid", "One or more roles are invalid."));
        if (!await PreservesActiveEmployeeAdmin(db, user, request.Kind,
                roles.Select(x => x.Id).ToArray(), cancellationToken))
            return Conflict(Failure(409, "users.last_employee_admin",
                "At least one active employee must keep the school-admin role."));
        if (user.Kind == SchoolUserKind.Teacher && request.Kind != SchoolUserKind.Teacher &&
            await db.TeacherGradeSubjectScopes.AnyAsync(x => x.TeacherUserId == userId && x.IsActive, cancellationToken))
            return Conflict(Failure(409, "users.teacher_scope_in_use",
                "Remove the teacher's academic scope before changing the user kind."));
        var now = DateTimeOffset.UtcNow;
        user.UserName = username; user.NormalizedUserName = normalized; user.Kind = request.Kind;
        user.Person.DisplayName = request.DisplayName.Trim(); user.Person.UpdatedAtUtc = now;
        user.UpdatedAtUtc = now; user.PermissionsVersion++;
        db.LocalUserRoles.RemoveRange(user.Roles);
        user.Roles = roles.Select(role => new LocalUserRole { UserId = user.Id, RoleId = role.Id, User = user,
            Role = role, AssignedAtUtc = now, AssignedByUserId = CurrentUserId() }).ToList();
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.UsersManage)]
    [HttpPut("{userId:guid}/status")]
    public async Task<IActionResult> SetStatus(Guid userId, [FromBody] SetSchoolUserStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (userId == CurrentUserId()) return Conflict(Failure(409, "users.self_status", "You cannot change your own status."));
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var statusQuery = await SchoolDepartmentScope.ApplyAsync(User, db, db.LocalUsers.Include(x => x.Credential).Include(x => x.Roles), cancellationToken);
        var user = await statusQuery
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null) return NotFound(Failure(404, "users.not_found", "User was not found."));
        if (!request.IsActive && !await CanDisableOrDelete(db, user, cancellationToken))
            return Conflict(Failure(409, "users.last_employee_admin",
                "The last active employee with the school-admin role cannot be disabled."));
        if (!request.IsActive && user.Kind == SchoolUserKind.Teacher &&
            (await db.ClassSubjectTeacherAssignments.AnyAsync(x => x.IsActive && x.TeacherGradeSubjectScope.TeacherUserId == userId, cancellationToken) ||
             await db.WeeklyTimetableSlots.AnyAsync(x => x.IsActive && x.PrimaryTeacherScope != null && x.PrimaryTeacherScope.TeacherUserId == userId, cancellationToken) ||
             await db.WeeklyTimetableSlotSubstituteTeachers.AnyAsync(x => x.IsActive && x.TeacherGradeSubjectScope.TeacherUserId == userId, cancellationToken) ||
             await db.ClassSectionTeacherScopes.AnyAsync(x => x.IsActive && x.TeacherGradeSubjectScope.TeacherUserId == userId, cancellationToken)))
            return Conflict(Failure(409, "users.teacher_timetable_in_use",
                "Remove the teacher from active classes and timetables before disabling the account."));
        if (request.IsActive && user.Credential is null)
            return Conflict(Failure(409, "users.activation_required", "The user must complete password activation first."));
        user.Status = request.IsActive ? LocalUserStatus.Active : LocalUserStatus.Disabled;
        user.UpdatedAtUtc = DateTimeOffset.UtcNow; await db.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.UsersManage)]
    [HttpDelete("{userId:guid}")]
    public async Task<IActionResult> Delete(Guid userId, CancellationToken cancellationToken)
    {
        if (userId == CurrentUserId()) return Conflict(Failure(409, "users.self_delete", "You cannot delete your own account."));
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var deleteQuery = await SchoolDepartmentScope.ApplyAsync(User, db, db.LocalUsers.Include(x => x.Person).Include(x => x.Roles), cancellationToken);
        var user = await deleteQuery
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null) return NotFound(Failure(404, "users.not_found", "User was not found."));
        if (!await CanDisableOrDelete(db, user, cancellationToken))
            return Conflict(Failure(409, "users.last_employee_admin",
                "The last active employee with the school-admin role cannot be deleted."));
        if (user.Kind == SchoolUserKind.Teacher &&
            await db.TeacherGradeSubjectScopes.AnyAsync(x => x.TeacherUserId == userId, cancellationToken))
            return Conflict(Failure(409, "users.teacher_scope_in_use",
                "Remove the teacher's academic scope before deleting the account."));
        db.LocalUsers.Remove(user); db.Persons.Remove(user.Person); await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct) =>
        await dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private static async Task<LocalRole[]?> RequireRoles(SchoolsDbContext db, IReadOnlyList<Guid> ids, CancellationToken ct)
    {
        var distinct = ids.Distinct().ToArray(); if (distinct.Length is < 1 or > 10) return null;
        var roles = await db.LocalRoles.Where(x => distinct.Contains(x.Id) && x.IsActive).ToArrayAsync(ct);
        return roles.Length == distinct.Length ? roles : null;
    }
    private static async Task<bool> PreservesActiveEmployeeAdmin(SchoolsDbContext db, LocalUserAccount user,
        SchoolUserKind newKind, IReadOnlyCollection<Guid> newRoleIds, CancellationToken ct)
    {
        var isActiveEmployeeAdmin = user.Status == LocalUserStatus.Active &&
            user.Kind == SchoolUserKind.Employee &&
            user.Roles.Any(x => x.RoleId == SchoolIdentitySeed.SchoolAdminRoleId);
        var remainsActiveEmployeeAdmin = newKind == SchoolUserKind.Employee &&
            newRoleIds.Contains(SchoolIdentitySeed.SchoolAdminRoleId);
        return !isActiveEmployeeAdmin || remainsActiveEmployeeAdmin ||
            await ActiveEmployeeAdminCount(db, ct) > 1;
    }
    private static async Task<bool> CanDisableOrDelete(SchoolsDbContext db, LocalUserAccount user, CancellationToken ct) =>
        user.Status != LocalUserStatus.Active || user.Kind != SchoolUserKind.Employee ||
        !user.Roles.Any(x => x.RoleId == SchoolIdentitySeed.SchoolAdminRoleId) ||
        await ActiveEmployeeAdminCount(db, ct) > 1;
    private static Task<int> ActiveEmployeeAdminCount(SchoolsDbContext db, CancellationToken ct) =>
        db.LocalUserRoles.CountAsync(x => x.RoleId == SchoolIdentitySeed.SchoolAdminRoleId &&
            x.User.Kind == SchoolUserKind.Employee && x.User.Status == LocalUserStatus.Active, ct);
    private Guid CurrentUserId() => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : Guid.Empty;
    private static string ContactType(PersonContactType type) => type switch
    {
        PersonContactType.Phone => "phone",
        PersonContactType.Email => "email",
        _ => "address"
    };
    private ApiResponse<object?> Failure(int status, string code, string message) =>
        ApiResponse<object?>.Failure(status, code, message, correlationId: HttpContext.TraceIdentifier);
}

public sealed record CreateSchoolUserRequest([Required, MaxLength(200)] string DisplayName,
    [Required, RegularExpression("^[A-Za-z0-9._-]{3,100}$")] string UserName,
    [Required, RegularExpression("^[0-9]{8,16}$")] string Phone, SchoolUserKind Kind, IReadOnlyList<Guid> RoleIds);
public sealed record UpdateSchoolUserRequest([Required, MaxLength(200)] string DisplayName,
    [Required, RegularExpression("^[A-Za-z0-9._-]{3,100}$")] string UserName,
    SchoolUserKind Kind, IReadOnlyList<Guid> RoleIds);
public sealed record SetSchoolUserStatusRequest(bool IsActive);
public sealed record CreateSchoolUserResponse(Guid Id, string FullLogin, bool InvitationDelivered);
public sealed record SchoolUserRoleResponse(Guid Id, string Code, string DisplayNameAr, string DisplayNameEn);
public sealed record SchoolUserDepartmentResponse(Guid Id, string NameAr, string NameEn, SchoolDepartmentType Type,
    bool IsPrimary, string? TitleAr, string? TitleEn);
public sealed record SchoolUserDepartmentOptionResponse(Guid Id, string NameAr, string NameEn, SchoolDepartmentType Type,
    int SortOrder);
public sealed record SchoolUserResponse(Guid Id, Guid PersonId, string UserName, string DisplayName,
    bool HasImage, string? PrimaryPhone, string Kind, string Status, long PermissionsVersion,
    IReadOnlyList<SchoolUserRoleResponse> Roles, IReadOnlyList<SchoolUserDepartmentResponse> Departments);
public sealed record SchoolUserDetailsResponse(Guid Id, Guid PersonId, string UserName, string DisplayName,
    bool HasImage, string Kind, string Status, DateOnly? DateOfBirth, string? GenderCode,
    DateTimeOffset CreatedAtUtc, DateTimeOffset? LastLoginAtUtc,
    IReadOnlyList<SchoolContactInformationResponse> Contacts, IReadOnlyList<SchoolUserRoleResponse> Roles,
    IReadOnlyList<TeacherAcademicScopeDetailsResponse> AcademicScopes,
    IReadOnlyList<TeacherClassScopeDetailsResponse> ClassScopes,
    IReadOnlyList<TeacherClassAssignmentDetailsResponse> ClassAssignments);
public sealed record TeacherAcademicScopeDetailsResponse(string AcademicYearNameAr, string AcademicYearNameEn,
    string EducationProgramNameAr, string EducationProgramNameEn, string EducationStageNameAr,
    string EducationStageNameEn, string GradeLevelNameAr, string GradeLevelNameEn,
    string SubjectNameAr, string SubjectNameEn);
public sealed record TeacherClassScopeDetailsResponse(string ClassSectionNameAr, string ClassSectionNameEn,
    string GradeLevelNameAr, string GradeLevelNameEn, string SubjectNameAr, string SubjectNameEn,
    string AcademicYearNameAr, string AcademicYearNameEn);
public sealed record TeacherClassAssignmentDetailsResponse(string Role, string ClassSectionNameAr,
    string ClassSectionNameEn, string GradeLevelNameAr, string GradeLevelNameEn, string SubjectNameAr,
    string SubjectNameEn, string AcademicYearNameAr, string AcademicYearNameEn,
    IReadOnlyList<TeacherTimetableSlotDetailsResponse> Slots);
public sealed record TeacherTimetableSlotDetailsResponse(string DayOfWeek, string StartsAt, string EndsAt);
