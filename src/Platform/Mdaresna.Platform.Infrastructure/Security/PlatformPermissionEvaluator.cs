using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Security;

internal sealed class PlatformPermissionEvaluator(PlatformDbContext dbContext) :
    IPlatformPermissionEvaluator
{
    public Task<bool> HasPermissionAsync(
        IdentityAccountId accountId,
        PermissionCode permission,
        CancellationToken cancellationToken = default) =>
        (from assignment in dbContext.RoleAssignments
         join role in dbContext.Roles on assignment.RoleId equals role.Id
         join rolePermission in dbContext.RolePermissions on role.Id equals rolePermission.RoleId
         where assignment.AccountId == accountId &&
               assignment.RevokedAtUtc == null &&
               role.IsActive &&
               rolePermission.PermissionCode == permission
         select rolePermission).AnyAsync(cancellationToken);
}
