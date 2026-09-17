using System.Text.Json.Serialization;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Schools.Contracts.Provisioning;

public sealed record ProvisionSchoolV1 : IIntegrationCommand
{
    [JsonConstructor]
    public ProvisionSchoolV1(Guid operationId, TenantId tenantId, SchoolId schoolId,
        Guid registrationRequestId, string schoolCode, string displayName, string schoolType,
        string deploymentMode, string? address, string? primaryPhone, Guid unitTypeId,
        string unitTypeCode, string unitTypeName, decimal unitPrice, string currency,
        Guid ownerPlatformAccountId, DateTimeOffset platformCreatedAtUtc, DateTimeOffset requestedAtUtc)
    {
        if (operationId == Guid.Empty || tenantId.IsEmpty || schoolId.IsEmpty)
            throw new ArgumentException("Provisioning identifiers cannot be empty.");
        if (string.IsNullOrWhiteSpace(schoolCode) || schoolCode.Trim().Length > 32)
            throw new ArgumentException("School code is invalid.", nameof(schoolCode));
        if (string.IsNullOrWhiteSpace(displayName) || displayName.Trim().Length > 200)
            throw new ArgumentException("School display name is invalid.", nameof(displayName));
        if (requestedAtUtc.Offset != TimeSpan.Zero)
            throw new ArgumentException("RequestedAtUtc must be UTC.", nameof(requestedAtUtc));
        if (registrationRequestId == Guid.Empty || unitTypeId == Guid.Empty || ownerPlatformAccountId == Guid.Empty)
            throw new ArgumentException("School snapshot references cannot be empty.");
        OperationId = operationId;
        TenantId = tenantId;
        SchoolId = schoolId;
        RegistrationRequestId = registrationRequestId;
        SchoolCode = schoolCode.Trim();
        DisplayName = displayName.Trim();
        SchoolType = schoolType;
        DeploymentMode = deploymentMode;
        Address = address;
        PrimaryPhone = primaryPhone;
        UnitTypeId = unitTypeId;
        UnitTypeCode = unitTypeCode;
        UnitTypeName = unitTypeName;
        UnitPrice = unitPrice;
        Currency = currency;
        OwnerPlatformAccountId = ownerPlatformAccountId;
        PlatformCreatedAtUtc = platformCreatedAtUtc;
        RequestedAtUtc = requestedAtUtc;
    }

    public static string MessageType => "mdaresna.platform.registry.provision-school";
    public static ushort SchemaVersion => 1;
    [JsonPropertyName("operationId")] public Guid OperationId { get; }
    [JsonPropertyName("tenantId")] public TenantId TenantId { get; }
    [JsonPropertyName("schoolId")] public SchoolId SchoolId { get; }
    [JsonPropertyName("registrationRequestId")] public Guid RegistrationRequestId { get; }
    [JsonPropertyName("schoolCode")] public string SchoolCode { get; }
    [JsonPropertyName("displayName")] public string DisplayName { get; }
    [JsonPropertyName("schoolType")] public string SchoolType { get; }
    [JsonPropertyName("deploymentMode")] public string DeploymentMode { get; }
    [JsonPropertyName("address")] public string? Address { get; }
    [JsonPropertyName("primaryPhone")] public string? PrimaryPhone { get; }
    [JsonPropertyName("unitTypeId")] public Guid UnitTypeId { get; }
    [JsonPropertyName("unitTypeCode")] public string UnitTypeCode { get; }
    [JsonPropertyName("unitTypeName")] public string UnitTypeName { get; }
    [JsonPropertyName("unitPrice")] public decimal UnitPrice { get; }
    [JsonPropertyName("currency")] public string Currency { get; }
    [JsonPropertyName("ownerPlatformAccountId")] public Guid OwnerPlatformAccountId { get; }
    [JsonPropertyName("platformCreatedAtUtc")] public DateTimeOffset PlatformCreatedAtUtc { get; }
    [JsonPropertyName("requestedAtUtc")] public DateTimeOffset RequestedAtUtc { get; }
}
