using System.Text.Json.Serialization;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Contracts.Common;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Contracts.Registry;

public sealed record ProvisionSchoolV1 : IIntegrationCommand
{
    [JsonConstructor]
    public ProvisionSchoolV1(
        Guid operationId,
        TenantId tenantId,
        SchoolId schoolId,
        string schoolCode,
        string displayName,
        SchoolTypeV1 schoolType,
        SchoolDeploymentModeV1 deploymentMode,
        DateTimeOffset requestedAtUtc)
    {
        OperationId = ContractGuard.NonEmpty(operationId, nameof(operationId));

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
        RequestedAtUtc = ContractGuard.Utc(requestedAtUtc, nameof(requestedAtUtc));
    }

    public static string MessageType => "mdaresna.platform.registry.provision-school";

    public static ushort SchemaVersion => 1;

    [JsonPropertyName("operationId")]
    public Guid OperationId { get; }

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

    [JsonPropertyName("requestedAtUtc")]
    public DateTimeOffset RequestedAtUtc { get; }
}
