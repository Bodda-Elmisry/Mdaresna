using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.Read;

public interface IRegistryReadStore
{
    Task<RegistryPage<TenantReadModel>> ListTenantsAsync(
        ListTenantsQuery query,
        CancellationToken cancellationToken = default);

    Task<TenantReadModel?> FindTenantAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default);

    Task<RegistryPage<SchoolReadModel>> ListSchoolsAsync(
        ListSchoolsQuery query,
        CancellationToken cancellationToken = default);

    Task<SchoolDirectorySummary> GetSchoolSummaryAsync(
        CancellationToken cancellationToken = default);

    Task<SchoolReadModel?> FindSchoolAsync(
        SchoolId schoolId,
        CancellationToken cancellationToken = default);
}
