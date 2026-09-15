using System.Text.Json.Serialization;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Contracts.Common;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Contracts.Registry;

/// <summary>
/// Fact emitted by the Schools application after a prospective owner submits
/// a school registration request. The Platform is the sole consumer that turns
/// this request into control-plane tenant and school records.
/// </summary>
public sealed record SchoolRegistrationRequestedV1 : IIntegrationEvent
{
    [JsonConstructor]
    public SchoolRegistrationRequestedV1(
        Guid registrationRequestId,
        TenantId tenantId,
        Guid requestedByAccountId,
        string schoolCode,
        string displayName,
        string? legalName,
        SchoolTypeV1 schoolType,
        SchoolDeploymentModeV1 deploymentMode,
        string? address,
        Guid? unitTypeId,
        DateTimeOffset requestedAtUtc)
    {
        RegistrationRequestId = ContractGuard.NonEmpty(
            registrationRequestId, nameof(registrationRequestId));
        if (tenantId.IsEmpty)
        {
            throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        }

        TenantId = tenantId;
        RequestedByAccountId = ContractGuard.NonEmpty(
            requestedByAccountId, nameof(requestedByAccountId));
        SchoolCode = ContractGuard.Text(schoolCode, 32, nameof(schoolCode));
        DisplayName = ContractGuard.Text(displayName, 200, nameof(displayName));
        LegalName = ContractGuard.OptionalText(legalName, 250, nameof(legalName));
        SchoolType = ContractGuard.Defined(schoolType, nameof(schoolType));
        DeploymentMode = ContractGuard.Defined(deploymentMode, nameof(deploymentMode));
        Address = ContractGuard.OptionalText(address, 500, nameof(address));
        if (unitTypeId == Guid.Empty)
        {
            throw new ArgumentException("UnitTypeId cannot be empty when supplied.", nameof(unitTypeId));
        }

        UnitTypeId = unitTypeId;
        RequestedAtUtc = ContractGuard.Utc(requestedAtUtc, nameof(requestedAtUtc));
    }

    public static string MessageType =>
        "mdaresna.schools.registry.school-registration-requested";

    public static ushort SchemaVersion => 1;

    [JsonPropertyName("registrationRequestId")]
    public Guid RegistrationRequestId { get; }

    [JsonPropertyName("tenantId")]
    public TenantId TenantId { get; }

    [JsonPropertyName("requestedByAccountId")]
    public Guid RequestedByAccountId { get; }

    [JsonPropertyName("schoolCode")]
    public string SchoolCode { get; }

    [JsonPropertyName("displayName")]
    public string DisplayName { get; }

    [JsonPropertyName("legalName")]
    public string? LegalName { get; }

    [JsonPropertyName("schoolType")]
    public SchoolTypeV1 SchoolType { get; }

    [JsonPropertyName("deploymentMode")]
    public SchoolDeploymentModeV1 DeploymentMode { get; }

    [JsonPropertyName("address")]
    public string? Address { get; }

    [JsonPropertyName("unitTypeId")]
    public Guid? UnitTypeId { get; }

    [JsonPropertyName("requestedAtUtc")]
    public DateTimeOffset RequestedAtUtc { get; }
}
