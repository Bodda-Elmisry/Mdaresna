using Mdaresna.IntegrationContracts.Messaging;

namespace Mdaresna.Messaging.Abstractions;

/// <summary>
/// Adds an integration event to the current business transaction.
/// Implementations must not save changes or contact the broker directly.
/// </summary>
public interface IOutboxWriter
{
    void Enqueue<TMessage>(IntegrationMessageEnvelope<TMessage> message)
        where TMessage : class, IIntegrationMessage;
}
