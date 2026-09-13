using System.Text.Json.Serialization;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Contracts.Common;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Contracts.Registry;

public sealed record TenantRegisteredV1 : IIntegrationEvent
{
    [JsonConstructor]
    public TenantRegisteredV1(
        TenantId tenantId,
        string displayName,
        DateTimeOffset registeredAtUtc)
    {
        if (tenantId.IsEmpty)
        {
            throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        }

        TenantId = tenantId;
        DisplayName = ContractGuard.Text(displayName, 200, nameof(displayName));
        RegisteredAtUtc = ContractGuard.Utc(registeredAtUtc, nameof(registeredAtUtc));
    }

    public static string MessageType => "mdaresna.platform.registry.tenant-registered";

    public static ushort SchemaVersion => 1;

    [JsonPropertyName("tenantId")]
    public TenantId TenantId { get; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; }

    [JsonPropertyName("registeredAtUtc")]
    public DateTimeOffset RegisteredAtUtc { get; }
}
