using Mdaresna.IntegrationContracts.Messaging;

namespace Mdaresna.Schools.Contracts.Provisioning;

public sealed record SchoolRoutingData(Guid TenantId, Guid SchoolId, string SchoolCode, string Status,
    string Provider, string Host, int Port, string DatabaseName, string CredentialSecretReference, bool RequireTls,
    long RoutingVersion);

public sealed record SchoolRoutingChangedV1(Guid TenantId, Guid SchoolId, string SchoolCode, long RoutingVersion,
    SchoolRoutingData? Target) : IIntegrationEvent
{
    public static string MessageType => "mdaresna.schools.routing.changed";
    public static ushort SchemaVersion => 1;
}
