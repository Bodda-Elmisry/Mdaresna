using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Tenancy.Abstractions.Context;

public sealed record TenantContext
{
    public TenantContext(
        TenantId tenantId,
        SchoolId? schoolId = null,
        BranchId? branchId = null)
    {
        if (tenantId.IsEmpty)
        {
            throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        }

        if (schoolId is { IsEmpty: true })
        {
            throw new ArgumentException("SchoolId cannot be empty.", nameof(schoolId));
        }

        if (branchId is { IsEmpty: true })
        {
            throw new ArgumentException("BranchId cannot be empty.", nameof(branchId));
        }

        if (branchId.HasValue && !schoolId.HasValue)
        {
            throw new ArgumentException(
                "A branch context requires a school context.",
                nameof(branchId));
        }

        TenantId = tenantId;
        SchoolId = schoolId;
        BranchId = branchId;
    }

    public TenantId TenantId { get; }

    public SchoolId? SchoolId { get; }

    public BranchId? BranchId { get; }
}
