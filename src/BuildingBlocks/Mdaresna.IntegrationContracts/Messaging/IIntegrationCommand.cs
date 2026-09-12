namespace Mdaresna.IntegrationContracts.Messaging;

/// <summary>
/// Marker interface for an immutable, versioned request directed to one owning system.
/// </summary>
public interface IIntegrationCommand : IIntegrationMessage
{
}
