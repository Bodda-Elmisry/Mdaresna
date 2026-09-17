using System.ComponentModel.DataAnnotations;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController]
[Route("api/schools/v1/users")]
public sealed class SchoolUsersController(
    ISchoolDbContextFactory dbFactory,
    IPasswordHasher<LocalUserAccount> passwordHasher) : ControllerBase
{
    [Authorize(Policy = SchoolPermissionPolicies.UsersView)]
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var users = await db.LocalUsers.AsNoTracking().Include(x => x.Person)
            .Include(x => x.Roles).ThenInclude(x => x.Role).OrderBy(x => x.UserName)
            .Select(x => new SchoolUserResponse(x.Id, x.PersonId, x.UserName, x.Person.DisplayName,
                x.Status.ToString(), x.PermissionsVersion,
                x.Roles.Select(r => new SchoolUserRoleResponse(r.RoleId, r.Role.Code, r.Role.DisplayNameAr)).ToArray()))
            .ToListAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SchoolUserResponse>>.Success(users, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.UsersManage)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSchoolUserRequest request, CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var username = request.UserName.Trim(); var normalized = username.ToUpperInvariant();
        if (username.Contains('@') || await db.LocalUsers.AnyAsync(x => x.NormalizedUserName == normalized, cancellationToken))
            return Conflict(Failure(409, "users.username_unavailable", "Username is unavailable."));
        var roles = await db.LocalRoles.Where(x => request.RoleIds.Contains(x.Id) && x.IsActive).ToListAsync(cancellationToken);
        if (roles.Count != request.RoleIds.Distinct().Count()) return BadRequest(Failure(400, "users.roles_invalid", "One or more roles are invalid."));
        var now = DateTimeOffset.UtcNow;
        var person = new Person { Id = Guid.NewGuid(), DisplayName = request.DisplayName.Trim(), Status = PersonStatus.Active, CreatedAtUtc = now, UpdatedAtUtc = now };
        var user = new LocalUserAccount { Id = Guid.NewGuid(), PersonId = person.Id, Person = person, UserName = username,
            NormalizedUserName = normalized, Status = LocalUserStatus.Active, PermissionsVersion = 1, CreatedAtUtc = now, UpdatedAtUtc = now };
        user.Credential = new LocalUserCredential { UserId = user.Id, User = user, SecurityStamp = Guid.NewGuid().ToString("N"),
            MustChangePassword = request.MustChangePassword, ChangedAtUtc = now };
        user.Credential.PasswordHash = passwordHasher.HashPassword(user, request.Password);
        user.Roles = roles.Select(role => new LocalUserRole { UserId = user.Id, RoleId = role.Id, User = user, Role = role,
            AssignedAtUtc = now, AssignedByUserId = CurrentUserId() }).ToList();
        db.Persons.Add(person); db.LocalUsers.Add(user); await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(List), null, ApiResponse<object>.Success(new { user.Id }, statusCode: 201, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.UsersManage)]
    [HttpPut("{userId:guid}/roles")]
    public async Task<IActionResult> ReplaceRoles(Guid userId, [FromBody] ReplaceSchoolUserRolesRequest request, CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var user = await db.LocalUsers.Include(x => x.Roles).SingleOrDefaultAsync(x => x.Id == userId, cancellationToken);
        if (user is null) return NotFound(Failure(404, "users.not_found", "User was not found."));
        var roleIds = request.RoleIds.Distinct().ToArray();
        var roles = await db.LocalRoles.Where(x => roleIds.Contains(x.Id) && x.IsActive).ToListAsync(cancellationToken);
        if (roles.Count != roleIds.Length) return BadRequest(Failure(400, "users.roles_invalid", "One or more roles are invalid."));
        var removingAdmin = user.Roles.Any(x => x.RoleId == SchoolIdentitySeed.SchoolAdminRoleId) && !roleIds.Contains(SchoolIdentitySeed.SchoolAdminRoleId);
        if (removingAdmin)
        {
            var adminCount = await db.LocalUserRoles.CountAsync(x => x.RoleId == SchoolIdentitySeed.SchoolAdminRoleId && x.User.Status == LocalUserStatus.Active, cancellationToken);
            if (adminCount <= 1) return Conflict(Failure(409, "users.last_admin", "The last active school administrator cannot lose the school-admin role."));
        }
        db.LocalUserRoles.RemoveRange(user.Roles);
        var now = DateTimeOffset.UtcNow;
        user.Roles = roles.Select(role => new LocalUserRole { UserId = user.Id, RoleId = role.Id, User = user, Role = role,
            AssignedAtUtc = now, AssignedByUserId = CurrentUserId() }).ToList();
        user.PermissionsVersion++; user.UpdatedAtUtc = now; await db.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct) =>
        await dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private Guid? CurrentUserId() => Guid.TryParse(User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : null;
    private ApiResponse<object?> Failure(int status, string code, string message) => ApiResponse<object?>.Failure(status, code, message, correlationId: HttpContext.TraceIdentifier);
}

public sealed record CreateSchoolUserRequest(
    [Required, MaxLength(200)] string DisplayName,
    [Required, RegularExpression("^[A-Za-z0-9._-]{3,100}$")] string UserName,
    [Required, MinLength(8), MaxLength(200)] string Password,
    IReadOnlyList<Guid> RoleIds,
    bool MustChangePassword = true);
public sealed record ReplaceSchoolUserRolesRequest(IReadOnlyList<Guid> RoleIds);
public sealed record SchoolUserRoleResponse(Guid Id, string Code, string DisplayName);
public sealed record SchoolUserResponse(Guid Id, Guid PersonId, string UserName, string DisplayName,
    string Status, long PermissionsVersion, IReadOnlyList<SchoolUserRoleResponse> Roles);
