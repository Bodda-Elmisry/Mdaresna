using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.IdentityAuth;

public sealed record PlatformLoginResult(Guid AccountId, string SecurityStamp, string? DisplayName);

/// <summary>
/// Password sign-in for central accounts that currently have an active Platform role.
/// The same null result is used for all refusals to avoid account enumeration.
/// </summary>
public sealed class PlatformLoginService(
    IdentityDbContext identityDb,
    PlatformDbContext platformDb,
    IPasswordHasher<Account> passwordHasher)
{
    private const int MaximumFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);

    public async Task<PlatformLoginResult?> LoginAsync(
        string identifier,
        string password,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(identifier) ||
            identifier.Length > 320 ||
            string.IsNullOrEmpty(password) ||
            password.Length > 1024)
        {
            return null;
        }

        var normalized = identifier.Trim();
        var identifierType = normalized.Contains('@')
            ? LoginIdentifierType.Email
            : LoginIdentifierType.Phone;

        if (identifierType == LoginIdentifierType.Email)
        {
            normalized = normalized.ToUpperInvariant();
        }

        var loginIdentifier = await identityDb.LoginIdentifiers
            .Include(x => x.Account)
                .ThenInclude(x => x.PasswordCredential)
            .SingleOrDefaultAsync(x =>
                x.Type == identifierType &&
                x.SchoolId == null &&
                x.NormalizedValue == normalized &&
                x.IsVerified,
                cancellationToken);

        var account = loginIdentifier?.Account;
        var credential = account?.PasswordCredential;
        if (account is null || credential is null ||
            account.Status != AccountStatus.Active ||
            credential.LockoutEndUtc > DateTimeOffset.UtcNow ||
            credential.MustChangePassword ||
            credential.HashingAlgorithm != PlatformPasswordCredentialFactory.Algorithm ||
            credential.HashingVersion != PlatformPasswordCredentialFactory.Version ||
            string.IsNullOrWhiteSpace(credential.SecurityStamp))
        {
            return null;
        }

        var verification = passwordHasher.VerifyHashedPassword(
            account, credential.PasswordHash, password);
        if (verification == PasswordVerificationResult.Failed)
        {
            var now = DateTimeOffset.UtcNow;
            if (credential.LockoutEndUtc <= now)
            {
                credential.FailedSignInCount = 0;
                credential.LockoutEndUtc = null;
            }

            credential.FailedSignInCount++;
            if (credential.FailedSignInCount >= MaximumFailedAttempts)
            {
                credential.LockoutEndUtc = now.Add(LockoutDuration);
            }

            AddSecurityEvent(account.Id, "platform.login.failed", false, now);
            await SaveSecurityStateAsync(cancellationToken);
            return null;
        }

        // MFA challenge is not yet implemented. Never silently bypass an enabled factor.
        if (await identityDb.MfaMethods.AnyAsync(
                x => x.AccountId == account.Id && x.IsEnabled,
                cancellationToken))
        {
            AddSecurityEvent(account.Id, "platform.login.mfa_required", false, DateTimeOffset.UtcNow);
            await SaveSecurityStateAsync(cancellationToken);
            return null;
        }

        var accountId = IdentityAccountId.From(account.Id);
        var hasPlatformRole = await (
            from assignment in platformDb.RoleAssignments
            join role in platformDb.Roles on assignment.RoleId equals role.Id
            where assignment.AccountId == accountId &&
                  assignment.RevokedAtUtc == null &&
                  role.IsActive
            select assignment).AnyAsync(cancellationToken);
        if (!hasPlatformRole)
        {
            AddSecurityEvent(account.Id, "platform.login.no_role", false, DateTimeOffset.UtcNow);
            await SaveSecurityStateAsync(cancellationToken);
            return null;
        }

        var signedInAt = DateTimeOffset.UtcNow;
        credential.FailedSignInCount = 0;
        credential.LockoutEndUtc = null;
        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            credential.PasswordHash = passwordHasher.HashPassword(account, password);
            credential.SecurityStamp = Guid.NewGuid().ToString("N");
            credential.ChangedAtUtc = signedInAt;
        }

        AddSecurityEvent(account.Id, "platform.login.succeeded", true, signedInAt);
        if (!await SaveSecurityStateAsync(cancellationToken))
        {
            return null;
        }

        return new PlatformLoginResult(account.Id, credential.SecurityStamp, account.DisplayName);
    }

    private void AddSecurityEvent(Guid accountId, string eventType, bool succeeded, DateTimeOffset now) =>
        identityDb.SecurityEvents.Add(new IdentitySecurityEvent
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            EventType = eventType,
            Succeeded = succeeded,
            OccurredAtUtc = now
        });

    private async Task<bool> SaveSecurityStateAsync(CancellationToken cancellationToken)
    {
        try
        {
            await identityDb.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Another request changed the credential concurrently. Fail closed.
            return false;
        }
    }
}
