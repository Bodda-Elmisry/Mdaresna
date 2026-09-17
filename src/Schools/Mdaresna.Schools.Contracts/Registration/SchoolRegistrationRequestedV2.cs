using System.Text.Json.Serialization;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Schools.Contracts.Registration;

public enum RequestedSchoolTypeV2
{
    Private = 1,
    Government = 2
}

/// <summary>A public school request. The Platform resolves or creates the owner account.</summary>
public sealed record SchoolRegistrationRequestedV2 : IIntegrationEvent
{
    [JsonConstructor]
    public SchoolRegistrationRequestedV2(
        Guid registrationRequestId,
        TenantId tenantId,
        string schoolCode,
        string schoolName,
        RequestedSchoolTypeV2 schoolType,
        string address,
        string schoolPrimaryPhone,
        string ownerName,
        string ownerPhone,
        DateTimeOffset requestedAtUtc)
    {
        RegistrationRequestId = NonEmpty(registrationRequestId, nameof(registrationRequestId));
        if (tenantId.IsEmpty) throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        TenantId = tenantId;
        SchoolCode = Text(schoolCode, 32, nameof(schoolCode));
        SchoolName = Text(schoolName, 200, nameof(schoolName));
        if (!Enum.IsDefined(schoolType)) throw new ArgumentOutOfRangeException(nameof(schoolType));
        SchoolType = schoolType;
        Address = Text(address, 500, nameof(address));
        SchoolPrimaryPhone = Phone(schoolPrimaryPhone, nameof(schoolPrimaryPhone));
        OwnerName = Text(ownerName, 200, nameof(ownerName));
        OwnerPhone = Phone(ownerPhone, nameof(ownerPhone));
        if (requestedAtUtc == default || requestedAtUtc.Offset != TimeSpan.Zero)
            throw new ArgumentException("RequestedAtUtc must use the UTC offset.", nameof(requestedAtUtc));
        RequestedAtUtc = requestedAtUtc;
    }

    public static string MessageType => "mdaresna.schools.registry.school-registration-requested";
    public static ushort SchemaVersion => 2;

    [JsonPropertyName("registrationRequestId")] public Guid RegistrationRequestId { get; }
    [JsonPropertyName("tenantId")] public TenantId TenantId { get; }
    [JsonPropertyName("schoolCode")] public string SchoolCode { get; }
    [JsonPropertyName("schoolName")] public string SchoolName { get; }
    [JsonPropertyName("schoolType")] public RequestedSchoolTypeV2 SchoolType { get; }
    [JsonPropertyName("address")] public string Address { get; }
    [JsonPropertyName("schoolPrimaryPhone")] public string SchoolPrimaryPhone { get; }
    [JsonPropertyName("ownerName")] public string OwnerName { get; }
    [JsonPropertyName("ownerPhone")] public string OwnerPhone { get; }
    [JsonPropertyName("requestedAtUtc")] public DateTimeOffset RequestedAtUtc { get; }

    private static Guid NonEmpty(Guid value, string name) => value != Guid.Empty ? value : throw new ArgumentException($"{name} cannot be empty.", name);
    private static string Text(string value, int maximum, string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, name);
        var result = value.Trim();
        return result.Length <= maximum ? result : throw new ArgumentOutOfRangeException(name);
    }
    private static string Phone(string value, string name)
    {
        var result = Text(value, 16, name);
        return result.Length >= 8 && result.All(char.IsAsciiDigit)
            ? result
            : throw new ArgumentException("Phone must contain 8-16 ASCII digits.", name);
    }
}
