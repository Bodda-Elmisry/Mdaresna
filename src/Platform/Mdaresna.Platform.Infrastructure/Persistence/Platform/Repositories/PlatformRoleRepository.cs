using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Domain.Access;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Repositories;

internal sealed class PlatformRoleRepository(PlatformDbContext dbContext) : IPlatformRoleRepository
{
    public async Task<PlatformRole?> FindByIdAsync(
        PlatformRoleId roleId,
        CancellationToken cancellationToken = default)
    {
        var role = await dbContext.Roles.SingleOrDefaultAsync(
            x => x.Id == roleId,
            cancellationToken);

        return await RestorePermissionsAsync(role, cancellationToken);
    }

    public async Task<PlatformRole?> FindByKeyAsync(
        string roleKey,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(roleKey);
        var normalizedKey = roleKey.Trim().ToLowerInvariant();
        var role = await dbContext.Roles.SingleOrDefaultAsync(
            x => x.Key == normalizedKey,
            cancellationToken);

        return await RestorePermissionsAsync(role, cancellationToken);
    }

    public async Task AddAsync(PlatformRole role, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(role);
        await dbContext.Roles.AddAsync(role, cancellationToken);
    }

    private async Task<PlatformRole?> RestorePermissionsAsync(
        PlatformRole? role,
        CancellationToken cancellationToken)
    {
        if (role is null)
        {
            return null;
        }

        var permissions = await dbContext.RolePermissions
            .AsNoTracking()
            .Where(x => x.RoleId == role.Id)
            .Select(x => x.PermissionCode)
            .ToArrayAsync(cancellationToken);

        role.RestorePermissions(permissions);
        return role;
    }
}
