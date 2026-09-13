using System.Security.Cryptography;
using System.Net;
using System.Text.Json;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Infrastructure.Messaging;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Mdaresna.Platform.UnitTests.Messaging;

/// <summary>
/// Opt in with MDARESNA_SMS_PROVIDER_TEST_LOCALDB=1. The test uses a uniquely
/// named LocalDB database and validates its exact name before cleanup.
/// </summary>
public sealed class PlatformSmsProviderIntegrationTests
{
    private const string OptInVariable = "MDARESNA_SMS_PROVIDER_TEST_LOCALDB";
    private const string DatabasePrefix = "MdaresnaSmsProviderTest_";

    [Fact]
    public async Task Provider_lifecycle_masks_password_enforces_versions_and_soft_deletes()
    {
        if (Environment.GetEnvironmentVariable(OptInVariable) != "1")
        {
            return;
        }

        var databaseName = DatabasePrefix + Guid.NewGuid().ToString("N");
        var options = OptionsFor(databaseName);
        var key = RandomNumberGenerator.GetBytes(32);
        var secrets = CreateProtector(key);
        var createdDatabase = false;
        var actor = Guid.NewGuid();

        try
        {
            await using (var db = new PlatformDbContext(options))
            {
                Assert.False(await db.Database.CanConnectAsync());
                createdDatabase = true;
                await db.Database.MigrateAsync();
            }

            Guid providerId;
            string initialVersion;
            await using (var db = new PlatformDbContext(options))
            {
                var service = new PlatformSmsProviderService(db, secrets);
                var created = await service.CreateAsync(Values("initial-password"), actor, "create-test");
                providerId = created.Id;
                initialVersion = created.Version;
                Assert.False(created.IsActive);
                Assert.True(created.HasPassword);
                Assert.NotEmpty(created.Version);
                Assert.DoesNotContain("initial-password", created.ToString(), StringComparison.Ordinal);
                var apiJson = JsonSerializer.Serialize(ApiResponse<PlatformSmsProviderReadModel>
                    .Success(created, statusCode: 201));
                Assert.DoesNotContain("initial-password", apiJson, StringComparison.Ordinal);
                Assert.DoesNotContain("EncryptedPassword", apiJson, StringComparison.Ordinal);

                var row = await db.SmsProviders.SingleAsync();
                Assert.NotEqual("initial-password", row.EncryptedPassword);
                Assert.Equal("initial-password", secrets.Unprotect(row.EncryptedPassword));
                Assert.Equal(1, await db.AuditEntries.CountAsync());
            }

            string updatedVersion;
            await using (var db = new PlatformDbContext(options))
            {
                var service = new PlatformSmsProviderService(db, secrets);
                var updated = await service.UpdateAsync(providerId, initialVersion,
                    Values(null, "new-sender"), actor, "update-test");
                updatedVersion = updated.Version;
                Assert.NotEqual(initialVersion, updatedVersion);
                Assert.Equal("new-sender", updated.SenderName);
                Assert.Equal("initial-password", secrets.Unprotect(
                    (await db.SmsProviders.SingleAsync()).EncryptedPassword));

                await Assert.ThrowsAsync<PlatformConflictException>(() =>
                    service.SetActiveAsync(providerId, initialVersion, true, actor, null));
            }

            await using (var db = new PlatformDbContext(options))
            {
                var service = new PlatformSmsProviderService(db, secrets);
                var active = await service.SetActiveAsync(providerId, updatedVersion,
                    true, actor, "activate-test");
                Assert.True(active.IsActive);

                var page = await service.ListAsync(true, 1, 10);
                Assert.Equal(1, page.TotalCount);
                Assert.Single(page.Items);
                Assert.Equal(providerId, page.Items[0].Id);
                Assert.DoesNotContain("initial-password", page.Items[0].ToString(), StringComparison.Ordinal);

                var inactive = await service.SetActiveAsync(providerId, active.Version,
                    false, actor, "deactivate-test");
                Assert.False(inactive.IsActive);
                Assert.Equal(0, (await service.ListAsync(true, 1, 10)).TotalCount);
                var missingProviderSender = new PlatformDbSmsSender(
                    db, CreateSmsConfiguration(key));
                var inactiveError = await Assert.ThrowsAsync<PlatformSmsDeliveryException>(() =>
                    missingProviderSender.SendAsync("00967777661929", "OTP 123456"));
                Assert.Equal("No active Platform SMS provider is configured.", inactiveError.Message);

                await service.DeleteAsync(providerId, inactive.Version, actor, "delete-test");
                Assert.True((await db.SmsProviders.SingleAsync()).IsDeleted);
                Assert.Equal(0, (await service.ListAsync(null, 1, 10)).TotalCount);
                var deletedError = await Assert.ThrowsAsync<PlatformSmsDeliveryException>(() =>
                    missingProviderSender.SendAsync("00967777661929", "OTP 123456"));
                Assert.Equal("No active Platform SMS provider is configured.", deletedError.Message);
                var failedLogs = await db.SmsLogs.OrderBy(x => x.CreatedAtUtc).ToListAsync();
                Assert.Equal(2, failedLogs.Count);
                Assert.All(failedLogs, log =>
                {
                    Assert.Equal("Failed", log.Status);
                    Assert.Equal("provider_configuration", log.FailureReason);
                    Assert.Null(log.HttpStatusCode);
                    Assert.Null(log.ResponseEncrypted);
                    Assert.NotNull(log.CompletedAtUtc);
                    Assert.Equal("OTP 123456", secrets.Unprotect(log.MessageEncrypted));
                });
                await Assert.ThrowsAsync<PlatformResourceNotFoundException>(() =>
                    service.GetAsync(providerId));
                Assert.Equal(5, await db.AuditEntries.CountAsync());
            }
        }
        finally
        {
            if (createdDatabase)
            {
                await using var db = new PlatformDbContext(options);
                ValidateCleanupTarget(db, databaseName);
                await db.Database.EnsureDeletedAsync();
            }
        }
    }

