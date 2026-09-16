using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Schools.Domain;

/// <summary>
/// Identifies the tenant and school whose isolated operational database is being used.
/// </summary>
public sealed record SchoolScope
{
    private SchoolScope(TenantId tenantId, SchoolId schoolId)
    {
        TenantId = tenantId;
        SchoolId = schoolId;
    }

    public TenantId TenantId { get; }

    public SchoolId SchoolId { get; }

    public static SchoolScope Create(TenantId tenantId, SchoolId schoolId)
    {
        if (tenantId.IsEmpty) throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));
        if (schoolId.IsEmpty) throw new ArgumentException("School ID cannot be empty.", nameof(schoolId));
        return new SchoolScope(tenantId, schoolId);
    }
}
