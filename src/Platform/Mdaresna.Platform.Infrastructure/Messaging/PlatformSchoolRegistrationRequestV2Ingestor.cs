using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Registry.ConsumeSchoolRegistrationRequest;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Mdaresna.Schools.Contracts.Registration;
using Mdaresna.SharedKernel.Time;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Messaging;

public sealed class PlatformSchoolRegistrationRequestV2Ingestor(
    PlatformDbContext db,
    ConsumeSchoolRegistrationRequestV2Handler handler,
    IClock clock)
{
    public const string ConsumerName = "platform.school-registration-request.v2";

    public async Task<bool> IngestAsync(
        IntegrationMessageEnvelope<SchoolRegistrationRequestedV2> envelope,
        CancellationToken cancellationToken = default)
    {
        var processed = false;
        var strategy = db.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            db.ChangeTracker.Clear();
            await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
            if (await db.InboxMessages.AsNoTracking().AnyAsync(
                    x => x.Consumer == ConsumerName && x.MessageId == envelope.MessageId,
                    cancellationToken))
            {
                await transaction.CommitAsync(cancellationToken);
                return;
            }
            await handler.HandleAsync(envelope, cancellationToken);
            var now = clock.UtcNow;
            db.InboxMessages.Add(new PlatformInboxMessage
            {
                Consumer = ConsumerName, MessageId = envelope.MessageId,
                MessageType = SchoolRegistrationRequestedV2.MessageType,
                SchemaVersion = SchoolRegistrationRequestedV2.SchemaVersion,
                ReceivedAtUtc = now, ProcessedAtUtc = now, AttemptCount = 1
            });
            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            processed = true;
        });
        return processed;
    }
}
