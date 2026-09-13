using Mdaresna.Platform.Application.Access.Staff;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.IdentityAuth.Staff;

public sealed class PlatformRoleCatalog(PlatformDbContext dbContext) : IPlatformRoleCatalog
{
    public async Task<IReadOnlyList<PlatformRoleCatalogItem>> ListAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var roles = await dbContext.Roles
            .AsNoTracking()
            .Where(role => includeInactive || role.IsActive)
            .OrderBy(role => role.Key)
            .Select(role => new
            {
                role.Id,
                role.Key,
                role.DisplayName,
                role.IsActive,
                role.IsSystem
            })
            .ToArrayAsync(cancellationToken);

        if (roles.Length == 0)
        {
            return [];
        }

        var roleIds = roles.Select(role => role.Id).ToArray();
        var permissions = await dbContext.RolePermissions
            .AsNoTracking()
            .Where(permission => roleIds.Contains(permission.RoleId))
            .Select(permission => new
            {
                permission.RoleId,
                permission.PermissionCode
            })
            .ToArrayAsync(cancellationToken);
        var permissionsByRole = permissions
            .GroupBy(permission => permission.RoleId)
            .ToDictionary(
                group => group.Key,
                group => (IReadOnlyList<string>)group
                    .Select(permission => permission.PermissionCode.Value)
                    .OrderBy(code => code, StringComparer.Ordinal)
                    .ToArray());

        return roles.Select(role =>
        {
            permissionsByRole.TryGetValue(role.Id, out var codes);
            return new PlatformRoleCatalogItem(
                role.Id.Value,
                role.Key,
                role.DisplayName,
                role.IsActive,
                role.IsSystem,
                codes ?? []);
        }).ToArray();
    }
}
