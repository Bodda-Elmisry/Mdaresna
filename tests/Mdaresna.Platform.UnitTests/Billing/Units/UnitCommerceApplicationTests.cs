using Mdaresna.Platform.Application.Billing.Units;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Billing.Units;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.UnitTests.Billing;
using Mdaresna.Platform.UnitTests.TestDoubles;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Billing.Units;

public sealed class UnitCommerceApplicationTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 13, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Catalog_create_update_and_deactivate_are_audited_and_versioned()
    {
        var types = new FakeUnitTypeRepository();
        var audit = new FakeUnitCommerceAuditWriter();
        var unitOfWork = new FakeBillingUnitOfWork();
        var clock = new FakeClock(Now);
        var actor = IdentityAccountId.New();
        var typeId = Guid.NewGuid();
        var create = new CreateUnitTypeCommand(typeId, "unit-001", "Unit", 10m,
            "egp", actor, Guid.NewGuid());

        var created = await new CreateUnitTypeCommandHandler(types, audit, unitOfWork, clock)
            .HandleAsync(create);
        var replay = await new CreateUnitTypeCommandHandler(types, audit, unitOfWork, clock)
            .HandleAsync(create);
        var updated = await new UpdateUnitTypeCommandHandler(types, audit, unitOfWork, clock)
            .HandleAsync(new UpdateUnitTypeCommand(typeId, created.Version,
                "New Unit", 12m, "EGP", actor, Guid.NewGuid()));
        var deactivated = await new DeactivateUnitTypeCommandHandler(types, audit, unitOfWork, clock)
            .HandleAsync(new DeactivateUnitTypeCommand(typeId, updated.Version,
                actor, Guid.NewGuid()));

        Assert.True(created.Changed);
        Assert.False(replay.Changed);
        Assert.Equal(1, updated.Version);
        Assert.Equal(2, deactivated.Version);
        Assert.False(deactivated.IsActive);
        Assert.Equal(3, unitOfWork.SaveCount);
        Assert.Equal(3, audit.Records.Count);
        Assert.All(audit.Records, record => Assert.Equal(actor, record.ActorId));
    }

    [Fact]
    public async Task Purchase_uses_server_price_snapshot_and_replays_after_catalog_price_change()
    {
        var actor = IdentityAccountId.New();
        var school = CreateSchool();
        var type = UnitType.Create(Guid.NewGuid(), "UNIT-002", "Old Unit", 25m, "EGP", Now);
        var types = new FakeUnitTypeRepository(type);
        var purchases = new FakeUnitPurchaseRepository();
        var payments = new FakePaymentRepository();
        var audit = new FakeUnitCommerceAuditWriter();
        var unitOfWork = new FakeBillingUnitOfWork();
        var handler = new SubmitUnitPurchaseCommandHandler(types, purchases, payments,
            new FakeSchoolRegistrationRepository(school), audit, unitOfWork, new FakeClock(Now));
        var command = new SubmitUnitPurchaseCommand(
            Guid.NewGuid(), school.TenantId, school.Id, type.Id, type.Version,
            4, " bank-123 ", " bank-transfer ", Now.AddMinutes(-1),
            actor, Guid.NewGuid());

        var first = await handler.HandleAsync(command);
        type.ChangeOffer("New Unit", 50m, "EGP", Now.AddMinutes(1));
        var replay = await handler.HandleAsync(command);

        Assert.True(first.WasCreated);
        Assert.False(replay.WasCreated);
        Assert.Equal(100m, first.Amount);
        Assert.Equal(25m, first.UnitPrice);
        Assert.Equal(100m, replay.Amount);
        Assert.Equal(100m, Assert.Single(payments.Requests).Amount);
        Assert.Equal(0, Assert.Single(purchases.Intents).UnitTypeVersion);
        Assert.Equal("Old Unit", Assert.Single(purchases.Intents).UnitTypeName);
        Assert.Single(audit.Records);
        Assert.Equal(actor, audit.Records[0].ActorId);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Reused_request_with_different_quantity_and_stale_new_quote_are_rejected()
    {
        var school = CreateSchool();
        var type = UnitType.Create(Guid.NewGuid(), "UNIT-003", "Unit", 25m, "EGP", Now);
        var types = new FakeUnitTypeRepository(type);
        var handler = new SubmitUnitPurchaseCommandHandler(types,
            new FakeUnitPurchaseRepository(), new FakePaymentRepository(),
            new FakeSchoolRegistrationRepository(school), new FakeUnitCommerceAuditWriter(),
            new FakeBillingUnitOfWork(), new FakeClock(Now));
        var command = new SubmitUnitPurchaseCommand(
            Guid.NewGuid(), school.TenantId, school.Id, type.Id, type.Version,
            2, "bank-456", "bank-transfer", Now.AddMinutes(-1),
            IdentityAccountId.New(), Guid.NewGuid());
        await handler.HandleAsync(command);

        var reused = await Assert.ThrowsAsync<PlatformConflictException>(() =>
            handler.HandleAsync(command with { Quantity = 3 }));
        Assert.Equal("request.idempotency_key_reused", reused.Code);

        type.ChangeOffer("New Unit", 30m, "EGP", Now.AddMinutes(1));
        var stale = await Assert.ThrowsAsync<PlatformConflictException>(() =>
            handler.HandleAsync(command with { RequestId = Guid.NewGuid() }));
        Assert.Equal("unit_type.version_conflict", stale.Code);
    }

    private static SchoolRegistration CreateSchool()
    {
        var tenantId = TenantId.New();
        return SchoolRegistration.Create(SchoolId.From(tenantId.Value), tenantId,
            Guid.NewGuid(), SchoolCode.Create("UNIT-SCHOOL"), "School",
            SchoolType.Private, DeploymentMode.SharedSaaS, Guid.NewGuid(), Now);
    }

    private sealed class FakeUnitTypeRepository(params UnitType[] types) : IUnitTypeRepository
    {
        private readonly List<UnitType> _types = [.. types];

        public Task<UnitType?> FindByIdAsync(Guid id, CancellationToken cancellationToken = default) =>
            Task.FromResult(_types.SingleOrDefault(item => item.Id == id));

        public Task<UnitType?> FindByCodeAsync(string code, CancellationToken cancellationToken = default) =>
            Task.FromResult(_types.SingleOrDefault(item => item.Code == code));

        public Task AddAsync(UnitType type, CancellationToken cancellationToken = default)
        {
            _types.Add(type);
            return Task.CompletedTask;
        }

        public Task<UnitTypePage> ListAsync(UnitTypeListQuery query,
            CancellationToken cancellationToken = default) =>
            Task.FromResult(new UnitTypePage([], _types.Count, query.PageNumber, query.PageSize));
    }

}