    [Fact]
    public async Task Sms_sender_persists_encrypted_success_and_rejected_responses()
    {
        if (Environment.GetEnvironmentVariable(OptInVariable) != "1")
        {
            return;
        }

        var databaseName = DatabasePrefix + Guid.NewGuid().ToString("N");
        var options = OptionsFor(databaseName);
        var key = RandomNumberGenerator.GetBytes(32);
        var configuration = CreateSmsConfiguration(key);
        var secrets = new PlatformSmsSecretProtector(configuration);
        var createdDatabase = false;
        var providerId = Guid.NewGuid();
        const string phone = "00967777661929";
        const string message = "Mdaresna activation code: 87654321";
        const string successResponse = "ACCEPTED: receipt 123";
        const string rejectedResponse = "ERROR: OTP 87654321 rejected";

        try
        {
            await using (var db = new PlatformDbContext(options))
            {
                Assert.False(await db.Database.CanConnectAsync());
                createdDatabase = true;
                await db.Database.MigrateAsync();
                var now = DateTimeOffset.UtcNow;
                db.SmsProviders.Add(new PlatformSmsProvider
                {
                    Id = providerId,
                    ProviderUserName = "provider-user",
                    EncryptedPassword = secrets.Protect("provider-password"),
                    SenderName = "Mdaresna",
                    ApiUrlTemplate = "https://sms.example.test/send?user={0}&password={1}&sender={2}&to={3}&text={4}",
                    MessageCharactersLength = 160,
                    Priority = 1,
                    IsActive = true,
                    SuccessResponsePrefix = "ACCEPTED:",
                    CreatedAtUtc = now,
                    UpdatedAtUtc = now
                });
                await db.SaveChangesAsync();
            }

            await using (var db = new PlatformDbContext(options))
            {
                var handler = new ResponseHandler(HttpStatusCode.OK, successResponse);
                using var client = new HttpClient(handler);
                var sender = new PlatformDbSmsSender(db, configuration, client);
                await sender.SendAsync(phone, message, "otp", null);
                Assert.Equal(1, handler.CallCount);
            }

            await using (var db = new PlatformDbContext(options))
            {
                var accepted = await db.SmsLogs.SingleAsync();
                Assert.Equal(providerId, accepted.ProviderId);
                Assert.Equal("platform", accepted.SourceSystem);
                Assert.Equal("otp", accepted.MessageTypeCode);
                Assert.Null(accepted.SchoolId);
                Assert.Null(accepted.SourceMessageId);
                Assert.Equal("Accepted", accepted.Status);
                Assert.Equal(200, accepted.HttpStatusCode);
                Assert.Null(accepted.FailureReason);
                Assert.NotNull(accepted.CompletedAtUtc);
                Assert.EndsWith("1929", accepted.RecipientMasked);
                Assert.DoesNotContain(phone, accepted.RecipientMasked);
                Assert.NotEqual(phone, accepted.RecipientEncrypted);
                Assert.NotEqual(message, accepted.MessageEncrypted);
                Assert.NotEqual(successResponse, accepted.ResponseEncrypted);
                Assert.Equal(phone, secrets.Unprotect(accepted.RecipientEncrypted));
                Assert.Equal(message, secrets.Unprotect(accepted.MessageEncrypted));
                Assert.Equal(successResponse, secrets.Unprotect(accepted.ResponseEncrypted!));
            }

            await using (var db = new PlatformDbContext(options))
            {
                var handler = new ResponseHandler(HttpStatusCode.OK, rejectedResponse);
                using var client = new HttpClient(handler);
                var sender = new PlatformDbSmsSender(db, configuration, client);
                var error = await Assert.ThrowsAsync<PlatformSmsDeliveryException>(() =>
                    sender.SendAsync(phone, message));
                Assert.Equal(1, handler.CallCount);
                Assert.DoesNotContain("87654321", error.ToString());
                Assert.DoesNotContain(rejectedResponse, error.ToString());
            }

            await using (var db = new PlatformDbContext(options))
            {
                var logs = await db.SmsLogs.OrderBy(x => x.CreatedAtUtc).ToListAsync();
                Assert.Equal(2, logs.Count);
                var rejected = Assert.Single(logs, x => x.Status == "Rejected");
                Assert.Equal(providerId, rejected.ProviderId);
                Assert.Equal("platform", rejected.SourceSystem);
                Assert.Equal("other", rejected.MessageTypeCode);
                Assert.Equal(200, rejected.HttpStatusCode);
                Assert.Equal("provider_unconfirmed", rejected.FailureReason);
                Assert.NotNull(rejected.CompletedAtUtc);
                Assert.NotEqual(rejectedResponse, rejected.ResponseEncrypted);
                Assert.Equal(rejectedResponse, secrets.Unprotect(rejected.ResponseEncrypted!));
                Assert.Equal(message, secrets.Unprotect(rejected.MessageEncrypted));
            }
        }
        finally
        {
            if (createdDatabase)
            {
                await using var db = new PlatformDbContext(options);
                ValidateCleanupTarget(db, databaseName);
                await db.Database.EnsureDeletedAsync();
            }
        }
    }

