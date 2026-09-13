using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.IdentityAuth.Staff;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.UnitTests.Auth;

/// <summary>
/// Opt-in integration coverage against the already-migrated local development databases.
/// Both database transactions are always rolled back. Set both
/// MDARESNA_PLATFORM_STAFF_INTEGRATION_PLATFORM_CONNECTION and
/// MDARESNA_PLATFORM_STAFF_INTEGRATION_IDENTITY_CONNECTION to run this test.
/// </summary>
public sealed class PlatformPhoneStaffIntegrationTests
{
    [Fact]
    public async Task Verified_global_phone_is_eligible_for_role_and_visible_in_staff_directory()
    {
        var connections = GetApprovedLocalConnections();
        if (connections is null)
        {
            return;
        }

        var platformOptions = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseSqlServer(connections.Value.Platform)
            .Options;
        var identityOptions = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseSqlServer(connections.Value.Identity)
            .Options;

        var actorId = Guid.NewGuid();
        var targetId = Guid.NewGuid();
        var unverifiedTargetId = Guid.NewGuid();
        var schoolOnlyTargetId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var phone = $"+20{Random.Shared.NextInt64(10000000000, 99999999999)}";

        await using (var platformDb = new PlatformDbContext(platformOptions))
        await using (var identityDb = new IdentityDbContext(identityOptions))
        await using (var platformTransaction = await platformDb.Database.BeginTransactionAsync())
        await using (var identityTransaction = await identityDb.Database.BeginTransactionAsync())
        {
            try
            {
                AddAccount(identityDb, targetId, "Phone-only operator", now);
                AddIdentifier(identityDb, targetId, LoginIdentifierType.Phone,
                    phone, null, true, now);

                AddAccount(identityDb, unverifiedTargetId, "Unverified phone", now);
                AddIdentifier(identityDb, unverifiedTargetId, LoginIdentifierType.Phone,
                    phone + "1", null, false, now);

                AddAccount(identityDb, schoolOnlyTargetId, "School-only user", now);
                AddIdentifier(identityDb, schoolOnlyTargetId,
                    LoginIdentifierType.SchoolUsername, "school-user-" + Guid.NewGuid().ToString("N"),
                    Guid.NewGuid(), true, now);
                await identityDb.SaveChangesAsync();

                var role = PlatformRole.Create(
                    PlatformRoleId.New(),
                    "test-phone-" + Guid.NewGuid().ToString("N"),
                    "Phone staff test role",
                    isSystem: false,
                    [PlatformPermissionCodes.SchoolsRead],
                    IdentityAccountId.From(actorId),
                    now);
                platformDb.Roles.Add(role);
                await platformDb.SaveChangesAsync();

                var manager = new PlatformStaffRoleManager(
                    platformDb, identityDb, new AllowAllPermissions());
                var assignment = await manager.AssignAsync(actorId, targetId, role.Id.Value);
                Assert.True(assignment.Changed);
                Assert.Equal(targetId, assignment.AccountId);

                var unverifiedError = await Assert.ThrowsAsync<PlatformConflictException>(() =>
                    manager.AssignAsync(actorId, unverifiedTargetId, role.Id.Value));
                Assert.Equal("staff.verified_active_account_required", unverifiedError.Code);

                var schoolOnlyError = await Assert.ThrowsAsync<PlatformConflictException>(() =>
                    manager.AssignAsync(actorId, schoolOnlyTargetId, role.Id.Value));
                Assert.Equal("staff.verified_active_account_required", schoolOnlyError.Code);

                var directory = new PlatformStaffDirectory(platformDb, identityDb);
                var staff = await FindStaffAsync(directory, targetId);
                Assert.NotNull(staff);
                Assert.Equal("Phone-only operator", staff.DisplayName);
                Assert.Equal(phone, staff.VerifiedPhone);
                Assert.Null(staff.VerifiedEmail);
                Assert.True(staff.IsActive);
                Assert.Contains(staff.ActiveRoles, item => item.RoleId == role.Id.Value);
            }
            finally
            {
                await platformTransaction.RollbackAsync();
                await identityTransaction.RollbackAsync();
            }
        }

        await using var verifyPlatform = new PlatformDbContext(platformOptions);
        await using var verifyIdentity = new IdentityDbContext(identityOptions);
        Assert.False(await verifyPlatform.RoleAssignments.AsNoTracking()
            .AnyAsync(x => x.AccountId == IdentityAccountId.From(targetId)));
        Assert.False(await verifyIdentity.Accounts.AsNoTracking()
            .AnyAsync(x => x.Id == targetId || x.Id == unverifiedTargetId ||
                           x.Id == schoolOnlyTargetId));
    }

    private static void AddAccount(
        IdentityDbContext db,
        Guid accountId,
        string displayName,
        DateTimeOffset now) => db.Accounts.Add(new Account
        {
            Id = accountId,
            Status = AccountStatus.Active,
            DisplayName = displayName,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });

    private static void AddIdentifier(
        IdentityDbContext db,
        Guid accountId,
        LoginIdentifierType type,
        string value,
        Guid? schoolId,
        bool verified,
        DateTimeOffset now) => db.LoginIdentifiers.Add(new LoginIdentifier
        {
            Id = Guid.NewGuid(),
            AccountId = accountId,
            Type = type,
            NormalizedValue = value,
            DisplayValue = value,
            SchoolId = schoolId,
            IsVerified = verified,
            VerifiedAtUtc = verified ? now : null,
            CreatedAtUtc = now
        });

    private static async Task<Mdaresna.Platform.Application.Access.Staff.PlatformStaffDirectoryItem?>
        FindStaffAsync(PlatformStaffDirectory directory, Guid accountId)
    {
        const int pageSize = 100;
        var page = await directory.ListAsync(1, pageSize);
        var staff = page.Items.SingleOrDefault(item => item.AccountId == accountId);
        for (var pageNumber = 2; staff is null &&
                 pageNumber <= (page.TotalCount + pageSize - 1) / pageSize; pageNumber++)
        {
            page = await directory.ListAsync(pageNumber, pageSize);
            staff = page.Items.SingleOrDefault(item => item.AccountId == accountId);
        }

        return staff;
    }

    private static (string Platform, string Identity)? GetApprovedLocalConnections()
    {
        var platform = Environment.GetEnvironmentVariable(
            "MDARESNA_PLATFORM_STAFF_INTEGRATION_PLATFORM_CONNECTION");
        var identity = Environment.GetEnvironmentVariable(
            "MDARESNA_PLATFORM_STAFF_INTEGRATION_IDENTITY_CONNECTION");
        if (string.IsNullOrWhiteSpace(platform) && string.IsNullOrWhiteSpace(identity))
        {
            return null;
        }

        Assert.False(string.IsNullOrWhiteSpace(platform));
        Assert.False(string.IsNullOrWhiteSpace(identity));
        VerifyTarget(platform!, "MdaresnaPlatformLocal");
        VerifyTarget(identity!, "MdaresnaIdentityLocal");
        return (platform!, identity!);
    }

    private static void VerifyTarget(string connection, string database)
    {
        var target = new SqlConnectionStringBuilder(connection);
        Assert.Equal(@"(localdb)\MSSQLLocalDB", target.DataSource, ignoreCase: true);
        Assert.Equal(database, target.InitialCatalog, ignoreCase: true);
        Assert.True(target.IntegratedSecurity);
    }

    private sealed class AllowAllPermissions : IPlatformPermissionEvaluator
    {
        public Task<bool> HasPermissionAsync(
            IdentityAccountId accountId,
            PermissionCode permission,
            CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}
