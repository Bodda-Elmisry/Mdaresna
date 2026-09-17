using System.Security.Cryptography;
using System.Text;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Application.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Infrastructure.Identity;

public sealed class SchoolOwnerActivationService(
    ISchoolDbContextFactory dbFactory,
    IPasswordHasher<LocalUserAccount> passwordHasher)
{
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
        var user = await db.LocalUsers.SingleOrDefaultAsync(
            x => x.NormalizedUserName == normalized, cancellationToken);
        if (user is null) return false;
        var challenge = await db.LocalUserActivationChallenges.SingleOrDefaultAsync(
            x => x.UserId == user.Id, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        if (user.Status != LocalUserStatus.PendingActivation || challenge is null ||
            challenge.ConsumedAtUtc is not null || challenge.ExpiresAtUtc <= now ||
            challenge.FailedAttempts >= 5) return false;
        var supplied = SHA256.HashData(challenge.CodeSalt.Concat(Encoding.UTF8.GetBytes(code)).ToArray());
        if (!CryptographicOperations.FixedTimeEquals(supplied, challenge.CodeHash))
        {
            challenge.FailedAttempts++;
            if (challenge.FailedAttempts >= 5) challenge.ConsumedAtUtc = now;
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
        return true;
    }
}
