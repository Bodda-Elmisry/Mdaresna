using System.Security.Cryptography;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Platform.Contracts.Messaging;
using Mdaresna.Platform.Infrastructure.Messaging;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Mdaresna.Platform.UnitTests.Messaging;

public sealed class PlatformSmsLogEventIntegrationTests
{
    private const string OptInVariable = "MDARESNA_SMS_EVENT_TEST_LOCALDB";
    private const string DatabasePrefix = "MdaresnaSmsEventTest_";

    [Fact]
    public void Event_contract_round_trips_with_school_scope()
    {
        var envelope = SchoolEvent(Guid.NewGuid());

        var restored = IntegrationJsonSerializer.Deserialize<SmsDeliveryAttemptRecordedV1>(
            IntegrationJsonSerializer.Serialize(envelope));

        Assert.Equal(envelope.MessageId, restored.MessageId);
        Assert.Equal("schools", restored.Producer);
        Assert.Equal(envelope.Scope.SchoolId, restored.Scope.SchoolId);
        Assert.Equal("otp", restored.Data.MessageTypeCode);
        Assert.Equal("ACCEPTED: OTP 12345678", restored.Data.ProviderResponse);
    }

    [Fact]
    public async Task School_and_family_events_are_encrypted_scoped_and_idempotent()
    {
        if (Environment.GetEnvironmentVariable(OptInVariable) != "1") return;

        var databaseName = DatabasePrefix + Guid.NewGuid().ToString("N");
        var dbOptions = OptionsFor(databaseName);
        var key = RandomNumberGenerator.GetBytes(32);
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PlatformSms:EncryptionKey"] = Convert.ToBase64String(key)
            })
            .Build();
        var secrets = new PlatformSmsSecretProtector(configuration);
        var createdDatabase = false;

        try
        {
            await using (var db = new PlatformDbContext(dbOptions))
            {
                Assert.False(await db.Database.CanConnectAsync());
                createdDatabase = true;
                await db.Database.MigrateAsync();
            }

            var schoolId = Guid.NewGuid();
            var schoolEvent = SchoolEvent(schoolId);
            await using (var db = new PlatformDbContext(dbOptions))
            {
                var ingestor = new PlatformSmsLogEventIngestor(db, configuration);
                Assert.True(await ingestor.IngestAsync(schoolEvent));
                Assert.False(await ingestor.IngestAsync(schoolEvent));
            }

            var familyData = new SmsDeliveryAttemptRecordedV1(
                "notification", "00967777661929", "Family notification", null,
                null, "Failed", "provider_unavailable",
                DateTimeOffset.UtcNow.AddSeconds(-1), DateTimeOffset.UtcNow);
            var familyEvent = IntegrationMessageEnvelope<SmsDeliveryAttemptRecordedV1>.Create(
                DateTimeOffset.UtcNow, "family", IntegrationMessageScope.Global,
                familyData);
            await using (var db = new PlatformDbContext(dbOptions))
            {
                var ingestor = new PlatformSmsLogEventIngestor(db, configuration);
                Assert.True(await ingestor.IngestAsync(familyEvent));
            }

            await using (var db = new PlatformDbContext(dbOptions))
            {
                var logs = await db.SmsLogs.AsNoTracking()
                    .OrderBy(x => x.SourceSystem).ToListAsync();
                Assert.Equal(2, logs.Count);
                var school = Assert.Single(logs, x => x.SourceSystem == "schools");
                Assert.Equal(schoolId, school.SchoolId);
                Assert.Equal("otp", school.MessageTypeCode);
                Assert.Equal(schoolEvent.MessageId, school.SourceMessageId);
                Assert.Equal("Accepted", school.Status);
                Assert.Equal(200, school.HttpStatusCode);
                Assert.NotEqual(schoolEvent.Data.Message, school.MessageEncrypted);
                Assert.Equal(schoolEvent.Data.Recipient,
                    secrets.Unprotect(school.RecipientEncrypted));
                Assert.Equal(schoolEvent.Data.Message,
                    secrets.Unprotect(school.MessageEncrypted));
                Assert.Equal(schoolEvent.Data.ProviderResponse,
                    secrets.Unprotect(school.ResponseEncrypted!));

                var family = Assert.Single(logs, x => x.SourceSystem == "family");
                Assert.Null(family.SchoolId);
                Assert.Equal("notification", family.MessageTypeCode);
                Assert.Equal("Failed", family.Status);
                Assert.Null(family.ResponseEncrypted);
                Assert.Equal(2, await db.InboxMessages.CountAsync(
                    x => x.Consumer == "platform.sms-log.v1"));
            }
        }
        finally
        {
            if (createdDatabase)
            {
                await using var db = new PlatformDbContext(dbOptions);
                ValidateCleanupTarget(db, databaseName);
                await db.Database.EnsureDeletedAsync();
            }
        }
    }

    private static IntegrationMessageEnvelope<SmsDeliveryAttemptRecordedV1> SchoolEvent(Guid schoolId)
    {
        var now = DateTimeOffset.UtcNow;
        var data = new SmsDeliveryAttemptRecordedV1(
            "otp", "00967777661929", "Your OTP is 12345678",
            "ACCEPTED: OTP 12345678", 200, "Accepted", null,
            now.AddSeconds(-1), now);
        return IntegrationMessageEnvelope<SmsDeliveryAttemptRecordedV1>.Create(
            now, "schools", IntegrationMessageScope.ForSchool(
                TenantId.From(schoolId), SchoolId.From(schoolId)), data);
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

    private static void ValidateCleanupTarget(PlatformDbContext db, string expectedName)
    {
        var target = new SqlConnectionStringBuilder(db.Database.GetDbConnection().ConnectionString);
        Assert.Equal(@"(localdb)\MSSQLLocalDB", target.DataSource, ignoreCase: true);
        Assert.Equal(expectedName, target.InitialCatalog, ignoreCase: false);
        Assert.StartsWith(DatabasePrefix, target.InitialCatalog, StringComparison.Ordinal);
        Assert.True(target.IntegratedSecurity);
    }
}
