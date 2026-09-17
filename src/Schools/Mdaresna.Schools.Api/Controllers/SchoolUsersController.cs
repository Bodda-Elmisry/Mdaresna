using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Domain.Identity;
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
        [FromQuery] Guid? roleId = null, [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20, CancellationToken cancellationToken = default)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100) return BadRequest(Failure(400, "users.paging_invalid", "Invalid paging values."));
        var term = search?.Trim();
        if (term?.Length > 100) return BadRequest(Failure(400, "users.search_invalid", "Search cannot exceed 100 characters."));
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var query = db.LocalUsers.AsNoTracking().Where(x => x.Kind == kind);
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
        var total = await query.CountAsync(cancellationToken);
        var users = await query.Include(x => x.Person).ThenInclude(x => x.Contacts)
            .Include(x => x.Roles).ThenInclude(x => x.Role).OrderBy(x => x.UserName)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .Select(x => new SchoolUserResponse(x.Id, x.PersonId, x.UserName, x.Person.DisplayName,
                x.Person.ProfileImage != null,
                x.Person.Contacts.Where(c => c.Type == PersonContactType.Phone && c.IsPrimary).Select(c => c.Value).FirstOrDefault(),
                x.Kind.ToString(), x.Status.ToString(), x.PermissionsVersion,
                x.Roles.Select(r => new SchoolUserRoleResponse(r.RoleId, r.Role.Code, r.Role.DisplayNameAr, r.Role.DisplayNameEn)).ToArray()))
            .ToListAsync(cancellationToken);
        return Ok(PagedApiResponse<SchoolUserResponse>.Success(users, total, pageNumber, pageSize,
            correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.UsersView)]
    [HttpGet("{userId:guid}/profile-image")]
    public async Task<IActionResult> ProfileImage(Guid userId, CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var image = await db.LocalUsers.AsNoTracking().Where(x => x.Id == userId && x.Person.ProfileImage != null)
            .Select(x => new { x.Person.ProfileImage!.Content, x.Person.ProfileImage.ContentType })
            .SingleOrDefaultAsync(cancellationToken);
        return image is null ? NotFound() : File(image.Content, image.ContentType);
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
        var user = await db.LocalUsers.Include(x => x.Person).Include(x => x.Roles)
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
        var user = await db.LocalUsers.Include(x => x.Credential).Include(x => x.Roles)
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null) return NotFound(Failure(404, "users.not_found", "User was not found."));
        if (!request.IsActive && !await CanDisableOrDelete(db, user, cancellationToken))
            return Conflict(Failure(409, "users.last_employee_admin",
                "The last active employee with the school-admin role cannot be disabled."));
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
        var user = await db.LocalUsers.Include(x => x.Person).Include(x => x.Roles)
            .SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null) return NotFound(Failure(404, "users.not_found", "User was not found."));
        if (!await CanDisableOrDelete(db, user, cancellationToken))
            return Conflict(Failure(409, "users.last_employee_admin",
                "The last active employee with the school-admin role cannot be deleted."));
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
public sealed record SchoolUserResponse(Guid Id, Guid PersonId, string UserName, string DisplayName,
    bool HasImage, string? PrimaryPhone, string Kind, string Status, long PermissionsVersion,
    IReadOnlyList<SchoolUserRoleResponse> Roles);
