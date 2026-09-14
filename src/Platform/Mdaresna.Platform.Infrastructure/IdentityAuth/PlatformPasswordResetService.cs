using System.Data;
using System.Security.Cryptography;
using System.Text;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Mdaresna.Platform.Infrastructure.IdentityAuth;

/// <summary>
/// The verified central phone identifies the person; Platform owns both the
/// reset challenge and the credential so reset is a single-database change.
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
        var target = await FindTargetAsync(phone, cancellationToken);
        if (target is null) return;
        string? generatedOnPreviousAttempt = null;
        var strategy = platformDb.Database.CreateExecutionStrategy();
        var code = await strategy.ExecuteAsync(async () =>
        {
            platformDb.ChangeTracker.Clear();
            await using var transaction = await platformDb.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);
            await DatabaseAdvisoryLock.AcquireAsync(platformDb, transaction,
                $"mdaresna-platform-password-reset:{target.UserId:N}", cancellationToken);
            var user = await platformDb.LocalUsers.Include(x => x.Credential)
                .SingleOrDefaultAsync(x => x.Id == target.UserId, cancellationToken);
            var challenge = await platformDb.LocalPasswordResetChallenges
                .SingleOrDefaultAsync(x => x.UserId == target.UserId, cancellationToken);
            var now = DateTimeOffset.UtcNow;

            if (generatedOnPreviousAttempt is not null && challenge is not null &&
                challenge.ConsumedAtUtc is null && challenge.ExpiresAtUtc > now &&
                CryptographicOperations.FixedTimeEquals(challenge.CodeHash,
                    HashCode(target.PersonId, generatedOnPreviousAttempt)))
            {
                await transaction.CommitAsync(cancellationToken);
                return generatedOnPreviousAttempt;
            }

            if (user?.Status != "Active" || user.Credential is null ||
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
                challenge = new PlatformLocalPasswordResetChallenge
                {
                    UserId = target.UserId,
                    CodeHash = HashCode(target.PersonId, generatedCode),
                    CreatedAtUtc = now,
                    ExpiresAtUtc = now.Add(CodeLifetime),
                    LastSentAtUtc = now,
                    SendWindowStartUtc = now,
                    SendCount = 1
                };
                platformDb.LocalPasswordResetChallenges.Add(challenge);
            }
            else
            {
                if (now - challenge.SendWindowStartUtc >= SendWindow)
                {
                    challenge.SendWindowStartUtc = now;
                    challenge.SendCount = 0;
                }
                challenge.CodeHash = HashCode(target.PersonId, generatedCode);
                challenge.CreatedAtUtc = now;
                challenge.ExpiresAtUtc = now.Add(CodeLifetime);
                challenge.ConsumedAtUtc = null;
                challenge.LastSentAtUtc = now;
                challenge.SendCount++;
                challenge.FailedAttempts = 0;
            }
            AddAudit(target.PersonId, "platform.password_reset.requested", now);
            await platformDb.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return generatedCode;
        });

        if (code is null) return;
        try
        {
            await smsSender.SendAsync(target.Phone,
                $"Mdaresna Platform password reset code: {code}. Expires in 10 minutes. Do not share it.",
                "otp", null, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            logger.LogWarning("Platform password reset SMS delivery failed.");
            try { await InvalidateUndeliveredCodeAsync(target, code, cancellationToken); }
            catch (Exception invalidationException) when (invalidationException is not OperationCanceledException)
            {
                logger.LogError("Failed to invalidate an undelivered Platform reset code.");
            }
        }
    }

    public async Task<bool> CompleteAsync(
        string phone, string code, string newPassword,
        CancellationToken cancellationToken = default)
    {
        if (code is not { Length: 8 } || !code.All(char.IsAsciiDigit) ||
            !IsValidPassword(newPassword)) return false;
        var target = await FindTargetAsync(phone, cancellationToken);
        if (target is null) return false;
        var strategy = platformDb.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync(async () =>
        {
            platformDb.ChangeTracker.Clear();
            await using var transaction = await platformDb.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, cancellationToken);
            await DatabaseAdvisoryLock.AcquireAsync(platformDb, transaction,
                $"mdaresna-platform-password-reset:{target.UserId:N}", cancellationToken);
            var user = await platformDb.LocalUsers.Include(x => x.Credential)
                .SingleOrDefaultAsync(x => x.Id == target.UserId, cancellationToken);
            var challenge = await platformDb.LocalPasswordResetChallenges
                .SingleOrDefaultAsync(x => x.UserId == target.UserId, cancellationToken);
            var now = DateTimeOffset.UtcNow;
            if (user?.Status != "Active" || user.Credential is null ||
                challenge is null || challenge.ConsumedAtUtc is not null ||
                challenge.ExpiresAtUtc <= now || challenge.FailedAttempts >= MaximumFailedAttempts)
                return false;

            if (!CryptographicOperations.FixedTimeEquals(challenge.CodeHash,
                    HashCode(target.PersonId, code)))
            {
                challenge.FailedAttempts++;
                if (challenge.FailedAttempts >= MaximumFailedAttempts) challenge.ConsumedAtUtc = now;
                AddAudit(target.PersonId, "platform.password_reset.code_rejected", now);
                await platformDb.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);
                return false;
            }

            var credential = user.Credential;
            credential.PasswordHash = passwordHasher.HashPassword(target.Account, newPassword);
            credential.HashingAlgorithm = PlatformPasswordCredentialFactory.Algorithm;
            credential.HashingVersion = PlatformPasswordCredentialFactory.Version;
            credential.SecurityStamp = Guid.NewGuid().ToString("N");
            credential.FailedSignInCount = 0;
            credential.LockoutEndUtc = null;
            credential.MustChangePassword = false;
            credential.ChangedAtUtc = now;
            challenge.ConsumedAtUtc = now;
            AddAudit(target.PersonId, "platform.password_reset.completed", now);
            await platformDb.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return true;
        });
    }

    private async Task<Target?> FindTargetAsync(string? phone, CancellationToken ct)
    {
        var normalized = phone?.Trim();
        if (normalized is not { Length: >= 8 and <= 16 } ||
            !normalized.All(char.IsAsciiDigit)) return null;
        var identifier = await identityDb.LoginIdentifiers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Type == LoginIdentifierType.Phone &&
                x.SchoolId == null && x.NormalizedValue == normalized && x.IsVerified, ct);
        if (identifier is null) return null;
        var account = await identityDb.Accounts.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == identifier.AccountId &&
                x.Status == AccountStatus.Active, ct);
        if (account is null) return null;
        var local = await platformDb.LocalUsers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.PersonId == account.Id && x.Status == "Active", ct);
        if (local is null || !await (from assignment in platformDb.RoleAssignments.AsNoTracking()
            join role in platformDb.Roles.AsNoTracking() on assignment.RoleId equals role.Id
            where assignment.AccountId == IdentityAccountId.From(account.Id) &&
                assignment.RevokedAtUtc == null && role.IsActive
            select assignment.Id).AnyAsync(ct)) return null;
        return new Target(account, local.Id, normalized);
    }

    private byte[] HashCode(Guid personId, string code) =>
        HMACSHA256.HashData(options.CodeHashKey,
            Encoding.UTF8.GetBytes($"mdaresna-platform-password-reset:{personId:N}:{code}"));

    private static bool IsValidPassword(string? password) =>
        password is { Length: >= 12 and <= 1024 } &&
        password.All(character => !char.IsControl(character));

    private void AddAudit(Guid personId, string action, DateTimeOffset now) =>
        platformDb.AuditEntries.Add(new PlatformAuditEntry
        {
            Id = Guid.NewGuid(),
            AccountId = IdentityAccountId.From(personId),
            Action = action,
            ResourceType = "platform-local-credential",
            ResourceId = personId.ToString("D"),
            OccurredAtUtc = now
        });

    private async Task InvalidateUndeliveredCodeAsync(Target target, string code, CancellationToken ct)
    {
        var strategy = platformDb.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            platformDb.ChangeTracker.Clear();
            await using var transaction = await platformDb.Database.BeginTransactionAsync(
                IsolationLevel.Serializable, ct);
            await DatabaseAdvisoryLock.AcquireAsync(platformDb, transaction,
                $"mdaresna-platform-password-reset:{target.UserId:N}", ct);
            var challenge = await platformDb.LocalPasswordResetChallenges
                .SingleOrDefaultAsync(x => x.UserId == target.UserId, ct);
            if (challenge is not null && challenge.ConsumedAtUtc is null &&
                CryptographicOperations.FixedTimeEquals(challenge.CodeHash,
                    HashCode(target.PersonId, code)))
            {
                var now = DateTimeOffset.UtcNow;
                challenge.ConsumedAtUtc = now;
                AddAudit(target.PersonId, "platform.password_reset.sms_failed", now);
                await platformDb.SaveChangesAsync(ct);
            }
            await transaction.CommitAsync(ct);
        });
    }

    private sealed record Target(Account Account, Guid UserId, string Phone)
    {
        public Guid PersonId => Account.Id;
    }
}