    private static PlatformSmsProviderValues Values(string? password, string sender = "sender") => new()
    {
        ProviderUserName = "provider-user",
        ProviderPassword = password,
        SenderName = sender,
        ApiUrlTemplate = "https://sms.example.test/send?user={0}&password={1}&sender={2}&to={3}&text={4}",
        MessageCharactersLength = 160,
        Priority = 1,
        SuccessResponsePrefix = "ACCEPTED:"
    };

    private static PlatformSmsSecretProtector CreateProtector(byte[] key)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PlatformSms:EncryptionKey"] = Convert.ToBase64String(key)
            })
            .Build();
        return new PlatformSmsSecretProtector(configuration);
    }

    private static IConfiguration CreateSmsConfiguration(byte[] key) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PlatformSms:EncryptionKey"] = Convert.ToBase64String(key)
            })
            .Build();

    private sealed class ResponseHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        public int CallCount { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            CallCount++;
            Assert.Equal("sms.example.test", request.RequestUri?.Host);
            return Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body)
            });
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
            .UseSqlServer(connection, sql => sql.MigrationsHistoryTable("__EFMigrationsHistory", "platform"))
            .Options;
    }

    private static void ValidateCleanupTarget(PlatformDbContext db, string expectedName)
    {
        var target = new SqlConnectionStringBuilder(db.Database.GetDbConnection().ConnectionString);
        Assert.Equal(@"(localdb)\MSSQLLocalDB", target.DataSource, ignoreCase: true);
        Assert.Equal(expectedName, target.InitialCatalog, ignoreCase: false);
        Assert.StartsWith(DatabasePrefix, target.InitialCatalog, StringComparison.Ordinal);
        Assert.True(target.IntegratedSecurity);
    }
}
