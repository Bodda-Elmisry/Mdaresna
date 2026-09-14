using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Security;

internal sealed class PlatformPermissionEvaluator(PlatformDbContext dbContext) :
    IPlatformPermissionEvaluator
{
    public async Task<bool> HasPermissionAsync(
        IdentityAccountId accountId,
        PermissionCode permission,
        CancellationToken cancellationToken = default)
    {
        if (!await dbContext.LocalUsers.AsNoTracking().AnyAsync(x =>
                x.PersonId == accountId.Value && x.Status == "Active", cancellationToken))
            return false;
        return await (from assignment in dbContext.RoleAssignments
         join role in dbContext.Roles on assignment.RoleId equals role.Id
         join rolePermission in dbContext.RolePermissions on role.Id equals rolePermission.RoleId
         where assignment.AccountId == accountId &&
               assignment.RevokedAtUtc == null &&
               role.IsActive &&
               rolePermission.PermissionCode == permission
         select rolePermission).AnyAsync(cancellationToken);
    }
}
