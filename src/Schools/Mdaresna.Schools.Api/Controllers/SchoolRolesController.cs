using System.ComponentModel.DataAnnotations;
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
[Route("api/schools/v1/roles")]
public sealed class SchoolRolesController(ISchoolDbContextFactory dbFactory) : ControllerBase
{
    [Authorize(Policy = SchoolPermissionPolicies.RolesView)]
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var roles = await db.LocalRoles.AsNoTracking().Include(x => x.Permissions).ThenInclude(x => x.Permission)
            .OrderBy(x => x.Code).Select(x => new SchoolRoleResponse(x.Id, x.Code, x.DisplayNameAr, x.DisplayNameEn,
                x.IsSystem, x.IsActive, x.Permissions.Select(p => p.Permission.Code).OrderBy(p => p).ToArray()))
            .ToListAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SchoolRoleResponse>>.Success(roles, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.RolesView)]
    [HttpGet("permissions")]
    public async Task<IActionResult> Permissions(CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var items = await db.LocalPermissions.AsNoTracking().Where(x => x.IsActive).OrderBy(x => x.Module).ThenBy(x => x.Code)
            .Select(x => new SchoolPermissionResponse(x.Id, x.Code, x.Module, x.DisplayNameAr, x.DisplayNameEn)).ToListAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SchoolPermissionResponse>>.Success(items, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.RolesManage)]
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveSchoolRoleRequest request, CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var code = request.Code.Trim().ToLowerInvariant();
        if (await db.LocalRoles.AnyAsync(x => x.Code == code, cancellationToken)) return Conflict(Failure(409, "roles.code_exists", "Role code already exists."));
        var permissions = await RequirePermissions(db, request.PermissionIds, cancellationToken);
        if (permissions is null) return BadRequest(Failure(400, "roles.permissions_invalid", "One or more permissions are invalid."));
        var now = DateTimeOffset.UtcNow; var role = new LocalRole { Id = Guid.NewGuid(), Code = code,
            DisplayNameAr = request.DisplayNameAr.Trim(), DisplayNameEn = request.DisplayNameEn.Trim(), IsActive = true,
            IsSystem = false, CreatedAtUtc = now, UpdatedAtUtc = now };
        role.Permissions = permissions.Select(x => new LocalRolePermission { RoleId = role.Id, PermissionId = x.Id,
            Role = role, Permission = x, GrantedAtUtc = now, GrantedByUserId = CurrentUserId() }).ToList();
        db.LocalRoles.Add(role); await db.SaveChangesAsync(cancellationToken);
        return CreatedAtAction(nameof(List), null, ApiResponse<object>.Success(new { role.Id }, statusCode: 201, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.RolesManage)]
    [HttpPut("{roleId:guid}/permissions")]
    public async Task<IActionResult> ReplacePermissions(Guid roleId, [FromBody] ReplaceRolePermissionsRequest request, CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var role = await db.LocalRoles.Include(x => x.Permissions).SingleOrDefaultAsync(x => x.Id == roleId, cancellationToken);
        if (role is null) return NotFound(Failure(404, "roles.not_found", "Role was not found."));
        if (role.IsSystem) return Conflict(Failure(409, "roles.system_protected", "System role permissions are managed by versioned seed data."));
        var permissions = await RequirePermissions(db, request.PermissionIds, cancellationToken);
        if (permissions is null) return BadRequest(Failure(400, "roles.permissions_invalid", "One or more permissions are invalid."));
        db.LocalRolePermissions.RemoveRange(role.Permissions); var now = DateTimeOffset.UtcNow;
        role.Permissions = permissions.Select(x => new LocalRolePermission { RoleId = role.Id, PermissionId = x.Id,
            Role = role, Permission = x, GrantedAtUtc = now, GrantedByUserId = CurrentUserId() }).ToList();
        role.UpdatedAtUtc = now;
        var affected = await db.LocalUserRoles.Where(x => x.RoleId == roleId).Select(x => x.User).ToListAsync(cancellationToken);
        foreach (var user in affected) { user.PermissionsVersion++; user.UpdatedAtUtc = now; }
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [Authorize(Policy = SchoolPermissionPolicies.RolesManage)]
    [HttpDelete("{roleId:guid}")]
    public async Task<IActionResult> Delete(Guid roleId, CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken); if (db is null) return Unauthorized();
        var role = await db.LocalRoles.SingleOrDefaultAsync(x => x.Id == roleId, cancellationToken);
        if (role is null) return NotFound(Failure(404, "roles.not_found", "Role was not found."));
        if (role.IsSystem) return Conflict(Failure(409, "roles.system_protected", "System roles cannot be deleted."));
        if (await db.LocalUserRoles.AnyAsync(x => x.RoleId == roleId, cancellationToken)) return Conflict(Failure(409, "roles.in_use", "Role is assigned to users."));
        db.LocalRoles.Remove(role); await db.SaveChangesAsync(cancellationToken); return NoContent();
    }

    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct) => await dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private static async Task<LocalPermission[]?> RequirePermissions(SchoolsDbContext db, IReadOnlyList<Guid> ids, CancellationToken ct)
    {
        var distinct = ids.Distinct().ToArray(); var permissions = await db.LocalPermissions.Where(x => distinct.Contains(x.Id) && x.IsActive).ToArrayAsync(ct);
        return permissions.Length == distinct.Length ? permissions : null;
    }
    private Guid? CurrentUserId() => Guid.TryParse(User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : null;
    private ApiResponse<object?> Failure(int status, string code, string message) => ApiResponse<object?>.Failure(status, code, message, correlationId: HttpContext.TraceIdentifier);
}

public sealed record SaveSchoolRoleRequest(
    [Required, RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")] string Code,
    [Required, MaxLength(150)] string DisplayNameAr,
    [Required, MaxLength(150)] string DisplayNameEn,
    IReadOnlyList<Guid> PermissionIds);
public sealed record ReplaceRolePermissionsRequest(IReadOnlyList<Guid> PermissionIds);
public sealed record SchoolPermissionResponse(Guid Id, string Code, string Module, string DisplayNameAr, string DisplayNameEn);
public sealed record SchoolRoleResponse(Guid Id, string Code, string DisplayNameAr, string DisplayNameEn,
    bool IsSystem, bool IsActive, IReadOnlyList<string> Permissions);
