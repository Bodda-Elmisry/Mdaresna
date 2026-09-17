using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace Mdaresna.Schools.Infrastructure.Identity;

public interface ISchoolIdentityBootstrapper
{
    Task<SchoolOwnerBootstrapResult> BootstrapOwnerAsync(SchoolsDbContext db, Guid platformAccountId,
        CancellationToken cancellationToken = default);
}

public sealed record SchoolOwnerBootstrapResult(Guid UserId, string UserName, string ActivationCode);

internal sealed class SchoolIdentityBootstrapper : ISchoolIdentityBootstrapper
{
    public async Task<SchoolOwnerBootstrapResult> BootstrapOwnerAsync(SchoolsDbContext db, Guid platformAccountId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (platformAccountId == Guid.Empty) throw new ArgumentException("Platform account is required.", nameof(platformAccountId));
        const string userName = "owner";
        var existing = await db.LocalUsers.SingleOrDefaultAsync(x => x.PlatformAccountId == platformAccountId, cancellationToken);
        var existingChallenge = existing is null ? null : await db.LocalUserActivationChallenges
            .SingleOrDefaultAsync(x => x.UserId == existing.Id, cancellationToken);
        if (existing is not null && existingChallenge is null)
            return new(existing.Id, existing.UserName, string.Empty);
        if (!await db.LocalRoles.AnyAsync(x => x.Id == SchoolIdentitySeed.SchoolAdminRoleId, cancellationToken))
            throw new InvalidOperationException("School identity seed data has not been applied.");

        var now = DateTimeOffset.UtcNow;
        var activationCode = RandomNumberGenerator.GetInt32(0, 100_000_000).ToString("D8");
        var salt = RandomNumberGenerator.GetBytes(32);
        if (existing is not null)
        {
            existingChallenge!.CodeSalt = salt;
            existingChallenge.CodeHash = HashCode(salt, activationCode);
            existingChallenge.ExpiresAtUtc = now.AddHours(24);
            existingChallenge.ConsumedAtUtc = null;
            existingChallenge.FailedAttempts = 0;
            await db.SaveChangesAsync(cancellationToken);
            return new(existing.Id, existing.UserName, activationCode);
        }
        var person = new Person { Id = Guid.NewGuid(), DisplayName = "صاحب المدرسة",
            Status = PersonStatus.Active, CreatedAtUtc = now, UpdatedAtUtc = now };
        var user = new LocalUserAccount { Id = Guid.NewGuid(), PersonId = person.Id, PlatformAccountId = platformAccountId,
            Person = person, UserName = userName, NormalizedUserName = userName.ToUpperInvariant(), Status = LocalUserStatus.PendingActivation,
            PermissionsVersion = 1, CreatedAtUtc = now, UpdatedAtUtc = now };
        user.Roles.Add(new LocalUserRole { UserId = user.Id, RoleId = SchoolIdentitySeed.SchoolAdminRoleId,
            User = user, AssignedAtUtc = now });
        db.Persons.Add(person); db.LocalUsers.Add(user);
        db.LocalUserActivationChallenges.Add(new LocalUserActivationChallenge { UserId = user.Id, User = user,
            CodeSalt = salt, CodeHash = HashCode(salt, activationCode), ExpiresAtUtc = now.AddHours(24) });
        await db.SaveChangesAsync(cancellationToken);
        return new(user.Id, userName, activationCode);
    }

    private static byte[] HashCode(byte[] salt, string code) =>
        SHA256.HashData(salt.Concat(System.Text.Encoding.UTF8.GetBytes(code)).ToArray());
}
