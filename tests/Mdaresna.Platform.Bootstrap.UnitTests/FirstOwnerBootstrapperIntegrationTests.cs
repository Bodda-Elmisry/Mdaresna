using Mdaresna.Platform.Bootstrap;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Mdaresna.Platform.Bootstrap.UnitTests;

/// <summary>
/// Opt in with MDARESNA_FIRST_OWNER_BOOTSTRAP_TEST_LOCALDB=1. The test uses two
/// GUID-named databases and only deletes those exact test databases afterward.
/// </summary>
public sealed class FirstOwnerBootstrapperIntegrationTests
{
    private const string OptInVariable = "MDARESNA_FIRST_OWNER_BOOTSTRAP_TEST_LOCALDB";
    private const string DatabasePrefix = "MdaresnaBootstrapTest_";

    [Fact]
    public async Task First_owner_is_provisioned_once_and_rerun_never_reactivates_disabled_owner()
    {
        if (Environment.GetEnvironmentVariable(OptInVariable) != "1")
        {
            return;
        }

        var nonce = Guid.NewGuid().ToString("N");
        var identityName = $"{DatabasePrefix}{nonce}_Identity";
        var platformName = $"{DatabasePrefix}{nonce}_Platform";
        var identityOptions = OptionsFor<IdentityDbContext>(identityName, "identity");
        var platformOptions = OptionsFor<PlatformDbContext>(platformName, "platform");
        var deleteIdentity = false;
        var deletePlatform = false;

        try
        {
            await using (var identityDb = new IdentityDbContext(identityOptions))
            await using (var platformDb = new PlatformDbContext(platformOptions))
            {
                Assert.False(await identityDb.Database.CanConnectAsync());
                Assert.False(await platformDb.Database.CanConnectAsync());
                deleteIdentity = true;
                await identityDb.Database.MigrateAsync();
                deletePlatform = true;
                await platformDb.Database.MigrateAsync();
            }

            var options = new BootstrapOptions("App Manager", Guid.NewGuid(), DryRun: false);
            BootstrapResult first;
            await using (var identityDb = new IdentityDbContext(identityOptions))
            await using (var platformDb = new PlatformDbContext(platformOptions))
            {
                first = await new FirstOwnerBootstrapper(identityDb, platformDb).RunAsync(options);
            }

            Assert.False(first.AlreadyCompleted);
            await AssertProvisionedAsync(identityOptions, platformOptions, first.AccountId,
                AccountStatus.PendingVerification, isVerified: false);

            await using (var identityDb = new IdentityDbContext(identityOptions))
            await using (var platformDb = new PlatformDbContext(platformOptions))
            {
                var rerun = await new FirstOwnerBootstrapper(identityDb, platformDb).RunAsync(options);
                Assert.True(rerun.AlreadyCompleted);
                Assert.Equal(first.AccountId, rerun.AccountId);
            }

            await using (var identityDb = new IdentityDbContext(identityOptions))
            {
                var account = await identityDb.Accounts.SingleAsync(x => x.Id == first.AccountId);
                account.Status = AccountStatus.Disabled;
                account.UpdatedAtUtc = DateTimeOffset.UtcNow;
                await identityDb.SaveChangesAsync();
            }

            await using (var identityDb = new IdentityDbContext(identityOptions))
            await using (var platformDb = new PlatformDbContext(platformOptions))
            {
                var rerun = await new FirstOwnerBootstrapper(identityDb, platformDb).RunAsync(options);
                Assert.True(rerun.AlreadyCompleted);
                Assert.Equal(first.AccountId, rerun.AccountId);

                var differentOperation = options with { OperationId = Guid.NewGuid() };
                await Assert.ThrowsAsync<BootstrapRejectedException>(() =>
                    new FirstOwnerBootstrapper(identityDb, platformDb).RunAsync(differentOperation));
            }

            await AssertProvisionedAsync(identityOptions, platformOptions, first.AccountId,
                AccountStatus.Disabled, isVerified: false);
        }
        finally
        {
            if (deletePlatform)
            {
                await using var db = new PlatformDbContext(platformOptions);
                ValidateCleanupTarget(db, platformName);
                await db.Database.EnsureDeletedAsync();
            }

            if (deleteIdentity)
            {
                await using var db = new IdentityDbContext(identityOptions);
                ValidateCleanupTarget(db, identityName);
                await db.Database.EnsureDeletedAsync();
            }
        }
    }

    private static async Task AssertProvisionedAsync(
        DbContextOptions<IdentityDbContext> identityOptions,
        DbContextOptions<PlatformDbContext> platformOptions,
        Guid accountId,
        AccountStatus expectedStatus,
        bool isVerified)
    {
        await using var identityDb = new IdentityDbContext(identityOptions);
        await using var platformDb = new PlatformDbContext(platformOptions);
        var account = await identityDb.Accounts.AsNoTracking()
            .Include(x => x.LoginIdentifiers)
            .Include(x => x.PasswordCredential)
            .SingleAsync();
        Assert.Equal(accountId, account.Id);
        Assert.Equal(expectedStatus, account.Status);
        Assert.Null(account.PasswordCredential);
        var phone = Assert.Single(account.LoginIdentifiers);
        Assert.Equal(BootstrapOptions.FirstOwnerPhone, phone.NormalizedValue);
        Assert.Equal(isVerified, phone.IsVerified);
        Assert.Null(phone.VerifiedAtUtc);

        var role = await platformDb.Roles.AsNoTracking().SingleAsync();
        Assert.Equal("app-manager", role.Key);
        Assert.True(role.IsSystem);
        var assignment = await platformDb.RoleAssignments.AsNoTracking().SingleAsync();
        Assert.Equal(IdentityAccountId.From(accountId), assignment.AccountId);
        Assert.Equal(role.Id, assignment.RoleId);
        Assert.Null(assignment.RevokedAtUtc);
        Assert.Single(await platformDb.AuditEntries.AsNoTracking().ToListAsync());
    }

    private static DbContextOptions<T> OptionsFor<T>(string databaseName, string schema)
        where T : DbContext
    {
        var connection = new SqlConnectionStringBuilder
        {
            DataSource = @"(localdb)\MSSQLLocalDB",
            InitialCatalog = databaseName,
            IntegratedSecurity = true,
            TrustServerCertificate = true
        }.ConnectionString;
        return new DbContextOptionsBuilder<T>()
            .UseSqlServer(connection, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", schema))
            .Options;
    }

    private static void ValidateCleanupTarget(DbContext db, string expectedName)
    {
        var target = new SqlConnectionStringBuilder(db.Database.GetDbConnection().ConnectionString);
        Assert.Equal(@"(localdb)\MSSQLLocalDB", target.DataSource, ignoreCase: true);
        Assert.Equal(expectedName, target.InitialCatalog, ignoreCase: false);
        Assert.StartsWith(DatabasePrefix, target.InitialCatalog, StringComparison.Ordinal);
        Assert.True(target.IntegratedSecurity);
    }
}
