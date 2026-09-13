using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using Mdaresna.SharedKernel.Time;
using Mdaresna.Tenancy.Abstractions.Identifiers;

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
