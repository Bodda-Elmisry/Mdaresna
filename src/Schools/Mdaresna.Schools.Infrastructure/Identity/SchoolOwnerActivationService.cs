using System.Security.Cryptography;
using System.Text;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Application.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Infrastructure.Identity;

public sealed class SchoolOwnerActivationService(
    ISchoolDbContextFactory dbFactory,
    IPasswordHasher<LocalUserAccount> passwordHasher,
    ISchoolOwnerActivationCodeSender codeSender)
{
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan SendCooldown = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan SendWindow = TimeSpan.FromHours(24);
    private const int MaximumDailySends = 5;
    private const int MaximumFailedAttempts = 5;

    public async Task<SchoolOwnerActivationStartResult> StartAsync(string login,
        CancellationToken cancellationToken = default)
    {
        SchoolLoginIdentifier identifier;
        try { identifier = SchoolLoginIdentifier.Parse(login); }
        catch (ArgumentException) { return SchoolOwnerActivationStartResult.Accepted; }
        await using var db = await dbFactory.CreateAsync(identifier.SchoolCode, cancellationToken);
        if (db is null) return SchoolOwnerActivationStartResult.Accepted;
        var normalized = identifier.UserName.Trim().ToUpperInvariant();
        var user = await db.LocalUsers.SingleOrDefaultAsync(
            x => x.NormalizedUserName == normalized, cancellationToken);
        if (user is null || user.Status != LocalUserStatus.PendingActivation ||
            user.PlatformAccountId is null) return SchoolOwnerActivationStartResult.Accepted;
        var school = await db.SchoolInformation.AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        if (school is null) return SchoolOwnerActivationStartResult.Accepted;

        var challenge = await db.LocalUserActivationChallenges.SingleOrDefaultAsync(
            x => x.UserId == user.Id, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        if (challenge?.LastSentAtUtc is { } lastSent && challenge.ConsumedAtUtc is null &&
            now - lastSent < SendCooldown) return SchoolOwnerActivationStartResult.Accepted;
        if (challenge?.SendWindowStartUtc is { } windowStart && now - windowStart < SendWindow &&
            challenge.SendCount >= MaximumDailySends) return SchoolOwnerActivationStartResult.Accepted;

        var code = RandomNumberGenerator.GetInt32(0, 100_000_000).ToString("D8");
        var salt = RandomNumberGenerator.GetBytes(32);
        if (challenge is null)
        {
            challenge = new LocalUserActivationChallenge
            {
                UserId = user.Id, User = user, CodeSalt = salt, CodeHash = HashCode(salt, code),
                ExpiresAtUtc = now.Add(CodeLifetime), LastSentAtUtc = now,
                SendWindowStartUtc = now, SendCount = 1
            };
            db.LocalUserActivationChallenges.Add(challenge);
        }
        else
        {
            if (challenge.SendWindowStartUtc is null || now - challenge.SendWindowStartUtc >= SendWindow)
            {
                challenge.SendWindowStartUtc = now;
                challenge.SendCount = 0;
            }
            challenge.CodeSalt = salt;
            challenge.CodeHash = HashCode(salt, code);
            challenge.ExpiresAtUtc = now.Add(CodeLifetime);
            challenge.ConsumedAtUtc = null;
            challenge.FailedAttempts = 0;
            challenge.LastSentAtUtc = now;
            challenge.SendCount++;
        }
        try { await db.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return SchoolOwnerActivationStartResult.Accepted; }

        var sent = await codeSender.SendAsync(user.PlatformAccountId.Value,
            school.PlatformSchoolReferenceId, $"{user.UserName}@{identifier.SchoolCode}", code, cancellationToken);
        if (sent) return SchoolOwnerActivationStartResult.Accepted;

        db.ChangeTracker.Clear();
        challenge = await db.LocalUserActivationChallenges.SingleOrDefaultAsync(
            x => x.UserId == user.Id, cancellationToken);
        if (challenge is { ConsumedAtUtc: null } &&
            CryptographicOperations.FixedTimeEquals(challenge.CodeHash, HashCode(salt, code)))
        {
            challenge.ConsumedAtUtc = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(cancellationToken);
        }
        return SchoolOwnerActivationStartResult.DeliveryFailed;
    }

    public async Task<bool> CompleteAsync(string login, string code, string password,
        CancellationToken cancellationToken = default)
    {
        if (code is not { Length: 8 } || !code.All(char.IsAsciiDigit) ||
            password is not { Length: >= 12 and <= 1024 } || password.Any(char.IsControl))
            return false;
        SchoolLoginIdentifier identifier;
        try { identifier = SchoolLoginIdentifier.Parse(login); }
        catch (ArgumentException) { return false; }
        await using var db = await dbFactory.CreateAsync(identifier.SchoolCode, cancellationToken);
        if (db is null) return false;
        var normalized = identifier.UserName.Trim().ToUpperInvariant();
        var user = await db.LocalUsers.Include(x => x.Credential).SingleOrDefaultAsync(
            x => x.NormalizedUserName == normalized, cancellationToken);
        if (user?.PlatformAccountId is null) return false;
        var school = await db.SchoolInformation.AsNoTracking().SingleOrDefaultAsync(cancellationToken);
        if (school is null) return false;
        var challenge = await db.LocalUserActivationChallenges.SingleOrDefaultAsync(
            x => x.UserId == user.Id, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        if (user.Status == LocalUserStatus.Active && user.Credential is not null &&
            challenge?.ConsumedAtUtc is not null &&
            passwordHasher.VerifyHashedPassword(user, user.Credential.PasswordHash, password) !=
            PasswordVerificationResult.Failed)
            return await codeSender.ConfirmAsync(user.PlatformAccountId.Value,
                school.PlatformSchoolReferenceId, cancellationToken);
        if (user.Status != LocalUserStatus.PendingActivation || challenge is null ||
            challenge.ConsumedAtUtc is not null || challenge.ExpiresAtUtc <= now ||
            challenge.FailedAttempts >= MaximumFailedAttempts) return false;
        var supplied = SHA256.HashData(challenge.CodeSalt.Concat(Encoding.UTF8.GetBytes(code)).ToArray());
        if (!CryptographicOperations.FixedTimeEquals(supplied, challenge.CodeHash))
        {
            challenge.FailedAttempts++;
            if (challenge.FailedAttempts >= MaximumFailedAttempts) challenge.ConsumedAtUtc = now;
            await db.SaveChangesAsync(cancellationToken);
            return false;
        }
        user.Credential = new LocalUserCredential
        {
            UserId = user.Id, User = user, SecurityStamp = Guid.NewGuid().ToString("N"),
            MustChangePassword = false, ChangedAtUtc = now
        };
        user.Credential.PasswordHash = passwordHasher.HashPassword(user, password);
        user.Status = LocalUserStatus.Active; user.UpdatedAtUtc = now;
        challenge.ConsumedAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);
        return await codeSender.ConfirmAsync(user.PlatformAccountId.Value,
            school.PlatformSchoolReferenceId, cancellationToken);
    }

    private static byte[] HashCode(byte[] salt, string code) =>
        SHA256.HashData(salt.Concat(Encoding.UTF8.GetBytes(code)).ToArray());
}

public enum SchoolOwnerActivationStartResult
{
    Accepted = 1,
    DeliveryFailed = 2
}
