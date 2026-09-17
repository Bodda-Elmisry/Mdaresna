using Mdaresna.Schools.Application.Identity;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Infrastructure.Identity;

internal sealed class SchoolLocalLoginService(
    ISchoolLoginTenantResolver tenantResolver,
    IPasswordHasher<LocalUserAccount> passwordHasher) : ISchoolLoginService
{
    public async Task<SchoolLoginResult?> LoginAsync(string login, string password, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(password)) return null;
        SchoolLoginIdentifier identifier;
        try { identifier = SchoolLoginIdentifier.Parse(login); }
        catch (ArgumentException) { return null; }
        var target = await tenantResolver.ResolveAsync(identifier.SchoolCode, cancellationToken);
        if (target is null) return null;

        await using var db = CreateContext(target);
        var normalized = identifier.UserName.Trim().ToUpperInvariant();
        var user = await db.LocalUsers
            .Include(x => x.Person).Include(x => x.Credential)
            .Include(x => x.Roles).ThenInclude(x => x.Role)
                .ThenInclude(x => x.Permissions).ThenInclude(x => x.Permission)
            .SingleOrDefaultAsync(x => x.NormalizedUserName == normalized, cancellationToken);
        if (user?.Credential is null || user.Status != LocalUserStatus.Active) return null;

        var now = DateTimeOffset.UtcNow;
        if (user.Credential.LockoutEndUtc > now) return null;
        var verified = passwordHasher.VerifyHashedPassword(user, user.Credential.PasswordHash, password);
        if (verified == PasswordVerificationResult.Failed)
        {
            user.Credential.FailedSignInCount++;
            if (user.Credential.FailedSignInCount >= 5)
                user.Credential.LockoutEndUtc = now.AddMinutes(15);
            await db.SaveChangesAsync(cancellationToken);
            return null;
        }

        if (verified == PasswordVerificationResult.SuccessRehashNeeded)
            user.Credential.PasswordHash = passwordHasher.HashPassword(user, password);
        user.Credential.FailedSignInCount = 0; user.Credential.LockoutEndUtc = null;
        user.LastLoginAtUtc = now; user.UpdatedAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);

        var roles = user.Roles.Where(x => x.Role.IsActive).Select(x => x.Role.Code)
            .Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        var permissions = user.Roles.Where(x => x.Role.IsActive)
            .SelectMany(x => x.Role.Permissions).Where(x => x.Permission.IsActive)
            .Select(x => x.Permission.Code).Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal).ToArray();
        return new SchoolLoginResult(user.Id, user.PersonId, target.TenantId, target.SchoolId,
            target.SchoolCode, user.UserName, user.Person.DisplayName, user.Credential.SecurityStamp,
            user.PermissionsVersion, roles, permissions);
    }

    private static SchoolsDbContext CreateContext(SchoolLoginTarget target)
    {
        if (target.Provider.Equals("PostgreSql", StringComparison.OrdinalIgnoreCase))
            return new PostgreSqlSchoolsDbContext(new DbContextOptionsBuilder<PostgreSqlSchoolsDbContext>()
                .UseNpgsql(target.ConnectionString).Options);
        if (target.Provider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
            return new SqlServerSchoolsDbContext(new DbContextOptionsBuilder<SqlServerSchoolsDbContext>()
                .UseSqlServer(target.ConnectionString).Options);
        throw new InvalidOperationException($"Unsupported school database provider '{target.Provider}'.");
    }
}
