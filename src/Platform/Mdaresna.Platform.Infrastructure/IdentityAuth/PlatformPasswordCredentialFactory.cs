using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.AspNetCore.Identity;

namespace Mdaresna.Platform.Infrastructure.IdentityAuth;

/// <summary>
/// For an explicit, audited operator provisioning workflow; never expose as public registration.
/// </summary>
public sealed class PlatformPasswordCredentialFactory(IPasswordHasher<Account> passwordHasher)
{
    public const string Algorithm = "AspNetIdentityV3";
    public const int Version = 3;

    public PasswordCredential Create(Account account, string password, DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        if (account.Id == Guid.Empty || nowUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("A persisted account and UTC timestamp are required.");
        }

        return new PasswordCredential
        {
            AccountId = account.Id,
            PasswordHash = passwordHasher.HashPassword(account, password),
            HashingAlgorithm = Algorithm,
            HashingVersion = Version,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ChangedAtUtc = nowUtc
        };
    }

    public PlatformLocalCredential CreateLocal(
        Account account, PlatformLocalUser localUser, string password, DateTimeOffset nowUtc)
    {
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(localUser);
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        if (account.Id == Guid.Empty || localUser.Id == Guid.Empty ||
            localUser.PersonId != account.Id || nowUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("A linked local user and UTC timestamp are required.");
        }

        return new PlatformLocalCredential
        {
            UserId = localUser.Id,
            PasswordHash = passwordHasher.HashPassword(account, password),
            HashingAlgorithm = Algorithm,
            HashingVersion = Version,
            SecurityStamp = Guid.NewGuid().ToString("N"),
            ChangedAtUtc = nowUtc
        };
    }
}
