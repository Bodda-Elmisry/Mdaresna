using Mdaresna.Messaging.Abstractions;

namespace Mdaresna.Platform.Application.Abstractions.Messaging;

/// <summary>
/// Writes integration messages to the outbox owned by PlatformDb.
/// Implementations must participate in the current Platform unit of work.
/// </summary>
public interface IPlatformOutboxWriter : IOutboxWriter
{
}
