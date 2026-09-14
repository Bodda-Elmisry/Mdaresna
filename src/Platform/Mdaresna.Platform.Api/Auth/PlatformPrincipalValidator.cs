using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.IdentityAuth;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Api.Auth;

internal sealed class PlatformPrincipalValidator(
    IdentityDbContext identityDb,
    PlatformDbContext platformDb)
{
    public async Task<bool> ValidateAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        if (principal.FindFirstValue(PlatformTokenClaims.Purpose) != PlatformTokenClaims.TokenPurpose ||
            !Guid.TryParse(principal.FindFirstValue(JwtRegisteredClaimNames.Sub), out var rawAccountId) ||
            rawAccountId == Guid.Empty)
        {
            return false;
        }

        var stamp = principal.FindFirstValue(PlatformTokenClaims.SecurityStamp);
        if (string.IsNullOrWhiteSpace(stamp))
        {
            return false;
        }

        var account = await identityDb.Accounts
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == rawAccountId, cancellationToken);
        var localUser = await platformDb.LocalUsers.AsNoTracking()
            .Include(x => x.Credential)
            .SingleOrDefaultAsync(x => x.PersonId == rawAccountId, cancellationToken);
        var credential = localUser?.Credential;
        if (account?.Status != AccountStatus.Active ||
            localUser?.Status != "Active" ||
            credential is null ||
            credential.SecurityStamp != stamp ||
            credential.HashingAlgorithm != PlatformPasswordCredentialFactory.Algorithm ||
            credential.HashingVersion != PlatformPasswordCredentialFactory.Version ||
            credential.LockoutEndUtc > DateTimeOffset.UtcNow ||
            credential.MustChangePassword ||
            await identityDb.MfaMethods.AsNoTracking().AnyAsync(
                x => x.AccountId == rawAccountId && x.IsEnabled,
                cancellationToken))
        {
            return false;
        }

        var accountId = IdentityAccountId.From(rawAccountId);
        return await (
            from assignment in platformDb.RoleAssignments.AsNoTracking()
            join role in platformDb.Roles.AsNoTracking() on assignment.RoleId equals role.Id
            where assignment.AccountId == accountId &&
                  assignment.RevokedAtUtc == null &&
                  role.IsActive
            select assignment).AnyAsync(cancellationToken);
    }
}
