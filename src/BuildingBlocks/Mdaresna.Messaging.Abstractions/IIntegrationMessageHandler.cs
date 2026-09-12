using Mdaresna.IntegrationContracts.Messaging;

namespace Mdaresna.Messaging.Abstractions;

public interface IIntegrationMessageHandler<TMessage>
    where TMessage : class, IIntegrationMessage
{
    Task HandleAsync(
        IntegrationMessageEnvelope<TMessage> message,
        CancellationToken cancellationToken);
}
