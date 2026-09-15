using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Application.Registry.Read;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Registry;

public sealed class RegistryReadServiceTests
{
    [Fact]
    public async Task List_queries_normalize_search_and_preserve_filters_and_paging()
    {
        var store = new FakeReadStore();
        var service = new RegistryReadService(store);
        var tenantId = TenantId.New();

        await service.ListTenantsAsync(new ListTenantsQuery("  academy  ", TenantStatus.Active, 2, 10));
        await service.ListSchoolsAsync(new ListSchoolsQuery(
            tenantId, "  school  ", SchoolLifecycleStatus.Approved, SchoolType.Private, 3, 5,
            DisplayName: "  academy  ", Address: "  cairo  ", UnitType: "  coin  ", Owner: "  owner  "));

        Assert.Equal("academy", store.LastTenantQuery?.Search);
        Assert.Equal(2, store.LastTenantQuery?.PageNumber);
        Assert.Equal("school", store.LastSchoolQuery?.Search);
        Assert.Equal(tenantId, store.LastSchoolQuery?.TenantId);
        Assert.Equal(SchoolLifecycleStatus.Approved, store.LastSchoolQuery?.Status);
        Assert.Equal(3, store.LastSchoolQuery?.PageNumber);
        Assert.Equal("academy", store.LastSchoolQuery?.DisplayName);
        Assert.Equal("cairo", store.LastSchoolQuery?.Address);
        Assert.Equal("coin", store.LastSchoolQuery?.UnitType);
        Assert.Equal("owner", store.LastSchoolQuery?.Owner);
    }

    [Fact]
    public async Task Invalid_pages_and_unknown_statuses_are_rejected_before_storage()
    {
        var store = new FakeReadStore();
        var service = new RegistryReadService(store);

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.ListTenantsAsync(new ListTenantsQuery(PageNumber: 0)));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.ListSchoolsAsync(new ListSchoolsQuery(PageSize: 101)));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            service.ListSchoolsAsync(new ListSchoolsQuery(Status: (SchoolLifecycleStatus)99)));

        Assert.Null(store.LastTenantQuery);
        Assert.Null(store.LastSchoolQuery);
    }

    [Fact]
    public async Task Missing_detail_returns_stable_not_found_error()
    {
        var service = new RegistryReadService(new FakeReadStore());

        var tenantError = await Assert.ThrowsAsync<PlatformResourceNotFoundException>(() =>
            service.GetTenantAsync(TenantId.New()));
        var schoolError = await Assert.ThrowsAsync<PlatformResourceNotFoundException>(() =>
            service.GetSchoolAsync(SchoolId.New()));

        Assert.Equal("tenant.not_found", tenantError.Code);
        Assert.Equal("school.not_found", schoolError.Code);
    }

    private sealed class FakeReadStore : IRegistryReadStore
    {
        public ListTenantsQuery? LastTenantQuery { get; private set; }
        public ListSchoolsQuery? LastSchoolQuery { get; private set; }

        public Task<RegistryPage<TenantReadModel>> ListTenantsAsync(
            ListTenantsQuery query,
            CancellationToken cancellationToken = default)
        {
            LastTenantQuery = query;
            return Task.FromResult(new RegistryPage<TenantReadModel>([], 0, query.PageNumber, query.PageSize));
        }

        public Task<TenantReadModel?> FindTenantAsync(
            TenantId tenantId,
            CancellationToken cancellationToken = default) => Task.FromResult<TenantReadModel?>(null);

        public Task<RegistryPage<SchoolReadModel>> ListSchoolsAsync(
            ListSchoolsQuery query,
            CancellationToken cancellationToken = default)
        {
            LastSchoolQuery = query;
            return Task.FromResult(new RegistryPage<SchoolReadModel>([], 0, query.PageNumber, query.PageSize));
        }

        public Task<SchoolReadModel?> FindSchoolAsync(
            SchoolId schoolId,
            CancellationToken cancellationToken = default) => Task.FromResult<SchoolReadModel?>(null);
    }
}
