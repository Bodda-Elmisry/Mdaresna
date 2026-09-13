using System.Text.Json.Serialization;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Contracts.Common;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Contracts.Registry;

public sealed record SchoolLifecycleChangedV1 : IIntegrationEvent
{
    [JsonConstructor]
    public SchoolLifecycleChangedV1(
        TenantId tenantId,
        SchoolId schoolId,
        SchoolLifecycleStatusV1 previousStatus,
        SchoolLifecycleStatusV1 currentStatus,
        Guid? provisioningOperationId,
        string? reason,
        DateTimeOffset changedAtUtc)
    {
        if (tenantId.IsEmpty)
        {
            throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        }

        if (schoolId.IsEmpty)
        {
            throw new ArgumentException("SchoolId cannot be empty.", nameof(schoolId));
        }

        if (provisioningOperationId == Guid.Empty)
        {
            throw new ArgumentException(
                "ProvisioningOperationId cannot be empty when supplied.",
                nameof(provisioningOperationId));
        }

        TenantId = tenantId;
        SchoolId = schoolId;
        PreviousStatus = ContractGuard.Defined(previousStatus, nameof(previousStatus));
        CurrentStatus = ContractGuard.Defined(currentStatus, nameof(currentStatus));

        if (PreviousStatus == CurrentStatus)
        {
            throw new ArgumentException("Lifecycle statuses must be different.", nameof(currentStatus));
        }

        ProvisioningOperationId = provisioningOperationId;
        Reason = ContractGuard.OptionalText(reason, 1000, nameof(reason));
        ChangedAtUtc = ContractGuard.Utc(changedAtUtc, nameof(changedAtUtc));
    }

    public static string MessageType => "mdaresna.platform.registry.school-lifecycle-changed";

    public static ushort SchemaVersion => 1;

    [JsonPropertyName("tenantId")]
    public TenantId TenantId { get; }

    [JsonPropertyName("schoolId")]
    public SchoolId SchoolId { get; }

    [JsonPropertyName("previousStatus")]
    public SchoolLifecycleStatusV1 PreviousStatus { get; }

    [JsonPropertyName("currentStatus")]
    public SchoolLifecycleStatusV1 CurrentStatus { get; }

    [JsonPropertyName("provisioningOperationId")]
    public Guid? ProvisioningOperationId { get; }

    [JsonPropertyName("reason")]
    public string? Reason { get; }

    [JsonPropertyName("changedAtUtc")]
    public DateTimeOffset ChangedAtUtc { get; }
}
