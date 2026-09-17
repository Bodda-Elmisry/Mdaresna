using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Application.Billing.Units;
using Mdaresna.Platform.Application.Registry.ConsumeSchoolRegistrationRequest;
using Mdaresna.Platform.Application.Registry.CreateTenant;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using Mdaresna.Platform.Application.Registry.RegisterSchool;
using Mdaresna.Platform.Contracts.Registry;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Billing.Units;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.UnitTests.TestDoubles;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Registry;

public sealed class ConsumeSchoolRegistrationRequestHandlerTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Valid_event_creates_tenant_and_pending_school_idempotently()
    {
        var requesterId = Guid.NewGuid();
        var tenants = new FakeTenantRepository();
        var schools = new FakeSchoolRegistrationRepository();
        var outbox = new FakeOutboxWriter();
        var audit = new FakeRegistryAuditWriter();
        var unitOfWork = new FakePlatformUnitOfWork();
        var clock = new FakeClock(Now);
        var handler = new ConsumeSchoolRegistrationRequestHandler(
            new ExistingAccountLookup(requesterId),
            new EmptyUnitTypeRepository(),
            schools,
            new CreateTenantCommandHandler(tenants, outbox, audit, unitOfWork, clock),
            new RegisterSchoolCommandHandler(
                tenants, schools, outbox, audit, unitOfWork, clock),
            new TransitionSchoolCommandHandler(
                schools, new FakeUnitTypeRepository(), outbox, audit, unitOfWork, clock));
        var envelope = RequestEvent(requesterId);

        await handler.HandleAsync(envelope);
        await handler.HandleAsync(envelope);

        Assert.NotNull(await tenants.FindByIdAsync(envelope.Data.TenantId));
        var school = Assert.Single(schools.Items);
        Assert.Equal(SchoolLifecycleStatus.PendingVerification, school.Status);
        Assert.Equal(envelope.Data.Address, school.Address);
        Assert.Equal(3, outbox.Messages.Count);
        Assert.Equal(3, audit.Records.Count);
        Assert.Equal(3, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Event_from_a_different_producer_is_rejected()
    {
        var requesterId = Guid.NewGuid();
        var valid = RequestEvent(requesterId);
        var invalid = new IntegrationMessageEnvelope<SchoolRegistrationRequestedV1>(
            valid.MessageId,
            valid.MessageType,
            valid.SchemaVersion,
            valid.OccurredAtUtc,
            "platform",
            valid.Scope,
            valid.Aggregate,
            valid.CorrelationId,
            valid.CausationId,
            valid.TraceParent,
            valid.Data);
        var tenants = new FakeTenantRepository();
        var schools = new FakeSchoolRegistrationRepository();
        var outbox = new FakeOutboxWriter();
        var audit = new FakeRegistryAuditWriter();
        var unitOfWork = new FakePlatformUnitOfWork();
        var clock = new FakeClock(Now);
        var handler = new ConsumeSchoolRegistrationRequestHandler(
            new ExistingAccountLookup(requesterId),
            new EmptyUnitTypeRepository(),
            schools,
            new CreateTenantCommandHandler(tenants, outbox, audit, unitOfWork, clock),
            new RegisterSchoolCommandHandler(
                tenants, schools, outbox, audit, unitOfWork, clock),
            new TransitionSchoolCommandHandler(
                schools, new FakeUnitTypeRepository(), outbox, audit, unitOfWork, clock));

        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(invalid));
        Assert.Empty(schools.Items);
    }

    private static IntegrationMessageEnvelope<SchoolRegistrationRequestedV1> RequestEvent(
        Guid requesterId)
    {
        var tenantId = TenantId.New();
        var data = new SchoolRegistrationRequestedV1(
            Guid.NewGuid(),
            tenantId,
            requesterId,
            "SCH-101",
            "Future School",
            "Future School LLC",
            SchoolTypeV1.Private,
            SchoolDeploymentModeV1.SharedSaaS,
            "Cairo",
            null,
            Now);
        return IntegrationMessageEnvelope<SchoolRegistrationRequestedV1>.Create(
            Now,
            "schools",
            IntegrationMessageScope.ForTenant(tenantId),
            data);
    }

    private sealed class ExistingAccountLookup(Guid accountId) : ISharedIdentityAccountLookup
    {
        public Task<bool> ExistsAsync(
            IdentityAccountId candidate,
            CancellationToken cancellationToken = default) =>
            Task.FromResult((Guid)candidate == accountId);
    }

    private sealed class EmptyUnitTypeRepository : IUnitTypeRepository
    {
        public Task<UnitType?> FindByIdAsync(
            Guid unitTypeId,
            CancellationToken cancellationToken = default) => Task.FromResult<UnitType?>(null);

        public Task<UnitType?> FindByCodeAsync(
            string normalizedCode,
            CancellationToken cancellationToken = default) => Task.FromResult<UnitType?>(null);

        public Task AddAsync(
            UnitType unitType,
            CancellationToken cancellationToken = default) => Task.CompletedTask;

        public Task<bool> HasUsageAsync(
            Guid unitTypeId,
            CancellationToken cancellationToken = default) => Task.FromResult(false);

        public void Remove(UnitType unitType)
        {
        }

        public Task<UnitTypePage> ListAsync(
            UnitTypeListQuery query,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new UnitTypePage([], 0, query.PageNumber, query.PageSize));
    }
}
