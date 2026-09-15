using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Registry.ConsumeSchoolRegistrationRequest;
using Mdaresna.Platform.Contracts.Registry;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Mdaresna.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Messaging;

/// <summary>
/// Applies a Schools registration event and stores its inbox receipt in one
/// Platform-database transaction. RabbitMQ must acknowledge only after this
/// method returns successfully.
/// </summary>
public sealed class PlatformSchoolRegistrationRequestIngestor(
    PlatformDbContext db,
    ConsumeSchoolRegistrationRequestHandler handler,
    IClock clock)
{
    public const string ConsumerName = "platform.school-registration-request.v1";

    /// <returns>True for the first processing; false for an inbox duplicate.</returns>
    public async Task<bool> IngestAsync(
        IntegrationMessageEnvelope<SchoolRegistrationRequestedV1> envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        var processed = false;
        var strategy = db.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            // A retry must rebuild the unit of work from database state rather
            // than reuse entries tracked during the rolled-back attempt.
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database
                .BeginTransactionAsync(cancellationToken);

            if (await db.InboxMessages.AsNoTracking().AnyAsync(
                    item => item.Consumer == ConsumerName &&
                            item.MessageId == envelope.MessageId,
                    cancellationToken))
            {
                await transaction.CommitAsync(cancellationToken);
                return;
            }

            await handler.HandleAsync(envelope, cancellationToken);

            var now = clock.UtcNow;
            db.InboxMessages.Add(new PlatformInboxMessage
            {
                Consumer = ConsumerName,
                MessageId = envelope.MessageId,
                MessageType = SchoolRegistrationRequestedV1.MessageType,
                SchemaVersion = SchoolRegistrationRequestedV1.SchemaVersion,
                ReceivedAtUtc = now,
                ProcessedAtUtc = now,
                AttemptCount = 1
            });
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            processed = true;
        });

        return processed;
    }
}
