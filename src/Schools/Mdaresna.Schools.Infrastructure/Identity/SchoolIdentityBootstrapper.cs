using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Infrastructure.Identity;

public interface ISchoolIdentityBootstrapper
{
    Task<SchoolOwnerBootstrapResult> BootstrapOwnerAsync(SchoolsDbContext db, Guid platformAccountId,
        CancellationToken cancellationToken = default);
}

public sealed record SchoolOwnerBootstrapResult(Guid UserId, string UserName);

internal sealed class SchoolIdentityBootstrapper : ISchoolIdentityBootstrapper
{
    public async Task<SchoolOwnerBootstrapResult> BootstrapOwnerAsync(SchoolsDbContext db, Guid platformAccountId,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (platformAccountId == Guid.Empty) throw new ArgumentException("Platform account is required.", nameof(platformAccountId));
        const string userName = "owner";
        var existing = await db.LocalUsers.SingleOrDefaultAsync(x => x.PlatformAccountId == platformAccountId, cancellationToken);
        if (existing is not null)
        {
            var obsoleteChallenge = await db.LocalUserActivationChallenges
                .SingleOrDefaultAsync(x => x.UserId == existing.Id, cancellationToken);
            if (obsoleteChallenge is { ConsumedAtUtc: null })
            {
                obsoleteChallenge.ConsumedAtUtc = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync(cancellationToken);
            }
            return new(existing.Id, existing.UserName);
        }
        if (!await db.LocalRoles.AnyAsync(x => x.Id == SchoolIdentitySeed.SchoolAdminRoleId, cancellationToken))
            throw new InvalidOperationException("School identity seed data has not been applied.");

        var now = DateTimeOffset.UtcNow;
        var person = new Person { Id = Guid.NewGuid(), DisplayName = "صاحب المدرسة",
            Status = PersonStatus.Active, CreatedAtUtc = now, UpdatedAtUtc = now };
        var user = new LocalUserAccount { Id = Guid.NewGuid(), PersonId = person.Id, PlatformAccountId = platformAccountId,
            Person = person, UserName = userName, NormalizedUserName = userName.ToUpperInvariant(), Status = LocalUserStatus.PendingActivation,
            PermissionsVersion = 1, CreatedAtUtc = now, UpdatedAtUtc = now };
        user.Roles.Add(new LocalUserRole { UserId = user.Id, RoleId = SchoolIdentitySeed.SchoolAdminRoleId,
            User = user, AssignedAtUtc = now });
        db.Persons.Add(person); db.LocalUsers.Add(user);
        await db.SaveChangesAsync(cancellationToken);
        return new(user.Id, userName);
    }
}
