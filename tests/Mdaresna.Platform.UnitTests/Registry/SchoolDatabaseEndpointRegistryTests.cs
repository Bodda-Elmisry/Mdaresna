using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Application.Registry.DatabaseEndpoints;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.UnitTests.TestDoubles;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Registry;

public sealed class SchoolDatabaseEndpointRegistryTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 15, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task First_endpoint_for_a_purpose_becomes_primary()
    {
        var school = CreateSchool();
        var endpoints = new FakeSchoolDatabaseEndpointRepository();
        var audit = new FakeRegistryAuditWriter();
        var unitOfWork = new FakePlatformUnitOfWork();
        var registry = new SchoolDatabaseEndpointRegistry(
            endpoints,
            new FakeSchoolRegistrationRepository(school),
            audit,
            unitOfWork,
            new FakeClock(Now));

        var result = await registry.RegisterAsync(new RegisterSchoolDatabaseEndpointCommand(
            Guid.NewGuid(),
            school.Id,
            SchoolDatabasePurpose.Operational,
            SchoolDatabaseProvider.PostgreSql,
            "school-db.internal",
            5432,
            "school_one",
            "secret://schools/one/database",
            RequireTls: true,
            IsPrimary: false,
            "egypt-north",
            "1.0.0",
            IdentityAccountId.New(),
            Guid.NewGuid()));

        Assert.True(result.IsPrimary);
        Assert.Equal(SchoolDatabaseEndpointStatus.Provisioning, result.Status);
        Assert.Equal(1, unitOfWork.SaveCount);
        Assert.Single(audit.Records);
        Assert.DoesNotContain("secret://", audit.Records[0].MetadataJson);
    }

    [Fact]
    public async Task Resolver_returns_only_an_active_primary_endpoint()
    {
        var schoolId = SchoolId.New();
        var endpoint = CreateEndpoint(schoolId, isPrimary: true);
        var repository = new FakeSchoolDatabaseEndpointRepository(endpoint);
        var resolver = new SchoolDatabaseEndpointResolver(repository);

        await Assert.ThrowsAsync<PlatformResourceNotFoundException>(() =>
            resolver.ResolvePrimaryAsync(schoolId));

        endpoint.ChangeStatus(SchoolDatabaseEndpointStatus.Active, Now.AddMinutes(1));
        var target = await resolver.ResolvePrimaryAsync(schoolId);

        Assert.Equal(endpoint.Id, target.EndpointId);
        Assert.Equal("secret://schools/one/database", target.CredentialSecretReference);
    }

    [Fact]
    public async Task Changing_primary_demotes_the_previous_endpoint_in_one_transaction()
    {
        var school = CreateSchool();
        var previous = CreateEndpoint(school.Id, isPrimary: true, databaseName: "school_one");
        var replacement = CreateEndpoint(school.Id, isPrimary: false, databaseName: "school_two");
        previous.ChangeStatus(SchoolDatabaseEndpointStatus.Active, Now.AddMinutes(1));
        replacement.ChangeStatus(SchoolDatabaseEndpointStatus.Active, Now.AddMinutes(1));
        var endpoints = new FakeSchoolDatabaseEndpointRepository(previous, replacement);
        var unitOfWork = new FakePlatformUnitOfWork();
        var registry = new SchoolDatabaseEndpointRegistry(
            endpoints,
            new FakeSchoolRegistrationRepository(school),
            new FakeRegistryAuditWriter(),
            unitOfWork,
            new FakeClock(Now.AddMinutes(2)));

        await registry.MakePrimaryAsync(new MakeSchoolDatabaseEndpointPrimaryCommand(
            replacement.Id,
            school.Id,
            replacement.Version,
            IdentityAccountId.New(),
            Guid.NewGuid()));

        Assert.False(previous.IsPrimary);
        Assert.True(replacement.IsPrimary);
        Assert.Equal(2, unitOfWork.SaveCount);
    }

    private static SchoolRegistration CreateSchool()
    {
        var tenantId = TenantId.New();
        return SchoolRegistration.Create(
            SchoolId.From(tenantId.Value),
            tenantId,
            Guid.NewGuid(),
            SchoolCode.Create("SCHOOL-DB-1"),
            "School One",
            SchoolType.Private,
            DeploymentMode.DedicatedCloud,
            Guid.NewGuid(),
            Now);
    }

    private static SchoolDatabaseEndpoint CreateEndpoint(
        SchoolId schoolId,
        bool isPrimary,
        string databaseName = "school_one") => SchoolDatabaseEndpoint.Create(
            Guid.NewGuid(),
            schoolId,
            SchoolDatabasePurpose.Operational,
            SchoolDatabaseProvider.PostgreSql,
            "school-db.internal",
            5432,
            databaseName,
            "secret://schools/one/database",
            requireTls: true,
            isPrimary,
            "egypt-north",
            "1.0.0",
            Now);
}
