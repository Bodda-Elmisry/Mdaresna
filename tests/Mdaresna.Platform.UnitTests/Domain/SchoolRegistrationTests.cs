using Mdaresna.Platform.Domain.Common;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Domain;

public sealed class SchoolRegistrationTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 12, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Private_school_cannot_use_government_on_premises_deployment()
    {
        var tenantId = TenantId.New();
        var exception = Assert.Throws<PlatformDomainException>(() => SchoolRegistration.Create(
            SchoolId.From(tenantId.Value),
            tenantId,
            Guid.NewGuid(),
            SchoolCode.Create("PRIVATE-01"),
            "Private School",
            SchoolType.Private,
            DeploymentMode.GovernmentOnPremises,
            Guid.NewGuid(),
            Now));

        Assert.Equal("school.invalid_deployment_mode", exception.Code);
    }

    [Fact]
    public void School_id_must_equal_tenant_id()
    {
        var exception = Assert.Throws<PlatformDomainException>(() => SchoolRegistration.Create(
            SchoolId.New(),
            TenantId.New(),
            Guid.NewGuid(),
            SchoolCode.Create("OTHER-001"),
            "Other School",
            SchoolType.Private,
            DeploymentMode.SharedSaaS,
            Guid.NewGuid(),
            Now));

        Assert.Equal("school.tenant_id_mismatch", exception.Code);
    }

    [Fact]
    public void Persisted_school_cannot_rehydrate_with_a_different_tenant_id()
    {
        var exception = Assert.Throws<PlatformDomainException>(() => SchoolRegistration.Rehydrate(
            SchoolId.New(),
            TenantId.New(),
            Guid.NewGuid(),
            "OTHER-002",
            "Other School",
            SchoolType.Private,
            DeploymentMode.SharedSaaS,
            SchoolLifecycleStatus.Draft,
            Guid.NewGuid(),
            null,
            null,
            Now,
            Now,
            0));

        Assert.Equal("school.tenant_id_mismatch", exception.Code);
    }

    [Fact]
    public void Government_school_can_complete_the_provisioning_lifecycle()
    {
        var actorId = Guid.NewGuid();
        var operationId = Guid.NewGuid();
        var school = CreateGovernmentSchool(actorId);

        school.SubmitForVerification(actorId, Now.AddMinutes(1));
        school.Approve(Guid.NewGuid(), actorId, Now.AddMinutes(2));
        school.BeginProvisioning(operationId, actorId, Now.AddMinutes(3));
        school.Activate(operationId, actorId, Now.AddMinutes(4));

        Assert.Equal(SchoolLifecycleStatus.Active, school.Status);
        Assert.Equal(operationId, school.ProvisioningOperationId);
        Assert.Equal(Now.AddMinutes(4), school.ActivatedAtUtc);
        Assert.Null(school.StatusReason);
        Assert.Equal(5, school.Version);
        Assert.Equal(5, school.DomainEvents.Count);
    }

    [Fact]
    public void School_cannot_skip_required_lifecycle_states()
    {
        var actorId = Guid.NewGuid();
        var school = CreateGovernmentSchool(actorId);

        var exception = Assert.Throws<PlatformDomainException>(() =>
            school.Approve(Guid.NewGuid(), actorId, Now.AddMinutes(1)));

        Assert.Equal("school.invalid_status_transition", exception.Code);
        Assert.Equal(SchoolLifecycleStatus.Draft, school.Status);
    }

    [Fact]
    public void Non_event_changes_still_advance_the_aggregate_version()
    {
        var actorId = Guid.NewGuid();
        var school = CreateGovernmentSchool(actorId);
        var originalVersion = school.Version;
        var originalEventCount = school.DomainEvents.Count;

        school.Rename("Renamed Government School", Now.AddMinutes(1));
        school.ChangeDeploymentMode(DeploymentMode.DedicatedCloud, Now.AddMinutes(2));

        Assert.Equal(originalVersion + 2, school.Version);
        Assert.Equal(originalEventCount, school.DomainEvents.Count);
    }

    [Fact]
    public void Provisioning_result_must_match_the_current_operation()
    {
        var actorId = Guid.NewGuid();
        var school = CreateGovernmentSchool(actorId);
        school.SubmitForVerification(actorId, Now.AddMinutes(1));
        school.Approve(Guid.NewGuid(), actorId, Now.AddMinutes(2));
        school.BeginProvisioning(Guid.NewGuid(), actorId, Now.AddMinutes(3));

        var exception = Assert.Throws<PlatformDomainException>(() =>
            school.Activate(Guid.NewGuid(), actorId, Now.AddMinutes(4)));

        Assert.Equal("school.provisioning_operation_mismatch", exception.Code);
        Assert.Equal(SchoolLifecycleStatus.Provisioning, school.Status);
    }

    private static SchoolRegistration CreateGovernmentSchool(Guid actorId)
    {
        var tenantId = TenantId.New();
        return SchoolRegistration.Create(
            SchoolId.From(tenantId.Value),
            tenantId,
            Guid.NewGuid(),
            SchoolCode.Create("GOV-001"),
            "Government School",
            SchoolType.Government,
            DeploymentMode.GovernmentOnPremises,
            actorId,
            Now);
    }
}
