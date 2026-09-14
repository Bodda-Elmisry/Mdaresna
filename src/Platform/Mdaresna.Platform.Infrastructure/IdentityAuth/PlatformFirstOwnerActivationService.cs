using System.Data;
using System.Security.Cryptography;
using System.Text;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Mdaresna.Platform.Infrastructure.IdentityAuth;

public sealed class PlatformActivationOptions
{
    public byte[] CodeHashKey { get; }

    public PlatformActivationOptions(byte[] codeHashKey)
    {
        if (codeHashKey is not { Length: >= 32 })
        {
            throw new ArgumentException("A random activation HMAC key of at least 32 bytes is required.");
        }

        CodeHashKey = codeHashKey.ToArray();
    }

    public static PlatformActivationOptions FromConfiguration(IConfiguration configuration)
    {
        var raw = configuration["PlatformActivation:CodeHashKey"];
        try
        {
            return new PlatformActivationOptions(Convert.FromBase64String(raw ?? string.Empty));
        }
        catch (FormatException)
        {
            throw new InvalidOperationException(
                "PlatformActivation:CodeHashKey must be a Base64-encoded random 32-byte secret.");
        }
    }
}

/// <summary>
/// First-owner activation only. No normal Platform token is issued until the
/// phone code is verified and a Platform-local password has been established.
/// </summary>
public sealed class PlatformFirstOwnerActivationService(
    IdentityDbContext identityDb,
    PlatformDbContext platformDb,
    IPlatformSmsSender smsSender,
    PlatformPasswordCredentialFactory credentialFactory,
    IPasswordHasher<Account> passwordHasher,
    PlatformActivationOptions options,
    ILogger<PlatformFirstOwnerActivationService> logger)
{
    private const string ProvisionedAuditAction = "platform.bootstrap.first_owner.provisioned";
    private const int MaximumFailedAttempts = 5;
    private const int MaximumDailySends = 10;
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan SendCooldown = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan SendWindow = TimeSpan.FromDays(1);

    public async Task StartAsync(string phone, CancellationToken cancellationToken = default)
    {
        var normalized = phone?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > 32)
        {
            return;
        }

        var identifier = await identityDb.LoginIdentifiers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Type == LoginIdentifierType.Phone &&
                                       x.SchoolId == null &&
                                       x.NormalizedValue == normalized,
                cancellationToken);
        if (identifier is null || identifier.IsVerified ||
            !await IsProvisionedFirstOwnerAsync(identifier.AccountId, cancellationToken))
        {
            return;
        }

        string? generatedOnPreviousAttempt = null;
        var strategy = identityDb.Database.CreateExecutionStrategy();
        var code = await strategy.ExecuteAsync(async () =>
        {
            identityDb.ChangeTracker.Clear();
            await using var transaction = await identityDb.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);
            await DatabaseAdvisoryLock.AcquireAsync(identityDb, transaction,
                $"mdaresna-platform-first-owner-activation:{identifier.AccountId:N}", cancellationToken);
            var account = await identityDb.Accounts
                .SingleOrDefaultAsync(x => x.Id == identifier.AccountId, cancellationToken);
            var localUser = await platformDb.LocalUsers.AsNoTracking()
                .SingleOrDefaultAsync(x => x.PersonId == identifier.AccountId, cancellationToken);
            var now = DateTimeOffset.UtcNow;
            var challenge = await identityDb.ActivationChallenges
                .SingleOrDefaultAsync(x => x.AccountId == identifier.AccountId, cancellationToken);
            if (generatedOnPreviousAttempt is not null && challenge is not null &&
                challenge.ConsumedAtUtc is null && challenge.ExpiresAtUtc > now &&
                CryptographicOperations.FixedTimeEquals(
                    challenge.CodeHash,
                    HashCode(identifier.AccountId, generatedOnPreviousAttempt)))
            {
                // A prior commit may have succeeded while its acknowledgement
                // was lost. Send the code that actually reached the database.
                await transaction.CommitAsync(cancellationToken);
                return generatedOnPreviousAttempt;
            }

            if (account?.Status != AccountStatus.PendingVerification ||
                localUser is null || localUser.Status == "Disabled" ||
                challenge is not null &&
                (now - challenge.LastSentAtUtc < SendCooldown ||
                 now - challenge.SendWindowStartUtc < SendWindow &&
                 challenge.SendCount >= MaximumDailySends))
            {
                await transaction.CommitAsync(cancellationToken);
                return (string?)null;
            }

            var generatedCode = RandomNumberGenerator.GetInt32(0, 100_000_000).ToString("D8");
            generatedOnPreviousAttempt = generatedCode;
            var hash = HashCode(identifier.AccountId, generatedCode);
            if (challenge is null)
            {
                challenge = new AccountActivationChallenge
                {
                    AccountId = identifier.AccountId,
                    CreatedAtUtc = now,
                    SendWindowStartUtc = now,
                    SendCount = 1,
                    LastSentAtUtc = now,
                    ExpiresAtUtc = now.Add(CodeLifetime),
                    CodeHash = hash
                };
                identityDb.ActivationChallenges.Add(challenge);
            }
            else
            {
                if (now - challenge.SendWindowStartUtc >= SendWindow)
                {
                    challenge.SendWindowStartUtc = now;
                    challenge.SendCount = 0;
                }

                challenge.CreatedAtUtc = now;
                challenge.ExpiresAtUtc = now.Add(CodeLifetime);
                challenge.LastSentAtUtc = now;
                challenge.ConsumedAtUtc = null;
                challenge.FailedAttempts = 0;
                challenge.SendCount++;
                challenge.CodeHash = hash;
            }

            AddSecurityEvent(identifier.AccountId, "platform.first_owner.activation.requested", true, now);
            await identityDb.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return generatedCode;
        });
        if (code is null)
        {
            return;
        }

        try
        {
            await smsSender.SendAsync(normalized,
                $"Mdaresna Platform activation code: {code}. Expires in 10 minutes. Do not share it.",
                "otp",
                null,
                cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning("Platform first-owner activation SMS delivery failed.");
            // An undelivered code must never be accepted if the provider rejects it.
            try
            {
                await InvalidateUndeliveredCodeAsync(identifier.AccountId, code, cancellationToken);
            }
            catch (Exception invalidationException) when (invalidationException is not OperationCanceledException)
            {
                // Keep the public response indistinguishable. The unknown code
                // expires quickly even if this defensive invalidation fails.
                logger.LogError("Failed to invalidate an undelivered activation code.");
            }
        }
    }

    public async Task<bool> CompleteAsync(
        string phone,
        string code,
        string password,
        CancellationToken cancellationToken = default)
    {
        var normalized = phone?.Trim();
        if (string.IsNullOrWhiteSpace(normalized) || normalized.Length > 32 ||
            code is not { Length: 8 } || !code.All(char.IsAsciiDigit) ||
            !IsValidPassword(password))
        {
            return false;
        }

        var identifier = await identityDb.LoginIdentifiers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Type == LoginIdentifierType.Phone &&
                                       x.SchoolId == null &&
                                       x.NormalizedValue == normalized,
                cancellationToken);
        if (identifier is null || identifier.IsVerified ||
            !await IsProvisionedFirstOwnerAsync(identifier.AccountId, cancellationToken))
        {
            return false;
        }

        var completionCommittedOnPreviousAttempt = false;
        var strategy = identityDb.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            identityDb.ChangeTracker.Clear();
            await using var transaction = await identityDb.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);
            await DatabaseAdvisoryLock.AcquireAsync(identityDb, transaction,
                $"mdaresna-platform-first-owner-activation:{identifier.AccountId:N}", cancellationToken);
            var currentIdentifier = await identityDb.LoginIdentifiers
                .SingleOrDefaultAsync(x => x.Id == identifier.Id, cancellationToken);
            var account = await identityDb.Accounts
                .SingleOrDefaultAsync(x => x.Id == identifier.AccountId, cancellationToken);
            var localUser = await platformDb.LocalUsers
                .Include(x => x.Credential)
                .SingleOrDefaultAsync(x => x.PersonId == identifier.AccountId, cancellationToken);
            var challenge = await identityDb.ActivationChallenges
                .SingleOrDefaultAsync(x => x.AccountId == identifier.AccountId, cancellationToken);
            var now = DateTimeOffset.UtcNow;
            if (completionCommittedOnPreviousAttempt &&
                currentIdentifier?.IsVerified == true &&
                account?.Status == AccountStatus.Active &&
                localUser?.Credential is { } previousCredential &&
                challenge?.ConsumedAtUtc is not null &&
                CryptographicOperations.FixedTimeEquals(
                    challenge.CodeHash, HashCode(identifier.AccountId, code)) &&
                passwordHasher.VerifyHashedPassword(
                account, previousCredential.PasswordHash, password) !=
                PasswordVerificationResult.Failed)
            {
                await transaction.CommitAsync(cancellationToken);
                return true;
            }

            if (currentIdentifier is null || currentIdentifier.IsVerified ||
                account?.Status != AccountStatus.PendingVerification ||
                localUser is null || localUser.Status == "Disabled" || challenge is null ||
                challenge.ConsumedAtUtc is not null || challenge.ExpiresAtUtc <= now ||
                challenge.FailedAttempts >= MaximumFailedAttempts)
            {
                return false;
            }

            var suppliedHash = HashCode(identifier.AccountId, code);
            if (!CryptographicOperations.FixedTimeEquals(challenge.CodeHash, suppliedHash))
            {
                challenge.FailedAttempts++;
                if (challenge.FailedAttempts >= MaximumFailedAttempts)
                {
                    challenge.ConsumedAtUtc = now;
                }

                AddSecurityEvent(identifier.AccountId, "platform.first_owner.activation.code_rejected", false, now);
                await identityDb.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return false;
            }

            // Commit the local credential first. If the Identity commit fails,
            // the central account remains pending and login stays closed; the
            // same valid OTP/password can retry without replacing the hash.
            if (localUser.Credential is null)
            {
                localUser.Credential = credentialFactory.CreateLocal(
                    account, localUser, password, now);
            }
            else if (passwordHasher.VerifyHashedPassword(
                         account, localUser.Credential.PasswordHash, password) ==
                     PasswordVerificationResult.Failed)
            {
                return false;
            }
            localUser.Status = "Active";
            localUser.UpdatedAtUtc = now;
            await platformDb.SaveChangesAsync(cancellationToken);

            currentIdentifier.IsVerified = true;
            currentIdentifier.VerifiedAtUtc = now;
            account.Status = AccountStatus.Active;
            account.UpdatedAtUtc = now;
            challenge.ConsumedAtUtc = now;
            AddSecurityEvent(identifier.AccountId, "platform.first_owner.activation.completed", true, now);
            await identityDb.SaveChangesAsync(cancellationToken);
            completionCommittedOnPreviousAttempt = true;
            await transaction.CommitAsync(cancellationToken);
            return true;
        });
    }

    private async Task<bool> IsProvisionedFirstOwnerAsync(Guid accountId, CancellationToken ct)
    {
        var owner = IdentityAccountId.From(accountId);
        return await platformDb.AuditEntries.AsNoTracking().AnyAsync(
                   x => x.AccountId == owner && x.Action == ProvisionedAuditAction, ct) &&
               await (from assignment in platformDb.RoleAssignments.AsNoTracking()
                      join role in platformDb.Roles.AsNoTracking()
                          on assignment.RoleId equals role.Id
                      where assignment.AccountId == owner &&
                            assignment.RevokedAtUtc == null &&
                            role.IsActive && role.IsSystem && role.Key == "app-manager"
                      select assignment.Id).AnyAsync(ct);
    }

    private byte[] HashCode(Guid accountId, string code) =>
        HMACSHA256.HashData(options.CodeHashKey,
            Encoding.UTF8.GetBytes($"mdaresna-platform-first-owner:{accountId:N}:{code}"));

    private static bool IsValidPassword(string? password) =>
        password is { Length: >= 12 and <= 1024 } &&
        password.All(character => !char.IsControl(character));

    private void AddSecurityEvent(Guid accountId, string eventType, bool succeeded, DateTimeOffset now) =>
        identityDb.SecurityEvents.Add(new IdentitySecurityEvent
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            EventType = eventType,
            Succeeded = succeeded,
            OccurredAtUtc = now
        });

    private async Task InvalidateUndeliveredCodeAsync(Guid accountId, string code, CancellationToken ct)
    {
        var strategy = identityDb.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            identityDb.ChangeTracker.Clear();
            await using var transaction = await identityDb.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, ct);
            await DatabaseAdvisoryLock.AcquireAsync(identityDb, transaction,
                $"mdaresna-platform-first-owner-activation:{accountId:N}", ct);
            var challenge = await identityDb.ActivationChallenges
                .SingleOrDefaultAsync(x => x.AccountId == accountId, ct);
            if (challenge is not null && challenge.ConsumedAtUtc is null &&
                CryptographicOperations.FixedTimeEquals(challenge.CodeHash, HashCode(accountId, code)))
            {
                var now = DateTimeOffset.UtcNow;
                challenge.ConsumedAtUtc = now;
                AddSecurityEvent(accountId, "platform.first_owner.activation.sms_failed", false, now);
                await identityDb.SaveChangesAsync(ct);
            }

            await transaction.CommitAsync(ct);
        });
    }

}
