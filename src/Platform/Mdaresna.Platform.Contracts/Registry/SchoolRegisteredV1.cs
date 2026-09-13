using System.Text.Json.Serialization;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Contracts.Common;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Contracts.Registry;

public sealed record SchoolRegisteredV1 : IIntegrationEvent
{
    [JsonConstructor]
    public SchoolRegisteredV1(
        Guid registrationRequestId,
        TenantId tenantId,
        SchoolId schoolId,
        string schoolCode,
        string displayName,
        SchoolTypeV1 schoolType,
        SchoolDeploymentModeV1 deploymentMode,
        DateTimeOffset registeredAtUtc)
    {
        RegistrationRequestId = ContractGuard.NonEmpty(
            registrationRequestId,
            nameof(registrationRequestId));

        if (tenantId.IsEmpty)
        {
            throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        }

        if (schoolId.IsEmpty)
        {
            throw new ArgumentException("SchoolId cannot be empty.", nameof(schoolId));
        }

        TenantId = tenantId;
        SchoolId = schoolId;
        SchoolCode = ContractGuard.Text(schoolCode, 32, nameof(schoolCode));
        DisplayName = ContractGuard.Text(displayName, 200, nameof(displayName));
        SchoolType = ContractGuard.Defined(schoolType, nameof(schoolType));
        DeploymentMode = ContractGuard.Defined(deploymentMode, nameof(deploymentMode));
        RegisteredAtUtc = ContractGuard.Utc(registeredAtUtc, nameof(registeredAtUtc));
    }

    public static string MessageType => "mdaresna.platform.registry.school-registered";

    public static ushort SchemaVersion => 1;

    [JsonPropertyName("registrationRequestId")]
    public Guid RegistrationRequestId { get; }

    [JsonPropertyName("tenantId")]
    public TenantId TenantId { get; }

    [JsonPropertyName("schoolId")]
    public SchoolId SchoolId { get; }

    [JsonPropertyName("schoolCode")]
    public string SchoolCode { get; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; }

    [JsonPropertyName("schoolType")]
    public SchoolTypeV1 SchoolType { get; }

    [JsonPropertyName("deploymentMode")]
    public SchoolDeploymentModeV1 DeploymentMode { get; }

    [JsonPropertyName("registeredAtUtc")]
    public DateTimeOffset RegisteredAtUtc { get; }
}
