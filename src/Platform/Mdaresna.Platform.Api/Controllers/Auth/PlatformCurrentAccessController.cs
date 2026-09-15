using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Api.Controllers.Auth;

[ApiController]
[Authorize]
[Route("api/platform/v1/me/access")]
public sealed class PlatformCurrentAccessController(PlatformDbContext platformDb) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var accountId = IdentityAccountId.From(CurrentAccountId());
        var roles = await (
            from assignment in platformDb.RoleAssignments.AsNoTracking()
            join role in platformDb.Roles.AsNoTracking() on assignment.RoleId equals role.Id
            where assignment.AccountId == accountId && assignment.RevokedAtUtc == null && role.IsActive
            orderby role.Key
            select new PlatformLoginRole(role.Key, role.DisplayName))
            .ToArrayAsync(cancellationToken);
        var permissionValues = await (
            from assignment in platformDb.RoleAssignments.AsNoTracking()
            join role in platformDb.Roles.AsNoTracking() on assignment.RoleId equals role.Id
            join permission in platformDb.RolePermissions.AsNoTracking() on role.Id equals permission.RoleId
            where assignment.AccountId == accountId && assignment.RevokedAtUtc == null && role.IsActive
            select permission.PermissionCode).ToArrayAsync(cancellationToken);
        var permissions = permissionValues.Select(code => code.Value)
            .Distinct(StringComparer.Ordinal).OrderBy(code => code, StringComparer.Ordinal).ToArray();
        return Ok(ApiResponse<PlatformCurrentAccessResponse>.Success(
            new PlatformCurrentAccessResponse(roles, permissions),
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    private Guid CurrentAccountId() =>
        Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var accountId) && accountId != Guid.Empty
            ? accountId
            : throw new InvalidOperationException("A validated Platform account is required.");
}

public sealed record PlatformCurrentAccessResponse(
    IReadOnlyList<PlatformLoginRole> Roles,
    IReadOnlyList<string> PermissionCodes);
