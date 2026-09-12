namespace Mdaresna.IntegrationContracts.Messaging;

/// <summary>
/// Defines the stable wire name and version of a cross-system message.
/// </summary>
public interface IIntegrationMessage
{
    static abstract string MessageType { get; }

    static abstract ushort SchemaVersion { get; }
}
