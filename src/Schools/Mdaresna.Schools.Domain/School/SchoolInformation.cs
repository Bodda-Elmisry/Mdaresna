namespace Mdaresna.Schools.Domain.School;

public sealed class SchoolInformation
{
    private SchoolInformation() { }

    public Guid Id { get; private set; }
    public Guid PlatformSchoolReferenceId { get; private set; }
    public Guid PlatformTenantReferenceId { get; private set; }
    public Guid PlatformRegistrationRequestReferenceId { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string SchoolType { get; private set; } = string.Empty;
    public string DeploymentMode { get; private set; } = string.Empty;
    public string Status { get; private set; } = string.Empty;
    public string? Address { get; private set; }
    public string? PrimaryPhone { get; private set; }
    public Guid PlatformUnitTypeReferenceId { get; private set; }
    public string UnitTypeCode { get; private set; } = string.Empty;
    public string UnitTypeName { get; private set; } = string.Empty;
    public decimal UnitPrice { get; private set; }
    public string Currency { get; private set; } = string.Empty;
    public Guid OwnerPlatformAccountReferenceId { get; private set; }
    public DateTimeOffset PlatformCreatedAtUtc { get; private set; }
    public DateTimeOffset ActivatedAtUtc { get; private set; }
    public DateTimeOffset CreatedAtUtc { get; private set; }
    public DateTimeOffset UpdatedAtUtc { get; private set; }
    public byte[] RowVersion { get; private set; } = [];

    public static SchoolInformation Create(Guid platformSchoolReferenceId, Guid platformTenantReferenceId,
        Guid platformRegistrationRequestReferenceId, string code, string displayName, string schoolType,
        string deploymentMode, string? address, string? primaryPhone, Guid platformUnitTypeReferenceId,
        string unitTypeCode, string unitTypeName, decimal unitPrice, string currency,
        Guid ownerPlatformAccountReferenceId, DateTimeOffset platformCreatedAtUtc, DateTimeOffset activatedAtUtc)
    {
        if (platformSchoolReferenceId == Guid.Empty || platformTenantReferenceId == Guid.Empty ||
            platformRegistrationRequestReferenceId == Guid.Empty || platformUnitTypeReferenceId == Guid.Empty ||
            ownerPlatformAccountReferenceId == Guid.Empty) throw new ArgumentException("School references cannot be empty.");
        if (platformSchoolReferenceId != platformTenantReferenceId)
            throw new ArgumentException("The current one-school-per-tenant model requires matching school and tenant references.");
        if (platformCreatedAtUtc.Offset != TimeSpan.Zero || activatedAtUtc.Offset != TimeSpan.Zero)
            throw new ArgumentException("School timestamps must be UTC.");
        return new SchoolInformation { Id = Guid.NewGuid(), PlatformSchoolReferenceId = platformSchoolReferenceId,
            PlatformTenantReferenceId = platformTenantReferenceId,
            PlatformRegistrationRequestReferenceId = platformRegistrationRequestReferenceId,
            Code = Required(code, 32), DisplayName = Required(displayName, 200),
            SchoolType = Required(schoolType, 32), DeploymentMode = Required(deploymentMode, 32), Status = "Active",
            Address = Optional(address, 500), PrimaryPhone = Optional(primaryPhone, 32),
            PlatformUnitTypeReferenceId = platformUnitTypeReferenceId, UnitTypeCode = Required(unitTypeCode, 32),
            UnitTypeName = Required(unitTypeName, 200), UnitPrice = unitPrice,
            Currency = Required(currency, 3).ToUpperInvariant(), OwnerPlatformAccountReferenceId = ownerPlatformAccountReferenceId,
            PlatformCreatedAtUtc = platformCreatedAtUtc, ActivatedAtUtc = activatedAtUtc,
            CreatedAtUtc = activatedAtUtc, UpdatedAtUtc = activatedAtUtc };
    }

    private static string Required(string value, int max) => string.IsNullOrWhiteSpace(value) || value.Trim().Length > max
        ? throw new ArgumentException("School information value is invalid.") : value.Trim();
    private static string? Optional(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : Required(value, max);
}
