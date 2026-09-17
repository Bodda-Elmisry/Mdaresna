using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Infrastructure.Identity;

public interface ISchoolIdentityBootstrapper
{
    Task<Guid> BootstrapOwnerAsync(SchoolsDbContext db, Guid platformAccountId, string displayName,
        string userName, string temporaryPassword, CancellationToken cancellationToken = default);
}

internal sealed class SchoolIdentityBootstrapper(IPasswordHasher<LocalUserAccount> passwordHasher)
    : ISchoolIdentityBootstrapper
{
    public async Task<Guid> BootstrapOwnerAsync(SchoolsDbContext db, Guid platformAccountId, string displayName,
        string userName, string temporaryPassword, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(db);
        if (platformAccountId == Guid.Empty) throw new ArgumentException("Platform account is required.", nameof(platformAccountId));
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName); ArgumentException.ThrowIfNullOrWhiteSpace(userName);
        if (string.IsNullOrEmpty(temporaryPassword) || temporaryPassword.Length < 8)
            throw new ArgumentException("Temporary password must contain at least 8 characters.", nameof(temporaryPassword));
        var existing = await db.LocalUsers.SingleOrDefaultAsync(x => x.PlatformAccountId == platformAccountId, cancellationToken);
        if (existing is not null) return existing.Id;
        var normalized = userName.Trim().ToUpperInvariant();
        if (userName.Contains('@') || await db.LocalUsers.AnyAsync(x => x.NormalizedUserName == normalized, cancellationToken))
            throw new InvalidOperationException("The requested school owner username is unavailable.");
        if (!await db.LocalRoles.AnyAsync(x => x.Id == SchoolIdentitySeed.SchoolAdminRoleId, cancellationToken))
            throw new InvalidOperationException("School identity seed data has not been applied.");

        var now = DateTimeOffset.UtcNow; var person = new Person { Id = Guid.NewGuid(), DisplayName = displayName.Trim(),
            Status = PersonStatus.Active, CreatedAtUtc = now, UpdatedAtUtc = now };
        var user = new LocalUserAccount { Id = Guid.NewGuid(), PersonId = person.Id, PlatformAccountId = platformAccountId,
            Person = person, UserName = userName.Trim(), NormalizedUserName = normalized, Status = LocalUserStatus.Active,
            PermissionsVersion = 1, CreatedAtUtc = now, UpdatedAtUtc = now };
        user.Credential = new LocalUserCredential { UserId = user.Id, User = user, SecurityStamp = Guid.NewGuid().ToString("N"),
            MustChangePassword = true, ChangedAtUtc = now };
        user.Credential.PasswordHash = passwordHasher.HashPassword(user, temporaryPassword);
        user.Roles.Add(new LocalUserRole { UserId = user.Id, RoleId = SchoolIdentitySeed.SchoolAdminRoleId,
            User = user, AssignedAtUtc = now });
        db.Persons.Add(person); db.LocalUsers.Add(user); await db.SaveChangesAsync(cancellationToken); return user.Id;
    }
}
