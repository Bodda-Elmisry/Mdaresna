using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;

namespace Mdaresna.Platform.Infrastructure.Messaging;

internal sealed class PlatformOutboxWriter(PlatformDbContext dbContext) : IPlatformOutboxWriter
{
    public void Enqueue<TMessage>(IntegrationMessageEnvelope<TMessage> message)
        where TMessage : class, IIntegrationMessage
    {
        ArgumentNullException.ThrowIfNull(message);

        dbContext.OutboxMessages.Add(new PlatformOutboxMessage
        {
            Id = message.MessageId,
            MessageType = message.MessageType,
            SchemaVersion = message.SchemaVersion,
            PayloadJson = IntegrationJsonSerializer.Serialize(message),
            OccurredAtUtc = message.OccurredAtUtc,
            CorrelationId = message.CorrelationId,
            CausationId = message.CausationId,
            TraceParent = message.TraceParent
        });
    }
}
