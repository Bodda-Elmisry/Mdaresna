using System.Data;
using Mdaresna.Platform.Infrastructure.Messaging;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mdaresna.Platform.Bootstrap;

/// <summary>
/// Provisions the initial Platform SMS provider once per environment. Later
/// operator edits, deactivation and soft deletion are never reversed by a rerun.
/// </summary>
internal sealed class SmsProviderSeedBootstrapper(
    PlatformDbContext db,
    PlatformSmsSecretProtector? secrets = null)
{
    internal static readonly Guid ProviderId =
        Guid.Parse("6cbab80e-c4b8-4ac4-a546-4eb45da0d984");

    internal const string ProviderUserName = "Mdaresna";
    internal const string SenderName = "Mdaresna";
    internal const string ApiUrlTemplate =
        "https://plat.alawaelsmart.com/MainServlet?orgName={2}&userName={0}&password={1}&mobileNo={3}&text={4}&coding=2";
    internal const int MessageCharactersLength = 20;
    internal const int Priority = 1;
    internal static readonly DateTimeOffset SeedTimestampUtc =
        new(2026, 2, 8, 21, 5, 50, TimeSpan.Zero);

    private const string LockResource = "mdaresna-platform-initial-sms-provider-seed";

    public async Task<SmsProviderSeedResult> RunAsync(
        string password,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(secrets);
        if (string.IsNullOrWhiteSpace(password) || password.Length > 300)
        {
            throw new BootstrapRejectedException("SMS provider seed credential is missing or invalid.");
        }

        await EnsureDatabaseReadyAsync(cancellationToken);
        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await AcquireLockAsync(transaction, cancellationToken);

        if (await db.SmsProviders.AsNoTracking()
                .AnyAsync(x => x.Id == ProviderId, cancellationToken))
        {
            await transaction.CommitAsync(cancellationToken);
            return new SmsProviderSeedResult(ProviderId, AlreadyCompleted: true);
        }

        if (await db.SmsProviders.AsNoTracking().AnyAsync(cancellationToken))
        {
            throw new BootstrapRejectedException(
                "Another Platform SMS provider already exists. Manual review is required.");
        }

        db.SmsProviders.Add(new PlatformSmsProvider
        {
            Id = ProviderId,
            ProviderUserName = ProviderUserName,
            EncryptedPassword = secrets.Protect(password),
            SenderName = SenderName,
            ApiUrlTemplate = ApiUrlTemplate,
            MessageCharactersLength = MessageCharactersLength,
            Priority = Priority,
            IsActive = true,
            IsDeleted = false,
            SuccessResponsePrefix = PlatformSmsOptions.LegacyNonEmptyResponse,
            CreatedAtUtc = SeedTimestampUtc,
            UpdatedAtUtc = SeedTimestampUtc
        });
        db.AuditEntries.Add(new PlatformAuditEntry
        {
            Id = Guid.NewGuid(),
            Action = "platform.sms_provider.seeded",
            ResourceType = "platform-sms-provider",
            ResourceId = ProviderId.ToString("D"),
            OccurredAtUtc = DateTimeOffset.UtcNow,
            CorrelationId = ProviderId.ToString("D")
        });

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new SmsProviderSeedResult(ProviderId, AlreadyCompleted: false);
    }

    public async Task<string> InspectAsync(CancellationToken cancellationToken = default)
    {
        await EnsureDatabaseReadyAsync(cancellationToken);
        if (await db.SmsProviders.AsNoTracking()
                .AnyAsync(x => x.Id == ProviderId, cancellationToken))
        {
            return "The initial Platform SMS provider already exists; a rerun will leave it unchanged.";
        }

        return await db.SmsProviders.AsNoTracking().AnyAsync(cancellationToken)
            ? "Other Platform SMS providers exist; seeding would require manual review."
            : "The Platform database is ready for initial SMS provider provisioning.";
    }

    private async Task EnsureDatabaseReadyAsync(CancellationToken cancellationToken)
    {
        if (!await db.Database.CanConnectAsync(cancellationToken) ||
            (await db.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
        {
            throw new BootstrapRejectedException(
                "The Platform database must be reachable with all reviewed migrations applied.");
        }
    }

    private async Task AcquireLockAsync(
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        var connection = (SqlConnection)db.Database.GetDbConnection();
        await using var command = connection.CreateCommand();
        command.Transaction = (SqlTransaction)transaction.GetDbTransaction();
        command.CommandText = "DECLARE @result int; " +
                              "EXEC @result = sys.sp_getapplock " +
                              "@Resource = @resource, @LockMode = 'Exclusive', " +
                              "@LockOwner = 'Transaction', @LockTimeout = 0; " +
                              "SELECT @result;";
        command.Parameters.Add(new SqlParameter("@resource", SqlDbType.NVarChar, 255)
        {
            Value = LockResource
        });
        var result = (int)(await command.ExecuteScalarAsync(cancellationToken) ?? -999);
        if (result < 0)
        {
            throw new BootstrapRejectedException(
                "Another SMS provider seed is running or the database lock was refused.");
        }
    }
}

internal sealed record SmsProviderSeedResult(Guid ProviderId, bool AlreadyCompleted);
