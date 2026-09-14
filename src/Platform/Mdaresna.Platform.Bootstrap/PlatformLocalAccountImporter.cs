using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Bootstrap;

/// <summary>
/// Expand-only person-link import. It never reads, copies, or changes
/// credentials; each Platform password must be established locally.
/// </summary>
internal sealed class PlatformLocalAccountImporter(
    IdentityDbContext identityDb,
    PlatformDbContext platformDb)
{
    public async Task<PlatformLocalImportResult> RunAsync(
        bool dryRun, CancellationToken cancellationToken = default)
    {
        if (!await identityDb.Database.CanConnectAsync(cancellationToken) ||
            !await platformDb.Database.CanConnectAsync(cancellationToken) ||
            (await identityDb.Database.GetPendingMigrationsAsync(cancellationToken)).Any() ||
            (await platformDb.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
        {
            throw new BootstrapRejectedException(
                "Both databases must be reachable and fully migrated before local-account import.");
        }

        var roleAccountIds = await (
            from assignment in platformDb.RoleAssignments.AsNoTracking()
            join role in platformDb.Roles.AsNoTracking()
                on assignment.RoleId equals role.Id
            where assignment.RevokedAtUtc == null && role.IsActive
            select assignment.AccountId).Distinct().ToArrayAsync(cancellationToken);
        var personIds = roleAccountIds.Select(x => x.Value).OrderBy(x => x).ToArray();
        var imported = 0;
        var skipped = 0;

        foreach (var personId in personIds)
        {
            var account = await identityDb.Accounts.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == personId, cancellationToken);
            if (account is null)
                throw new BootstrapRejectedException(
                    "A Platform role refers to a missing central person; import stopped for review.");

            var local = await platformDb.LocalUsers
                .SingleOrDefaultAsync(x => x.PersonId == personId, cancellationToken);
            if (local is not null)
            {
                skipped++;
                continue;
            }

            var now = DateTimeOffset.UtcNow;
            if (!dryRun)
            {
                var localId = Guid.NewGuid();
                platformDb.LocalUsers.Add(new PlatformLocalUser
                {
                    Id = localId,
                    PersonId = personId,
                    UserName = $"platform-{personId:N}",
                    NormalizedUserName = $"PLATFORM-{personId:N}",
                    DisplayName = account.DisplayName,
                    Status = account.Status is
                        AccountStatus.PendingVerification or AccountStatus.Active
                        ? "PendingActivation" : "Disabled",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
                await platformDb.SaveChangesAsync(cancellationToken);
            }
            imported++;
        }

        return new PlatformLocalImportResult(personIds.Length, imported, skipped, dryRun);
    }
}

internal sealed record PlatformLocalImportResult(
    int Total, int Imported, int Skipped, bool DryRun);
