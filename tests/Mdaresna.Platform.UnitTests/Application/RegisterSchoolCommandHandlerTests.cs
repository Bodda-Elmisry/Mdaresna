using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Application.Registry.RegisterSchool;
using Mdaresna.Platform.Contracts.Registry;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.UnitTests.TestDoubles;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Application;

public sealed class RegisterSchoolCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 12, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Replaying_the_same_registration_is_idempotent()
    {
        var fixture = CreateFixture();
        var command = CreateCommand(fixture.Tenant.Id);

        var first = await fixture.Handler.HandleAsync(command);
        var second = await fixture.Handler.HandleAsync(command);

        Assert.True(first.WasCreated);
        Assert.False(second.WasCreated);
        Assert.Equal(first.SchoolId, second.SchoolId);
        Assert.Equal(fixture.Tenant.Id.Value, first.SchoolId.Value);
        Assert.Single(fixture.Schools.Items);
        Assert.Single(fixture.Outbox.Messages);
        Assert.Single(fixture.Audit.Records);
        Assert.Equal(command.RequestedByAccountId, fixture.Audit.Records[0].ActorId);
        Assert.Equal(command.CorrelationId, fixture.Audit.Records[0].CorrelationId);
        Assert.Equal(1, fixture.UnitOfWork.SaveCount);

        var envelope = fixture.Outbox.Single<SchoolRegisteredV1>();
        Assert.Equal(command.RegistrationRequestId, envelope.Data.RegistrationRequestId);
        Assert.Equal(fixture.Tenant.Id, envelope.Scope.TenantId);
        Assert.Equal(first.SchoolId, envelope.Scope.SchoolId);
        Assert.Empty(fixture.Schools.Items.Single().DomainEvents);
    }

    [Fact]
    public async Task Reusing_registration_request_with_different_data_is_rejected()
    {
        var fixture = CreateFixture();
        var original = CreateCommand(fixture.Tenant.Id);
        await fixture.Handler.HandleAsync(original);
        var conflicting = original with { DisplayName = "A Different School" };

        var exception = await Assert.ThrowsAsync<PlatformConflictException>(() =>
            fixture.Handler.HandleAsync(conflicting));

        Assert.Equal("request.idempotency_key_reused", exception.Code);
        Assert.Single(fixture.Schools.Items);
        Assert.Single(fixture.Outbox.Messages);
        Assert.Single(fixture.Audit.Records);
        Assert.Equal(1, fixture.UnitOfWork.SaveCount);
    }

    [Fact]
    public async Task Different_registration_request_cannot_create_second_school_for_tenant()
    {
        var fixture = CreateFixture();
        var first = CreateCommand(fixture.Tenant.Id);
        await fixture.Handler.HandleAsync(first);

        var exception = await Assert.ThrowsAsync<PlatformConflictException>(() =>
            fixture.Handler.HandleAsync(first with
            {
                RegistrationRequestId = Guid.NewGuid(),
                SchoolCode = "SCHOOL-002"
            }));

        Assert.Equal("tenant.school_already_registered", exception.Code);
        Assert.Single(fixture.Schools.Items);
    }

    [Fact]
    public async Task Domain_events_are_retained_when_saving_fails()
    {
        var tenant = Tenant.Create(TenantId.New(), "Tenant One", null, Now);
        var schools = new FakeSchoolRegistrationRepository();
        var audit = new FakeRegistryAuditWriter();
        var handler = new RegisterSchoolCommandHandler(
            new FakeTenantRepository(tenant),
            schools,
            new FakeOutboxWriter(),
            audit,
            new FakePlatformUnitOfWork(new InvalidOperationException("Save failed.")),
            new FakeClock(Now));

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.HandleAsync(CreateCommand(tenant.Id)));

        Assert.NotEmpty(Assert.Single(schools.Items).DomainEvents);
        Assert.Single(audit.Records);
    }

    [Fact]
    public async Task Concurrent_unique_registration_is_reported_as_stable_conflict()
    {
        var tenant = Tenant.Create(TenantId.New(), "Tenant One", null, Now);
        var schools = new FakeSchoolRegistrationRepository();
        var handler = new RegisterSchoolCommandHandler(
            new FakeTenantRepository(tenant),
            schools,
            new FakeOutboxWriter(),
            new FakeRegistryAuditWriter(),
            new FakePlatformUnitOfWork(new PlatformConflictException(
                "registry.concurrent_conflict", "Concurrent registration.")),
            new FakeClock(Now));

        var exception = await Assert.ThrowsAsync<PlatformConflictException>(() =>
            handler.HandleAsync(CreateCommand(tenant.Id)));

        Assert.Equal("registry.concurrent_conflict", exception.Code);
        Assert.NotEmpty(Assert.Single(schools.Items).DomainEvents);
    }

    private static RegisterSchoolFixture CreateFixture()
    {
        var tenant = Tenant.Create(TenantId.New(), "Tenant One", null, Now);
        var tenants = new FakeTenantRepository(tenant);
        var schools = new FakeSchoolRegistrationRepository();
        var outbox = new FakeOutboxWriter();
        var audit = new FakeRegistryAuditWriter();
        var unitOfWork = new FakePlatformUnitOfWork();
        var handler = new RegisterSchoolCommandHandler(
            tenants,
            schools,
            outbox,
            audit,
            unitOfWork,
            new FakeClock(Now));

        return new RegisterSchoolFixture(tenant, schools, outbox, audit, unitOfWork, handler);
    }

    private static RegisterSchoolCommand CreateCommand(TenantId tenantId) => new(
        Guid.NewGuid(),
        tenantId,
        " school-001 ",
        " School One ",
        SchoolType.Private,
        DeploymentMode.SharedSaaS,
        IdentityAccountId.New(),
        Guid.NewGuid());

    private sealed record RegisterSchoolFixture(
        Tenant Tenant,
        FakeSchoolRegistrationRepository Schools,
        FakeOutboxWriter Outbox,
        FakeRegistryAuditWriter Audit,
        FakePlatformUnitOfWork UnitOfWork,
        RegisterSchoolCommandHandler Handler);
}
