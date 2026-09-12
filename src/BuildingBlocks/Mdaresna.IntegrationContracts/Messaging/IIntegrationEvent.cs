namespace Mdaresna.IntegrationContracts.Messaging;

/// <summary>
/// Marker interface for an immutable, versioned event that may cross system boundaries.
/// </summary>
public interface IIntegrationEvent : IIntegrationMessage
{
}
