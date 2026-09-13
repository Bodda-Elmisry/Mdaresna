using System.Security.Cryptography;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.IdentityAuth;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mdaresna.Platform.UnitTests.Auth;

/// <summary>
/// Opt in with MDARESNA_FIRST_OWNER_ACTIVATION_TEST_LOCALDB=1. This test creates
/// two uniquely named LocalDB databases, migrates them, and deletes only those
/// exact databases. It never sends a real SMS or touches development databases.
/// </summary>
public sealed class PlatformFirstOwnerActivationIntegrationTests
{
    private const string OptInVariable = "MDARESNA_FIRST_OWNER_ACTIVATION_TEST_LOCALDB";
    private const string TestPhone = "00967777661929";
    private const string Password = "Local-test-password-147852369";
    private const string DatabasePrefix = "MdaresnaActivationTest_";

    [Fact]
    public async Task Owner_remains_pending_until_valid_code_and_password_are_committed()
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

            var accountId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            await using (var identityDb = new IdentityDbContext(identityOptions))
            await using (var platformDb = new PlatformDbContext(platformOptions))
            {
                identityDb.Accounts.Add(new Account
                {
                    Id = accountId,
                    Status = AccountStatus.PendingVerification,
                    DisplayName = "App Manager",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now,
                    LoginIdentifiers =
                    [
                        new LoginIdentifier
                        {
                            Id = Guid.NewGuid(),
                            AccountId = accountId,
                            Type = LoginIdentifierType.Phone,
                            NormalizedValue = TestPhone,
                            DisplayValue = TestPhone,
                            SchoolId = null,
                            IsVerified = false,
                            CreatedAtUtc = now
                        }
                    ]
                });
                await identityDb.SaveChangesAsync();

                var ownerId = IdentityAccountId.From(accountId);
                var role = PlatformRole.Create(
                    PlatformRoleId.New(), "app-manager", "App Manager", true,
                    [PlatformPermissionCodes.AccessManage], ownerId, now);
                platformDb.Roles.Add(role);
                platformDb.RolePermissions.Add(new PlatformRolePermissionRecord
                {
                    RoleId = role.Id,
                    PermissionCode = PlatformPermissionCodes.AccessManage
                });
                platformDb.RoleAssignments.Add(PlatformRoleAssignment.Assign(
                    PlatformRoleAssignmentId.New(), ownerId, role.Id, ownerId, now));
                platformDb.AuditEntries.Add(new PlatformAuditEntry
                {
                    Id = Guid.NewGuid(),
                    AccountId = ownerId,
                    Action = "platform.bootstrap.first_owner.provisioned",
                    ResourceType = "platform-first-owner",
                    ResourceId = accountId.ToString("D"),
                    OccurredAtUtc = now
                });
                await platformDb.SaveChangesAsync();
            }

            var sms = new FakeSmsSender();
            var activationOptions = new PlatformActivationOptions(RandomNumberGenerator.GetBytes(32));
            Assert.False(await CanLoginAsync(identityOptions, platformOptions));

            await StartAsync(identityOptions, platformOptions, sms, activationOptions);
            Assert.Single(sms.Messages);
            var firstCode = ExtractCode(sms.Messages[0]);
            await AssertPendingWithoutPasswordAsync(identityOptions, accountId);

            Assert.False(await CompleteAsync(identityOptions, platformOptions, sms,
                activationOptions, "00000000" == firstCode ? "11111111" : "00000000"));
            await AssertPendingWithoutPasswordAsync(identityOptions, accountId);

            // Expire the first challenge without waiting for wall-clock time.
            await using (var identityDb = new IdentityDbContext(identityOptions))
            {
                var challenge = await identityDb.ActivationChallenges.SingleAsync();
                challenge.CreatedAtUtc = DateTimeOffset.UtcNow.AddMinutes(-20);
                challenge.ExpiresAtUtc = DateTimeOffset.UtcNow.AddMinutes(-10);
                challenge.LastSentAtUtc = DateTimeOffset.UtcNow.AddMinutes(-20);
                await identityDb.SaveChangesAsync();
            }

            Assert.False(await CompleteAsync(identityOptions, platformOptions, sms,
                activationOptions, firstCode));
            await AssertPendingWithoutPasswordAsync(identityOptions, accountId);

            await StartAsync(identityOptions, platformOptions, sms, activationOptions);
            Assert.Equal(2, sms.Messages.Count);
            var secondCode = ExtractCode(sms.Messages[1]);
            Assert.True(await CompleteAsync(identityOptions, platformOptions, sms,
                activationOptions, secondCode));

