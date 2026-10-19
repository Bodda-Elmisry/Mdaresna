using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Tenancy.Abstractions.Identifiers;
namespace Mdaresna.Schools.Contracts.Provisioning;

/// <summary>Operational provisioning is independent of commercial subscriptions.</summary>
public sealed record ProvisionSchoolV2(Guid OperationId, TenantId TenantId, SchoolId SchoolId, Guid RegistrationRequestId,
    string SchoolCode, string DisplayName, string SchoolType, string DeploymentMode, string? Address, string? PrimaryPhone,
    Guid OwnerPlatformAccountId, DateTimeOffset PlatformCreatedAtUtc, DateTimeOffset ApprovedAtUtc) : IIntegrationCommand
{
    public static string MessageType => "mdaresna.schools.provision.v2";
    public static ushort SchemaVersion => 2;
}
