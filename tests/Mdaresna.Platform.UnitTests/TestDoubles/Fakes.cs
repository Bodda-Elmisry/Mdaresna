using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using Mdaresna.SharedKernel.Time;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Mdaresna.Platform.Application.Billing.Units;
using Mdaresna.Platform.Domain.Billing.Units;

namespace Mdaresna.Platform.UnitTests.TestDoubles;

internal sealed class FakeClock(DateTimeOffset utcNow) : IClock
{
    public DateTimeOffset UtcNow { get; } = utcNow;
}

internal sealed class FakePlatformUnitOfWork(Exception? saveException = null) : IPlatformUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SaveCount++;

        if (saveException is not null)
        {
            throw saveException;
        }

        return Task.FromResult(1);
    }

    public async Task ExecuteInTransactionAsync(
        Func<CancellationToken, Task> operation,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(operation);
        await operation(cancellationToken);
    }
}

internal sealed class FakeUnitTypeRepository(params UnitType[] types) : IUnitTypeRepository
{
    public List<UnitType> Items { get; } = [.. types];
    public bool HasUsage { get; set; }
    public Task<UnitType?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.SingleOrDefault(x => x.Id == id));
    public Task<UnitType?> FindByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        Task.FromResult(Items.SingleOrDefault(x => x.Code == code));
    public Task AddAsync(UnitType item, CancellationToken cancellationToken = default) { Items.Add(item); return Task.CompletedTask; }
    public Task<bool> HasUsageAsync(Guid id, CancellationToken cancellationToken = default) => Task.FromResult(HasUsage);
    public void Remove(UnitType item) => Items.Remove(item);
    public Task<UnitTypePage> ListAsync(UnitTypeListQuery query, CancellationToken cancellationToken = default) =>
        Task.FromResult(new UnitTypePage([], Items.Count, query.PageNumber, query.PageSize));
}

internal sealed class FakeOutboxWriter : IPlatformOutboxWriter
{
    public List<object> Messages { get; } = [];

    public void Enqueue<TMessage>(IntegrationMessageEnvelope<TMessage> message)
        where TMessage : class, IIntegrationMessage => Messages.Add(message);

    public IntegrationMessageEnvelope<TMessage> Single<TMessage>()
        where TMessage : class, IIntegrationMessage =>
        Assert.Single(Messages.OfType<IntegrationMessageEnvelope<TMessage>>());
}

internal sealed class FakeRegistryAuditWriter : IPlatformRegistryAuditWriter
{
    public List<RegistryAuditRecord> Records { get; } = [];

    public void Stage(RegistryAuditRecord record) => Records.Add(record);
}

internal sealed class FakeTenantRepository(params Tenant[] tenants) : ITenantRepository
{
    private readonly Dictionary<TenantId, Tenant> _tenants = tenants.ToDictionary(item => item.Id);

    public Task<Tenant?> FindByIdAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _tenants.TryGetValue(tenantId, out var tenant);
        return Task.FromResult(tenant);
    }

    public Task AddAsync(Tenant tenant, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _tenants.Add(tenant.Id, tenant);
        return Task.CompletedTask;
    }
}

internal sealed class FakeSchoolRegistrationRepository(params SchoolRegistration[] schools)
    : ISchoolRegistrationRepository
{
    public List<SchoolRegistration> Items { get; } = [.. schools];

    public Task<SchoolRegistration?> FindByIdAsync(
        SchoolId schoolId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Items.SingleOrDefault(item => item.Id == schoolId));
    }

    public Task<SchoolRegistration?> FindByRegistrationRequestIdAsync(
        Guid registrationRequestId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Items.SingleOrDefault(
            item => item.RegistrationRequestId == registrationRequestId));
    }

    public Task<SchoolRegistration?> FindByCodeAsync(
        SchoolCode schoolCode,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Items.SingleOrDefault(item => item.Code == schoolCode));
    }

    public Task<bool> IsSchoolCodeInUseAsync(
        SchoolCode schoolCode,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Items.Any(item => item.Code == schoolCode));
    }

    public Task AddAsync(
        SchoolRegistration registration,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Items.Add(registration);
        return Task.CompletedTask;
    }
}

internal sealed class FakeSchoolDatabaseEndpointRepository(
    params SchoolDatabaseEndpoint[] endpoints) : ISchoolDatabaseEndpointRepository
{
    public List<SchoolDatabaseEndpoint> Items { get; } = [.. endpoints];

    public Task<SchoolDatabaseEndpoint?> FindByIdAsync(
        Guid endpointId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Items.SingleOrDefault(item => item.Id == endpointId));
    }

    public Task<IReadOnlyList<SchoolDatabaseEndpoint>> ListAsync(
        SchoolId schoolId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult<IReadOnlyList<SchoolDatabaseEndpoint>>(
            Items.Where(item => item.SchoolId == schoolId).ToArray());
    }

    public Task<SchoolDatabaseEndpoint?> FindPrimaryActiveAsync(
        SchoolId schoolId,
        SchoolDatabasePurpose purpose,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Items.SingleOrDefault(item =>
            item.SchoolId == schoolId && item.Purpose == purpose &&
            item.IsPrimary && item.Status == SchoolDatabaseEndpointStatus.Active));
    }

    public Task<SchoolDatabaseEndpoint?> FindPrimaryAsync(
        SchoolId schoolId,
        SchoolDatabasePurpose purpose,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Items.SingleOrDefault(item =>
            item.SchoolId == schoolId && item.Purpose == purpose && item.IsPrimary));
    }

    public Task<bool> TargetExistsAsync(
        SchoolId schoolId,
        string host,
        int port,
        string databaseName,
        Guid? excludingEndpointId = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(Items.Any(item =>
            item.SchoolId == schoolId && item.Host == host && item.Port == port &&
            item.DatabaseName == databaseName && item.Id != excludingEndpointId));
    }

    public Task AddAsync(
        SchoolDatabaseEndpoint endpoint,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Items.Add(endpoint);
        return Task.CompletedTask;
    }
}
