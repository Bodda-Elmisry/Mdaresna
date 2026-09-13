using System.Security.Cryptography;
using Mdaresna.Platform.Bootstrap;
using Mdaresna.Platform.Infrastructure.Messaging;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Mdaresna.Platform.Bootstrap.UnitTests;

public sealed class SmsProviderSeedTests
{
    private const string OptInVariable = "MDARESNA_SMS_PROVIDER_SEED_TEST_LOCALDB";
    private const string DatabasePrefix = "MdaresnaSmsSeedTest_";

    [Fact]
    public void Seed_command_accepts_only_explicit_modes()
    {
        Assert.False(SmsProviderSeedOptions.Parse(["--seed-sms-provider"]).DryRun);
        Assert.True(SmsProviderSeedOptions.Parse(
            ["--seed-sms-provider", "--dry-run"]).DryRun);
        Assert.Throws<BootstrapUsageException>(() =>
            SmsProviderSeedOptions.Parse(["--seed-sms-provider", "--dry-run", "--dry-run"]));
        Assert.Throws<BootstrapUsageException>(() =>
            SmsProviderSeedOptions.Parse(["--seed-sms-provider", "--password", "x"]));
    }

    [Fact]
    public async Task Seed_is_once_only_and_never_reactivates_a_deleted_provider()
    {
        if (Environment.GetEnvironmentVariable(OptInVariable) != "1")
        {
            return;
        }

        var databaseName = DatabasePrefix + Guid.NewGuid().ToString("N");
        var dbOptions = OptionsFor(databaseName);
        var deleteDatabase = false;
        var password = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var encryptionKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PlatformSms:EncryptionKey"] = encryptionKey
            })
            .Build();
        var protector = new PlatformSmsSecretProtector(configuration);

        try
        {
            await using (var db = new PlatformDbContext(dbOptions))
            {
                Assert.False(await db.Database.CanConnectAsync());
                deleteDatabase = true;
                await db.Database.MigrateAsync();
                Assert.Contains("ready", await new SmsProviderSeedBootstrapper(db)
                    .InspectAsync(), StringComparison.OrdinalIgnoreCase);
            }

            await using (var db = new PlatformDbContext(dbOptions))
            {
                var first = await new SmsProviderSeedBootstrapper(db, protector)
                    .RunAsync(password);
                Assert.False(first.AlreadyCompleted);
                Assert.Equal(SmsProviderSeedBootstrapper.ProviderId, first.ProviderId);
            }

            string encrypted;
            await using (var db = new PlatformDbContext(dbOptions))
            {
                var provider = await db.SmsProviders.SingleAsync();
                Assert.Equal(SmsProviderSeedBootstrapper.ProviderId, provider.Id);
                Assert.Equal(SmsProviderSeedBootstrapper.ProviderUserName,
                    provider.ProviderUserName);
                Assert.Equal(SmsProviderSeedBootstrapper.SenderName, provider.SenderName);
                Assert.Equal(SmsProviderSeedBootstrapper.ApiUrlTemplate,
                    provider.ApiUrlTemplate);
                Assert.Equal(20, provider.MessageCharactersLength);
                Assert.Equal(1, provider.Priority);
                Assert.True(provider.IsActive);
                Assert.False(provider.IsDeleted);
                Assert.Equal(SmsProviderSeedBootstrapper.SeedTimestampUtc,
                    provider.CreatedAtUtc);
                Assert.Equal(SmsProviderSeedBootstrapper.SeedTimestampUtc,
                    provider.UpdatedAtUtc);
                Assert.Equal(PlatformSmsOptions.LegacyNonEmptyResponse,
                    provider.SuccessResponsePrefix);
                encrypted = provider.EncryptedPassword;
                Assert.NotEqual(password, encrypted);
                Assert.Equal(password, protector.Unprotect(encrypted));
                Assert.Single(await db.AuditEntries.ToListAsync());

                provider.IsActive = false;
                provider.IsDeleted = true;
                provider.UpdatedAtUtc = DateTimeOffset.UtcNow;
                await db.SaveChangesAsync();
            }

            await using (var db = new PlatformDbContext(dbOptions))
            {
                var rerun = await new SmsProviderSeedBootstrapper(db, protector)
                    .RunAsync(Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)));
                Assert.True(rerun.AlreadyCompleted);
                var provider = await db.SmsProviders.AsNoTracking().SingleAsync();
                Assert.False(provider.IsActive);
                Assert.True(provider.IsDeleted);
                Assert.Equal(encrypted, provider.EncryptedPassword);
                Assert.Single(await db.AuditEntries.ToListAsync());
            }
        }
        finally
        {
            if (deleteDatabase)
            {
                await using var db = new PlatformDbContext(dbOptions);
                ValidateCleanupTarget(db, databaseName);
                await db.Database.EnsureDeletedAsync();
            }
        }
    }

    private static DbContextOptions<PlatformDbContext> OptionsFor(string databaseName)
    {
        var connection = new SqlConnectionStringBuilder
        {
            DataSource = @"(localdb)\MSSQLLocalDB",
            InitialCatalog = databaseName,
            IntegratedSecurity = true,
            TrustServerCertificate = true
        }.ConnectionString;
        return new DbContextOptionsBuilder<PlatformDbContext>()
            .UseSqlServer(connection, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", "platform"))
            .Options;
    }

    private static void ValidateCleanupTarget(DbContext db, string expectedName)
    {
        var target = new SqlConnectionStringBuilder(
            db.Database.GetDbConnection().ConnectionString);
        Assert.Equal(@"(localdb)\MSSQLLocalDB", target.DataSource,
            ignoreCase: true);
        Assert.Equal(expectedName, target.InitialCatalog, ignoreCase: false);
        Assert.StartsWith(DatabasePrefix, target.InitialCatalog,
            StringComparison.Ordinal);
        Assert.True(target.IntegratedSecurity);
    }
}