            await using (var identityDb = new IdentityDbContext(identityOptions))
            {
                var account = await identityDb.Accounts.Include(x => x.PasswordCredential)
                    .Include(x => x.LoginIdentifiers).SingleAsync(x => x.Id == accountId);
                Assert.Equal(AccountStatus.Active, account.Status);
                Assert.NotNull(account.PasswordCredential);
                Assert.True(account.LoginIdentifiers.Single().IsVerified);
                Assert.NotEqual(Password, account.PasswordCredential.PasswordHash);
                Assert.NotNull((await identityDb.ActivationChallenges.SingleAsync()).ConsumedAtUtc);
            }

            Assert.False(await CompleteAsync(identityOptions, platformOptions, sms,
                activationOptions, secondCode));
            Assert.True(await CanLoginAsync(identityOptions, platformOptions));
        }
        finally
        {
            if (deletePlatform)
            {
                await using var platformDb = new PlatformDbContext(platformOptions);
                ValidateCleanupTarget(platformDb, platformName);
                await platformDb.Database.EnsureDeletedAsync();
            }

            if (deleteIdentity)
            {
                await using var identityDb = new IdentityDbContext(identityOptions);
                ValidateCleanupTarget(identityDb, identityName);
                await identityDb.Database.EnsureDeletedAsync();
            }
        }
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

    private static async Task StartAsync(
        DbContextOptions<IdentityDbContext> identityOptions,
        DbContextOptions<PlatformDbContext> platformOptions,
        FakeSmsSender sms,
        PlatformActivationOptions options)
    {
        await using var identityDb = new IdentityDbContext(identityOptions);
        await using var platformDb = new PlatformDbContext(platformOptions);
        await Service(identityDb, platformDb, sms, options).StartAsync(TestPhone);
    }

    private static async Task<bool> CompleteAsync(
        DbContextOptions<IdentityDbContext> identityOptions,
        DbContextOptions<PlatformDbContext> platformOptions,
        FakeSmsSender sms,
        PlatformActivationOptions options,
        string code)
    {
        await using var identityDb = new IdentityDbContext(identityOptions);
        await using var platformDb = new PlatformDbContext(platformOptions);
        return await Service(identityDb, platformDb, sms, options)
            .CompleteAsync(TestPhone, code, Password);
    }

    private static PlatformFirstOwnerActivationService Service(
        IdentityDbContext identityDb,
        PlatformDbContext platformDb,
        FakeSmsSender sms,
        PlatformActivationOptions options) =>
        new(identityDb, platformDb, sms,
            new PlatformPasswordCredentialFactory(new PasswordHasher<Account>()),
            new PasswordHasher<Account>(),
            options, NullLogger<PlatformFirstOwnerActivationService>.Instance);

    private static async Task AssertPendingWithoutPasswordAsync(
        DbContextOptions<IdentityDbContext> options, Guid accountId)
    {
        await using var db = new IdentityDbContext(options);
        var account = await db.Accounts.Include(x => x.PasswordCredential)
            .Include(x => x.LoginIdentifiers).SingleAsync(x => x.Id == accountId);
        Assert.Equal(AccountStatus.PendingVerification, account.Status);
        Assert.Null(account.PasswordCredential);
        Assert.False(account.LoginIdentifiers.Single().IsVerified);
    }

    private static async Task<bool> CanLoginAsync(
        DbContextOptions<IdentityDbContext> identityOptions,
        DbContextOptions<PlatformDbContext> platformOptions)
    {
        await using var identityDb = new IdentityDbContext(identityOptions);
        await using var platformDb = new PlatformDbContext(platformOptions);
        return await new PlatformLoginService(identityDb, platformDb, new PasswordHasher<Account>())
            .LoginAsync(TestPhone, Password) is not null;
    }

    private static string ExtractCode(string message)
    {
        var match = System.Text.RegularExpressions.Regex.Match(message, @"\b[0-9]{8}\b");
        Assert.True(match.Success, "The fake SMS did not contain an eight-digit code.");
        return match.Value;
    }

    private sealed class FakeSmsSender : IPlatformSmsSender
    {
        public List<string> Messages { get; } = [];

        public Task SendAsync(string phoneNumber, string message, CancellationToken cancellationToken = default)
        {
            Assert.Equal(TestPhone, phoneNumber);
            Messages.Add(message);
            return Task.CompletedTask;
        }
    }
}
