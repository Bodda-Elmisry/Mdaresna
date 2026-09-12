using System.Text.Json.Serialization;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.IntegrationContracts.Messaging;

public sealed record IntegrationMessageScope
{
    [JsonConstructor]
    public IntegrationMessageScope(
        TenantId? tenantId,
        SchoolId? schoolId,
        BranchId? branchId)
    {
        if (tenantId is { IsEmpty: true })
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

        if (schoolId.HasValue && !tenantId.HasValue)
        {
            throw new ArgumentException(
                "A school-scoped message requires TenantId.",
                nameof(schoolId));
        }

        if (branchId.HasValue && !schoolId.HasValue)
        {
            throw new ArgumentException(
                "A branch-scoped message requires SchoolId.",
                nameof(branchId));
        }

        TenantId = tenantId;
        SchoolId = schoolId;
        BranchId = branchId;
    }

    [JsonPropertyName("tenantId")]
    public TenantId? TenantId { get; }

    [JsonPropertyName("schoolId")]
    public SchoolId? SchoolId { get; }

    [JsonPropertyName("branchId")]
    public BranchId? BranchId { get; }

    public static IntegrationMessageScope Global { get; } = new(null, null, null);

    public static IntegrationMessageScope ForTenant(TenantId tenantId) =>
        new(tenantId, null, null);

    public static IntegrationMessageScope ForSchool(
        TenantId tenantId,
        SchoolId schoolId) => new(tenantId, schoolId, null);

    public static IntegrationMessageScope ForBranch(
        TenantId tenantId,
        SchoolId schoolId,
        BranchId branchId) => new(tenantId, schoolId, branchId);
}
