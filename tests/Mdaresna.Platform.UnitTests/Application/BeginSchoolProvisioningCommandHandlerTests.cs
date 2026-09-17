using Mdaresna.Platform.Application.Registry.BeginSchoolProvisioning;
using ProvisionSchoolCommand = Mdaresna.Schools.Contracts.Provisioning.ProvisionSchoolV1;
using Mdaresna.Platform.Contracts.Registry;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Domain.Billing.Units;
using Mdaresna.Platform.UnitTests.TestDoubles;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Application;

public sealed class BeginSchoolProvisioningCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 12, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Replaying_the_same_provisioning_operation_does_not_enqueue_twice()
    {
        var actorId = IdentityAccountId.New();
        var tenantId = TenantId.New();
        var school = CreateApprovedSchool(tenantId, (Guid)actorId);
        var schools = new FakeSchoolRegistrationRepository(school);
        var outbox = new FakeOutboxWriter();
        var audit = new FakeRegistryAuditWriter();
        var unitOfWork = new FakePlatformUnitOfWork();
        var handler = new BeginSchoolProvisioningCommandHandler(
            schools,
            new FakeUnitTypeRepository(UnitType.Create(school.UnitTypeId!.Value, "BASE", "Base unit", 1m, "EGP", Now.AddDays(-1))),
            outbox,
            audit,
            unitOfWork,
            new FakeClock(Now));
        var operationId = Guid.NewGuid();
        var command = new BeginSchoolProvisioningCommand(
            tenantId,
            school.Id,
            operationId,
            actorId,
            Guid.NewGuid());

        await handler.HandleAsync(command);
        await handler.HandleAsync(command);

        Assert.Equal(SchoolLifecycleStatus.Provisioning, school.Status);
        Assert.Equal(operationId, school.ProvisioningOperationId);
        Assert.Equal(2, outbox.Messages.Count);
        Assert.Single(audit.Records);
        Assert.Equal(actorId, audit.Records[0].ActorId);
        Assert.Equal(command.CorrelationId, audit.Records[0].CorrelationId);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Empty(school.DomainEvents);

        var envelope = outbox.Single<ProvisionSchoolCommand>();
        Assert.Equal(operationId, envelope.Data.OperationId);
        Assert.Equal(school.Id, envelope.Scope.SchoolId);

        var lifecycleEnvelope = outbox.Single<SchoolLifecycleChangedV1>();
        Assert.Equal(SchoolLifecycleStatusV1.Approved, lifecycleEnvelope.Data.PreviousStatus);
        Assert.Equal(SchoolLifecycleStatusV1.Provisioning, lifecycleEnvelope.Data.CurrentStatus);
        Assert.Equal(operationId, lifecycleEnvelope.Data.ProvisioningOperationId);
        Assert.Equal(command.CorrelationId, lifecycleEnvelope.CorrelationId);
        Assert.Equal(school.Id, lifecycleEnvelope.Scope.SchoolId);
    }

    private static SchoolRegistration CreateApprovedSchool(TenantId tenantId, Guid actorId)
    {
        var school = SchoolRegistration.Create(
            SchoolId.From(tenantId.Value),
            tenantId,
            Guid.NewGuid(),
            SchoolCode.Create("SCHOOL-002"),
            "School Two",
            SchoolType.Private,
            DeploymentMode.DedicatedCloud,
            actorId,
            Now.AddMinutes(-3));
        school.SubmitForVerification(actorId, Now.AddMinutes(-2));
        school.Approve(Guid.NewGuid(), actorId, Now.AddMinutes(-1));
        return school;
    }
}
