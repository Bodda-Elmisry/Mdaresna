using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Application.Registry.CreateTenant;
using Mdaresna.Platform.Contracts.Registry;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.UnitTests.TestDoubles;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Application;

public sealed class CreateTenantCommandHandlerTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 12, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Replaying_the_same_tenant_request_is_idempotent()
    {
        var repository = new FakeTenantRepository();
        var outbox = new FakeOutboxWriter();
        var audit = new FakeRegistryAuditWriter();
        var unitOfWork = new FakePlatformUnitOfWork();
        var handler = new CreateTenantCommandHandler(
            repository,
            outbox,
            audit,
            unitOfWork,
            new FakeClock(Now));
        var command = new CreateTenantCommand(
            TenantId.New(),
            " Tenant One ",
            " Tenant One LLC ",
            IdentityAccountId.New(),
            Guid.NewGuid());

        var first = await handler.HandleAsync(command);
        var second = await handler.HandleAsync(command);

        Assert.True(first.WasCreated);
        Assert.False(second.WasCreated);
        Assert.Equal(first.TenantId, second.TenantId);
        Assert.Single(outbox.Messages);
        Assert.Single(audit.Records);
        Assert.Equal(command.RequestedByAccountId, audit.Records[0].ActorId);
        Assert.Equal(command.CorrelationId, audit.Records[0].CorrelationId);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Equal(first.TenantId, outbox.Single<TenantRegisteredV1>().Data.TenantId);
        var savedTenant = await repository.FindByIdAsync(first.TenantId);
        Assert.NotNull(savedTenant);
        Assert.Empty(savedTenant.DomainEvents);
    }

    [Fact]
    public async Task Reusing_a_tenant_id_with_different_data_is_rejected()
    {
        var repository = new FakeTenantRepository();
        var outbox = new FakeOutboxWriter();
        var audit = new FakeRegistryAuditWriter();
        var unitOfWork = new FakePlatformUnitOfWork();
        var handler = new CreateTenantCommandHandler(
            repository,
            outbox,
            audit,
            unitOfWork,
            new FakeClock(Now));
        var original = new CreateTenantCommand(
            TenantId.New(),
            "Tenant One",
            null,
            IdentityAccountId.New(),
            Guid.NewGuid());
        await handler.HandleAsync(original);

        var exception = await Assert.ThrowsAsync<PlatformConflictException>(() =>
            handler.HandleAsync(original with { DisplayName = "Different Tenant" }));

        Assert.Equal("tenant.id_reused", exception.Code);
        Assert.Single(outbox.Messages);
        Assert.Single(audit.Records);
        Assert.Equal(1, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Concurrent_unique_registration_is_reported_as_stable_conflict()
    {
        var repository = new FakeTenantRepository();
        var handler = new CreateTenantCommandHandler(
            repository,
            new FakeOutboxWriter(),
            new FakeRegistryAuditWriter(),
            new FakePlatformUnitOfWork(new PlatformConflictException(
                "registry.concurrent_conflict", "Concurrent registration.")),
            new FakeClock(Now));
        var command = new CreateTenantCommand(
            TenantId.New(), "Tenant One", null, IdentityAccountId.New(), Guid.NewGuid());

        var exception = await Assert.ThrowsAsync<PlatformConflictException>(() =>
            handler.HandleAsync(command));

        Assert.Equal("registry.concurrent_conflict", exception.Code);
        Assert.NotEmpty((await repository.FindByIdAsync(command.TenantId))!.DomainEvents);
    }
}
