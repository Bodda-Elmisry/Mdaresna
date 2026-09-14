using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.IdentityAuth;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Bootstrap;

/// <summary>
/// One-person, resumable move from legacy Identity credential to Platform.
/// This never logs or returns the hash. A partial Platform commit can be
/// completed safely by rerunning with the same person ID.
/// </summary>
internal sealed class PlatformCredentialMover(
    IdentityDbContext identityDb,
    PlatformDbContext platformDb)
{
    public async Task<PlatformCredentialMoveResult> RunAsync(
        Guid personId, bool execute, CancellationToken ct = default)
    {
        if (personId == Guid.Empty) throw new BootstrapRejectedException("A person ID is required.");
        if (!await identityDb.Database.CanConnectAsync(ct) ||
            !await platformDb.Database.CanConnectAsync(ct) ||
            (await identityDb.Database.GetPendingMigrationsAsync(ct)).Any() ||
            (await platformDb.Database.GetPendingMigrationsAsync(ct)).Any())
            throw new BootstrapRejectedException("Both databases must be reachable and fully migrated.");

        var account = await identityDb.Accounts.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == personId, ct);
        var verifiedPhone = await identityDb.LoginIdentifiers.AsNoTracking()
            .AnyAsync(x => x.AccountId == personId &&
                x.Type == LoginIdentifierType.Phone && x.SchoolId == null && x.IsVerified, ct);
        var hasRole = await (from assignment in platformDb.RoleAssignments.AsNoTracking()
            join role in platformDb.Roles.AsNoTracking() on assignment.RoleId equals role.Id
            where assignment.AccountId == IdentityAccountId.From(personId) &&
                assignment.RevokedAtUtc == null && role.IsActive
            select assignment.Id).AnyAsync(ct);
        if (account?.Status != AccountStatus.Active || !verifiedPhone || !hasRole)
            throw new BootstrapRejectedException("The target is not an active, verified Platform operator.");

        var legacy = await identityDb.PasswordCredentials.AsNoTracking()
            .SingleOrDefaultAsync(x => x.AccountId == personId, ct);
        var local = await platformDb.LocalUsers.AsNoTracking().Include(x => x.Credential)
            .SingleOrDefaultAsync(x => x.PersonId == personId, ct);
        if (local is null || local.Status == "Disabled")
            throw new BootstrapRejectedException("A linked, enabled Platform local user is required.");
        if (legacy is null)
        {
            if (local.Status != "Active" || local.Credential is null)
                throw new BootstrapRejectedException("No credential exists in either expected location.");
            return new PlatformCredentialMoveResult(AlreadyMoved: true, Executed: execute);
        }
        if (legacy.HashingAlgorithm != PlatformPasswordCredentialFactory.Algorithm ||
            legacy.HashingVersion != PlatformPasswordCredentialFactory.Version ||
            string.IsNullOrWhiteSpace(legacy.PasswordHash))
            throw new BootstrapRejectedException("The legacy credential format needs manual review.");
        if (local.Credential is not null &&
            (local.Status != "Active" || local.Credential.PasswordHash != legacy.PasswordHash))
            throw new BootstrapRejectedException("The local credential differs from the legacy source.");
        if (!execute) return new PlatformCredentialMoveResult(AlreadyMoved: false, Executed: false);

        // First make the new login usable. If the process stops here, a rerun
        // recognizes the matching hash and removes the legacy copy.
        if (local.Credential is null)
        {
            var tracked = await platformDb.LocalUsers.Include(x => x.Credential)
                .SingleAsync(x => x.PersonId == personId, ct);
            tracked.Credential = new PlatformLocalCredential
            {
                UserId = tracked.Id,
                PasswordHash = legacy.PasswordHash,
                HashingAlgorithm = legacy.HashingAlgorithm,
                HashingVersion = legacy.HashingVersion,
                SecurityStamp = Guid.NewGuid().ToString("N"),
                FailedSignInCount = legacy.FailedSignInCount,
                LockoutEndUtc = legacy.LockoutEndUtc,
                MustChangePassword = legacy.MustChangePassword,
                ChangedAtUtc = legacy.ChangedAtUtc
            };
            tracked.Status = "Active";
            tracked.UpdatedAtUtc = DateTimeOffset.UtcNow;
            await platformDb.SaveChangesAsync(ct);
        }

        // Remove only the exact old credential. Concurrency checks reject a
        // password change that occurred while the local copy was being made.
        var trackedLegacy = await identityDb.PasswordCredentials
            .SingleAsync(x => x.AccountId == personId, ct);
        if (trackedLegacy.PasswordHash != legacy.PasswordHash ||
            trackedLegacy.SecurityStamp != legacy.SecurityStamp)
            throw new BootstrapRejectedException("Legacy credential changed during move; manual review required.");
        identityDb.PasswordCredentials.Remove(trackedLegacy);
        var now = DateTimeOffset.UtcNow;
        var sessions = await identityDb.Sessions
            .Where(x => x.AccountId == personId && x.RevokedAtUtc == null &&
                x.ExpiresAtUtc > now).ToArrayAsync(ct);
        foreach (var session in sessions)
        {
            session.RevokedAtUtc = now;
            session.RevocationReason = "platform-credential-moved";
        }
        identityDb.SecurityEvents.Add(new IdentitySecurityEvent
        {
            Id = Guid.NewGuid(),
            AccountId = personId,
            EventType = "platform.credential.moved_to_local",
            Succeeded = true,
            OccurredAtUtc = now
        });
        await identityDb.SaveChangesAsync(ct);
        return new PlatformCredentialMoveResult(AlreadyMoved: false, Executed: true);
    }
}

internal sealed record PlatformCredentialMoveResult(bool AlreadyMoved, bool Executed);
