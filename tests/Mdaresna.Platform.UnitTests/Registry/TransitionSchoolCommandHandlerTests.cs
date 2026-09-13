using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using Mdaresna.Platform.Contracts.Registry;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Common;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.UnitTests.TestDoubles;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Registry;

public sealed class TransitionSchoolCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 13, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Submitting_and_approving_stage_lifecycle_event_and_audit_atomically()
    {
        var actor = IdentityAccountId.New();
        var school = CreateDraftSchool((Guid)actor);
        var outbox = new FakeOutboxWriter();
        var audit = new FakeRegistryAuditWriter();
        var unitOfWork = new FakePlatformUnitOfWork();
        var handler = CreateHandler(school, outbox, audit, unitOfWork);

        var submitted = await handler.HandleAsync(Command(school, actor, SchoolLifecycleAction.SubmitForVerification));
        var approved = await handler.HandleAsync(Command(school, actor, SchoolLifecycleAction.Approve));

        Assert.Equal(SchoolLifecycleStatus.PendingVerification, submitted.Status);
        Assert.Equal(SchoolLifecycleStatus.Approved, approved.Status);
        Assert.Equal(school.Version, approved.Version);
        Assert.Equal(2, unitOfWork.SaveCount);
        Assert.Equal(2, audit.Records.Count);
        Assert.Equal(2, outbox.Messages.Count);
        Assert.Empty(school.DomainEvents);
        var lifecycleMessages = outbox.Messages
            .Cast<Mdaresna.IntegrationContracts.Messaging.IntegrationMessageEnvelope<SchoolLifecycleChangedV1>>()
            .ToArray();
        Assert.Equal(audit.Records[0].Id, lifecycleMessages[0].MessageId);
        Assert.Equal(audit.Records[1].Id, lifecycleMessages[1].MessageId);
        Assert.Equal(SchoolLifecycleStatusV1.Approved, lifecycleMessages[1].Data.CurrentStatus);
    }

    [Fact]
    public async Task Active_school_can_be_suspended_reinstated_and_closed_with_reasons()
    {
        var actor = IdentityAccountId.New();
        var school = CreateActiveSchool((Guid)actor);
        var outbox = new FakeOutboxWriter();
        var audit = new FakeRegistryAuditWriter();
        var handler = CreateHandler(school, outbox, audit, new FakePlatformUnitOfWork());

        await handler.HandleAsync(Command(school, actor, SchoolLifecycleAction.Suspend, "Contract overdue"));
        Assert.Equal(SchoolLifecycleStatus.Suspended, school.Status);
        Assert.Equal("Contract overdue", school.StatusReason);

        await handler.HandleAsync(Command(school, actor, SchoolLifecycleAction.Reinstate));
        Assert.Equal(SchoolLifecycleStatus.Active, school.Status);
        Assert.Null(school.StatusReason);

        await handler.HandleAsync(Command(school, actor, SchoolLifecycleAction.Close, "Contract ended"));
        Assert.Equal(SchoolLifecycleStatus.Closed, school.Status);
        Assert.Equal("Contract ended", school.StatusReason);
        Assert.Equal(3, audit.Records.Count);
        Assert.Equal(3, outbox.Messages.Count);
        Assert.Empty(school.DomainEvents);
    }

    [Fact]
    public async Task Wrong_tenant_stale_version_and_invalid_action_do_not_stage_changes()
    {
        var actor = IdentityAccountId.New();
        var school = CreateDraftSchool((Guid)actor);
        var outbox = new FakeOutboxWriter();
        var audit = new FakeRegistryAuditWriter();
        var unitOfWork = new FakePlatformUnitOfWork();
        var handler = CreateHandler(school, outbox, audit, unitOfWork);

        await Assert.ThrowsAsync<PlatformResourceNotFoundException>(() => handler.HandleAsync(
            Command(school, actor, SchoolLifecycleAction.SubmitForVerification) with
            { TenantId = TenantId.New() }));
        await Assert.ThrowsAsync<PlatformConflictException>(() => handler.HandleAsync(
            Command(school, actor, SchoolLifecycleAction.SubmitForVerification) with
            { ExpectedVersion = school.Version + 1 }));
        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(
            Command(school, actor, (SchoolLifecycleAction)99)));
        await Assert.ThrowsAsync<PlatformDomainException>(() => handler.HandleAsync(
            Command(school, actor, SchoolLifecycleAction.Reinstate)));

        Assert.Equal(SchoolLifecycleStatus.Draft, school.Status);
        Assert.Empty(outbox.Messages);
        Assert.Empty(audit.Records);
        Assert.Equal(0, unitOfWork.SaveCount);
    }

    [Fact]
    public async Task Failed_save_keeps_domain_event_for_transaction_retry_or_disposal()
    {
        var actor = IdentityAccountId.New();
        var school = CreateDraftSchool((Guid)actor);
        var outbox = new FakeOutboxWriter();
        var audit = new FakeRegistryAuditWriter();
        var unitOfWork = new FakePlatformUnitOfWork(new InvalidOperationException("database unavailable"));
        var handler = CreateHandler(school, outbox, audit, unitOfWork);

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(
            Command(school, actor, SchoolLifecycleAction.SubmitForVerification)));

        Assert.Single(school.DomainEvents);
        Assert.Single(outbox.Messages);
        Assert.Single(audit.Records);
    }

    private static TransitionSchoolCommandHandler CreateHandler(
        SchoolRegistration school,
        FakeOutboxWriter outbox,
        FakeRegistryAuditWriter audit,
        FakePlatformUnitOfWork unitOfWork) => new(
            new FakeSchoolRegistrationRepository(school),
            outbox,
            audit,
            unitOfWork,
            new FakeClock(Now));

    private static TransitionSchoolCommand Command(
        SchoolRegistration school,
        IdentityAccountId actor,
        SchoolLifecycleAction action,
        string? reason = null) => new(
            school.TenantId,
            school.Id,
            action,
            actor,
            school.Version,
            Guid.NewGuid(),
            reason);

    private static SchoolRegistration CreateDraftSchool(Guid actorId)
    {
        var tenantId = TenantId.New();
        var school = SchoolRegistration.Create(
            SchoolId.From(tenantId.Value),
            tenantId,
            Guid.NewGuid(),
            SchoolCode.Create("SCHOOL-TST"),
            "Test School",
            SchoolType.Private,
            DeploymentMode.SharedSaaS,
            actorId,
            Now.AddMinutes(-10));
        school.DequeueDomainEvents();
        return school;
    }

    private static SchoolRegistration CreateActiveSchool(Guid actorId)
    {
        var school = CreateDraftSchool(actorId);
        school.SubmitForVerification(actorId, Now.AddMinutes(-9));
        school.Approve(actorId, Now.AddMinutes(-8));
        var operationId = Guid.NewGuid();
        school.BeginProvisioning(operationId, actorId, Now.AddMinutes(-7));
        school.Activate(operationId, actorId, Now.AddMinutes(-6));
        school.DequeueDomainEvents();
        return school;
    }

    private sealed class FakeRegistryAuditWriter : IPlatformRegistryAuditWriter
    {
        public List<RegistryAuditRecord> Records { get; } = [];

        public void Stage(RegistryAuditRecord record) => Records.Add(record);
    }
}
