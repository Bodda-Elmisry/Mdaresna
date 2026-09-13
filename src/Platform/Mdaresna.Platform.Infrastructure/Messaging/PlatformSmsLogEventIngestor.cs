using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Contracts.Messaging;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Mdaresna.Platform.Infrastructure.Messaging;

/// <summary>
/// Transactionally stores a cross-system SMS event and its inbox receipt.
/// The caller must acknowledge the broker delivery only after this returns.
/// </summary>
public sealed class PlatformSmsLogEventIngestor(
    PlatformDbContext db,
    IConfiguration configuration)
{
    private const string ConsumerName = "platform.sms-log.v1";

    /// <returns>True when a new log was stored; false for a redelivery.</returns>
    public async Task<bool> IngestAsync(
        IntegrationMessageEnvelope<SmsDeliveryAttemptRecordedV1> envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        var source = envelope.Producer;
        if (source is not ("platform" or "schools" or "family"))
        {
            throw new ArgumentException("Unknown SMS event producer.", nameof(envelope));
        }

        var school = envelope.Scope.SchoolId;
        var tenant = envelope.Scope.TenantId;
        if (source == "schools" && school is null ||
            school is not null && (tenant is null || tenant.Value.Value != school.Value.Value))
        {
            throw new ArgumentException("SMS event school scope is invalid.", nameof(envelope));
        }

        var secrets = new PlatformSmsSecretProtector(configuration);
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        if (await db.InboxMessages.AsNoTracking().AnyAsync(
                x => x.Consumer == ConsumerName && x.MessageId == envelope.MessageId,
                cancellationToken))
        {
            await transaction.CommitAsync(cancellationToken);
            return false;
        }

        var data = envelope.Data;
        var now = DateTimeOffset.UtcNow;
        db.SmsLogs.Add(new PlatformSmsLog
        {
            Id = Guid.NewGuid(),
            SourceSystem = source,
            MessageTypeCode = data.MessageTypeCode,
            SchoolId = school?.Value,
            SourceMessageId = envelope.MessageId,
            ProviderId = null,
            RecipientEncrypted = secrets.ProtectPayload(data.Recipient),
            RecipientMasked = MaskRecipient(data.Recipient),
            MessageEncrypted = secrets.ProtectPayload(data.Message),
            ResponseEncrypted = data.ProviderResponse is null
                ? null
                : secrets.ProtectPayload(data.ProviderResponse),
            HttpStatusCode = data.HttpStatusCode,
            Status = data.Outcome,
            FailureReason = data.FailureReason,
            CreatedAtUtc = data.AttemptedAtUtc,
            CompletedAtUtc = data.CompletedAtUtc
        });
        db.InboxMessages.Add(new PlatformInboxMessage
        {
            Consumer = ConsumerName,
            MessageId = envelope.MessageId,
            MessageType = SmsDeliveryAttemptRecordedV1.MessageType,
            SchemaVersion = SmsDeliveryAttemptRecordedV1.SchemaVersion,
            ReceivedAtUtc = now,
            ProcessedAtUtc = now,
            AttemptCount = 1
        });
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return true;
    }

    private static string MaskRecipient(string recipient) =>
        recipient.Length <= 4
            ? new string('*', recipient.Length)
            : new string('*', recipient.Length - 4) + recipient[^4..];
}
