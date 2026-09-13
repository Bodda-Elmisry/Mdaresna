using Mdaresna.Platform.Application.Errors;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.Read;

public sealed class RegistryReadService(IRegistryReadStore store)
{
    public Task<RegistryPage<TenantReadModel>> ListTenantsAsync(
        ListTenantsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ValidatePage(query.PageNumber, query.PageSize);
        ValidateSearch(query.Search);
        if (query.Status.HasValue && !Enum.IsDefined(query.Status.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(query));
        }

        return store.ListTenantsAsync(query with { Search = NormalizeSearch(query.Search) }, cancellationToken);
    }

    public async Task<TenantReadModel> GetTenantAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        if (tenantId.IsEmpty)
        {
            throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        }

        return await store.FindTenantAsync(tenantId, cancellationToken)
            ?? throw new PlatformResourceNotFoundException(
                "tenant.not_found",
                $"Tenant '{tenantId}' was not found.");
    }

    public Task<RegistryPage<SchoolReadModel>> ListSchoolsAsync(
        ListSchoolsQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        ValidatePage(query.PageNumber, query.PageSize);
        ValidateSearch(query.Search);
        if (query.TenantId is { IsEmpty: true } ||
            query.Status.HasValue && !Enum.IsDefined(query.Status.Value) ||
            query.SchoolType.HasValue && !Enum.IsDefined(query.SchoolType.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(query));
        }

        return store.ListSchoolsAsync(query with { Search = NormalizeSearch(query.Search) }, cancellationToken);
    }

    public async Task<SchoolReadModel> GetSchoolAsync(
        SchoolId schoolId,
        CancellationToken cancellationToken = default)
    {
        if (schoolId.IsEmpty)
        {
            throw new ArgumentException("SchoolId cannot be empty.", nameof(schoolId));
        }

        return await store.FindSchoolAsync(schoolId, cancellationToken)
            ?? throw new PlatformResourceNotFoundException(
                "school.not_found",
                $"School '{schoolId}' was not found.");
    }

    private static void ValidatePage(int pageNumber, int pageSize)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100 ||
            ((long)pageNumber - 1) * pageSize > int.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(pageNumber),
                "Page number must be positive and page size must be between 1 and 100.");
        }
    }

    private static void ValidateSearch(string? search)
    {
        if (search?.Trim().Length > 200)
        {
            throw new ArgumentOutOfRangeException(nameof(search));
        }
    }

    private static string? NormalizeSearch(string? search) =>
        string.IsNullOrWhiteSpace(search) ? null : search.Trim();
}
