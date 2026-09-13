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
using Microsoft.Extensions.Logging;

namespace Mdaresna.Platform.Infrastructure.IdentityAuth;

/// <summary>
/// Phone-OTP recovery for an existing, active Platform operator. A separate
/// challenge and HMAC purpose keep this flow independent of first activation.
/// </summary>
public sealed class PlatformPasswordResetService(
    IdentityDbContext identityDb,
    PlatformDbContext platformDb,
    IPlatformSmsSender smsSender,
    IPasswordHasher<Account> passwordHasher,
    PlatformActivationOptions options,
    ILogger<PlatformPasswordResetService> logger)
{
    private const int MaximumFailedAttempts = 5;
    private const int MaximumDailySends = 10;
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan SendCooldown = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan SendWindow = TimeSpan.FromDays(1);

    public async Task StartAsync(string phone, CancellationToken cancellationToken = default)
    {
        var normalized = phone?.Trim();
        if (!IsValidPhone(normalized)) return;

        var identifier = await identityDb.LoginIdentifiers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Type == LoginIdentifierType.Phone &&
                                       x.SchoolId == null &&
                                       x.NormalizedValue == normalized,
                cancellationToken);
        if (identifier?.IsVerified != true ||
            !await HasPlatformAccessAsync(identifier.AccountId, cancellationToken))
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
                $"mdaresna-platform-password-reset:{identifier.AccountId:N}", cancellationToken);
            var account = await identityDb.Accounts.Include(x => x.PasswordCredential)
                .SingleOrDefaultAsync(x => x.Id == identifier.AccountId, cancellationToken);
            var currentIdentifier = await identityDb.LoginIdentifiers
                .SingleOrDefaultAsync(x => x.Id == identifier.Id, cancellationToken);
            var challenge = await identityDb.PasswordResetChallenges
                .SingleOrDefaultAsync(x => x.AccountId == identifier.AccountId, cancellationToken);
            var now = DateTimeOffset.UtcNow;

            if (generatedOnPreviousAttempt is not null && challenge is not null &&
                challenge.ConsumedAtUtc is null && challenge.ExpiresAtUtc > now &&
                CryptographicOperations.FixedTimeEquals(
                    challenge.CodeHash, HashCode(identifier.AccountId, generatedOnPreviousAttempt)))
            {
                await transaction.CommitAsync(cancellationToken);
                return generatedOnPreviousAttempt;
            }

            if (account?.Status != AccountStatus.Active || account.PasswordCredential is null ||
                currentIdentifier?.IsVerified != true ||
                currentIdentifier.Type != LoginIdentifierType.Phone ||
                currentIdentifier.SchoolId is not null ||
                currentIdentifier.NormalizedValue != normalized ||
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
            if (challenge is null)
            {
                challenge = new AccountPasswordResetChallenge
                {
                    AccountId = identifier.AccountId,
                    CodeHash = HashCode(identifier.AccountId, generatedCode),
                    CreatedAtUtc = now,
                    ExpiresAtUtc = now.Add(CodeLifetime),
                    LastSentAtUtc = now,
                    SendWindowStartUtc = now,
                    SendCount = 1
                };
                identityDb.PasswordResetChallenges.Add(challenge);
            }
            else
            {
                if (now - challenge.SendWindowStartUtc >= SendWindow)
                {
                    challenge.SendWindowStartUtc = now;
                    challenge.SendCount = 0;
                }

                challenge.CodeHash = HashCode(identifier.AccountId, generatedCode);
                challenge.CreatedAtUtc = now;
                challenge.ExpiresAtUtc = now.Add(CodeLifetime);
                challenge.ConsumedAtUtc = null;
                challenge.LastSentAtUtc = now;
                challenge.SendCount++;
                challenge.FailedAttempts = 0;
            }

            AddSecurityEvent(identifier.AccountId, "platform.password_reset.requested", true, now);
            await identityDb.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return generatedCode;
        });

        if (code is null) return;
        try
        {
            await smsSender.SendAsync(normalized!,
                $"Mdaresna Platform password reset code: {code}. Expires in 10 minutes. Do not share it.",
                "otp", null, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning("Platform password reset SMS delivery failed.");
            try
            {
                await InvalidateUndeliveredCodeAsync(identifier.AccountId, code, cancellationToken);
            }
            catch (Exception invalidationException) when (invalidationException is not OperationCanceledException)
            {
                logger.LogError("Failed to invalidate an undelivered password reset code.");
            }
        }
    }

    public async Task<bool> CompleteAsync(
        string phone, string code, string newPassword,
        CancellationToken cancellationToken = default)
    {
        var normalized = phone?.Trim();
        if (!IsValidPhone(normalized) || code is not { Length: 8 } ||
            !code.All(char.IsAsciiDigit) || !IsValidPassword(newPassword))
        {
            return false;
        }

        var identifier = await identityDb.LoginIdentifiers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Type == LoginIdentifierType.Phone &&
                                       x.SchoolId == null &&
                                       x.NormalizedValue == normalized,
                cancellationToken);
        if (identifier?.IsVerified != true ||
            !await HasPlatformAccessAsync(identifier.AccountId, cancellationToken))
        {
            return false;
        }

        var strategy = identityDb.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            identityDb.ChangeTracker.Clear();
            await using var transaction = await identityDb.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);
            await DatabaseAdvisoryLock.AcquireAsync(identityDb, transaction,
                $"mdaresna-platform-password-reset:{identifier.AccountId:N}", cancellationToken);
            var account = await identityDb.Accounts.Include(x => x.PasswordCredential)
                .SingleOrDefaultAsync(x => x.Id == identifier.AccountId, cancellationToken);
            var currentIdentifier = await identityDb.LoginIdentifiers
                .SingleOrDefaultAsync(x => x.Id == identifier.Id, cancellationToken);
            var challenge = await identityDb.PasswordResetChallenges
                .SingleOrDefaultAsync(x => x.AccountId == identifier.AccountId, cancellationToken);
            var now = DateTimeOffset.UtcNow;
            if (account?.Status != AccountStatus.Active || account.PasswordCredential is null ||
                currentIdentifier?.IsVerified != true ||
                currentIdentifier.Type != LoginIdentifierType.Phone ||
                currentIdentifier.SchoolId is not null ||
                currentIdentifier.NormalizedValue != normalized || challenge is null ||
                challenge.ConsumedAtUtc is not null || challenge.ExpiresAtUtc <= now ||
                challenge.FailedAttempts >= MaximumFailedAttempts)
            {
                return false;
            }

            if (!CryptographicOperations.FixedTimeEquals(
                    challenge.CodeHash, HashCode(identifier.AccountId, code)))
            {
                challenge.FailedAttempts++;
                if (challenge.FailedAttempts >= MaximumFailedAttempts)
                    challenge.ConsumedAtUtc = now;
                AddSecurityEvent(identifier.AccountId, "platform.password_reset.code_rejected", false, now);
                await identityDb.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return false;
            }

            var credential = account.PasswordCredential;
            credential.PasswordHash = passwordHasher.HashPassword(account, newPassword);
            credential.HashingAlgorithm = PlatformPasswordCredentialFactory.Algorithm;
            credential.HashingVersion = PlatformPasswordCredentialFactory.Version;
            credential.SecurityStamp = Guid.NewGuid().ToString("N");
            credential.FailedSignInCount = 0;
            credential.LockoutEndUtc = null;
            credential.MustChangePassword = false;
            credential.ChangedAtUtc = now;
            account.UpdatedAtUtc = now;
            challenge.ConsumedAtUtc = now;

            var sessions = await identityDb.Sessions
                .Where(x => x.AccountId == identifier.AccountId &&
                            x.RevokedAtUtc == null && x.ExpiresAtUtc > now)
                .ToArrayAsync(cancellationToken);
            foreach (var session in sessions)
            {
                session.RevokedAtUtc = now;
                session.RevocationReason = "password-reset";
            }

            AddSecurityEvent(identifier.AccountId, "platform.password_reset.completed", true, now);
            await identityDb.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        });
    }

    private async Task<bool> HasPlatformAccessAsync(Guid accountId, CancellationToken ct)
    {
        var owner = IdentityAccountId.From(accountId);
        return await (from assignment in platformDb.RoleAssignments.AsNoTracking()
                      join role in platformDb.Roles.AsNoTracking()
                          on assignment.RoleId equals role.Id
                      where assignment.AccountId == owner &&
                            assignment.RevokedAtUtc == null && role.IsActive
                      select assignment.Id).AnyAsync(ct);
    }

    private byte[] HashCode(Guid accountId, string code) =>
        HMACSHA256.HashData(options.CodeHashKey,
            Encoding.UTF8.GetBytes($"mdaresna-platform-password-reset:{accountId:N}:{code}"));

    private static bool IsValidPhone(string? phone) =>
        phone is { Length: >= 8 and <= 16 } && phone.All(char.IsAsciiDigit);

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
                $"mdaresna-platform-password-reset:{accountId:N}", ct);
            var challenge = await identityDb.PasswordResetChallenges
                .SingleOrDefaultAsync(x => x.AccountId == accountId, ct);
            if (challenge is not null && challenge.ConsumedAtUtc is null &&
                CryptographicOperations.FixedTimeEquals(challenge.CodeHash, HashCode(accountId, code)))
            {
                var now = DateTimeOffset.UtcNow;
                challenge.ConsumedAtUtc = now;
                AddSecurityEvent(accountId, "platform.password_reset.sms_failed", false, now);
                await identityDb.SaveChangesAsync(ct);
            }
            await transaction.CommitAsync(ct);
        });
    }

}
