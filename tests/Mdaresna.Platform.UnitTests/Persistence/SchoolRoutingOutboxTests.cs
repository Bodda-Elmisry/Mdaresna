using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.UnitTests.Persistence;

public sealed class SchoolRoutingOutboxTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;
    private static SchoolRegistration CreateSchool()
    {
        var id = Guid.NewGuid(); var actor = Guid.NewGuid(); var operation = Guid.NewGuid();
        var school = SchoolRegistration.Create(SchoolId.From(id), TenantId.From(id), Guid.NewGuid(), SchoolCode.Create("ABC"),
            "Fixture", SchoolType.Private, DeploymentMode.SharedSaaS, actor, Now);
        school.SubmitForVerification(actor, Now);
        school.Approve(Guid.NewGuid(), actor, Now);
        school.BeginProvisioning(operation, actor, Now);
        school.Activate(operation, actor, Now);
        return school;
    }
    private static SchoolDatabaseEndpoint Endpoint(SchoolRegistration school, string host = "first.internal", bool primary = true)
    {
        var endpoint = SchoolDatabaseEndpoint.Create(Guid.NewGuid(), school.Id, SchoolDatabasePurpose.Operational,
            SchoolDatabaseProvider.PostgreSql, host, 5432, "fixture_school", "fixture-secret", true, primary, null, null, Now);
        endpoint.ChangeStatus(SchoolDatabaseEndpointStatus.Active, Now);
        return endpoint;
    }
    private static PlatformDbContext Context(string name) => new(new DbContextOptionsBuilder<PlatformDbContext>().UseInMemoryDatabase(name).Options);
    private static void InitializeRowVersions(PlatformDbContext db)
    {
        foreach (var entry in db.ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
            if (entry.Metadata.FindProperty("RowVersion") is not null) entry.Property("RowVersion").CurrentValue = new byte[8];
    }

    [Fact]
    public async Task SchoolAndEndpointChangesStageCompleteVersionedSnapshotsInSameSave()
    {
        await using var db = Context(Guid.NewGuid().ToString());
        var school = CreateSchool(); var endpoint = Endpoint(school);
        db.Schools.Add(school); db.SchoolDatabaseEndpoints.Add(endpoint);
        InitializeRowVersions(db);
        await db.SaveChangesAsync();
        Assert.Equal(1, school.RoutingVersion);
        var first = IntegrationJsonSerializer.Deserialize<SchoolRoutingChangedV1>(Assert.Single(db.OutboxMessages).PayloadJson).Data;
        Assert.Equal("first.internal", first.Target!.Host);
        endpoint.UpdateTarget("second.internal", 5432, "fixture_school", "new-secret", true, null, null, Now.AddSeconds(1));
        await db.SaveChangesAsync();
        var latest = db.OutboxMessages.AsEnumerable().Select(m => IntegrationJsonSerializer.Deserialize<SchoolRoutingChangedV1>(m.PayloadJson).Data)
            .OrderBy(m => m.RoutingVersion).Last();
        Assert.Equal(2, latest.RoutingVersion); Assert.Equal("second.internal", latest.Target!.Host);
        Assert.Equal("new-secret", latest.Target.CredentialSecretReference);
        Assert.DoesNotContain("Password", db.OutboxMessages.AsEnumerable().Last().PayloadJson);
        school.Suspend("fixture", Guid.NewGuid(), Now.AddSeconds(2));
        await db.SaveChangesAsync();
        Assert.Equal(3, school.RoutingVersion);
        Assert.Null(db.OutboxMessages.AsEnumerable().Select(m => IntegrationJsonSerializer.Deserialize<SchoolRoutingChangedV1>(m.PayloadJson).Data)
            .Single(m => m.RoutingVersion == 3).Target);
    }

    [Fact]
    public async Task PrimaryReplacementUsesSchoolRevisionAndUnchangedSaveDoesNotPublish()
    {
        await using var db = Context(Guid.NewGuid().ToString());
        var school = CreateSchool(); var first = Endpoint(school); var next = Endpoint(school, "next.internal", false);
        db.Add(school); db.Add(first); db.Add(next); InitializeRowVersions(db); await db.SaveChangesAsync();
        first.SetPrimary(false, Now.AddSeconds(1)); next.SetPrimary(true, Now.AddSeconds(1));
        await db.SaveChangesAsync();
        Assert.Equal(2, school.RoutingVersion);
        var latest = db.OutboxMessages.AsEnumerable().Select(m => IntegrationJsonSerializer.Deserialize<SchoolRoutingChangedV1>(m.PayloadJson).Data)
            .Single(m => m.RoutingVersion == 2);
        Assert.Equal("next.internal", latest.Target!.Host);
        await db.SaveChangesAsync(); Assert.Equal(2, db.OutboxMessages.Count());
        var property = db.Model.FindEntityType(typeof(SchoolRegistration))!.FindProperty(nameof(SchoolRegistration.RoutingVersion))!;
        Assert.True(property.IsConcurrencyToken);
    }

    [Fact]
    public async Task PhysicalDeletionStagesTombstone()
    {
        await using var db = Context(Guid.NewGuid().ToString());
        var school = CreateSchool(); db.Add(school); InitializeRowVersions(db); await db.SaveChangesAsync();
        db.Remove(school); await db.SaveChangesAsync();
        Assert.Empty(db.Schools);
        var latest = db.OutboxMessages.AsEnumerable().Select(m => IntegrationJsonSerializer.Deserialize<SchoolRoutingChangedV1>(m.PayloadJson).Data)
            .OrderBy(x => x.RoutingVersion).Last();
        Assert.Null(latest.Target); Assert.Equal(2, latest.RoutingVersion);
    }

    [Fact]
    public async Task ConcurrentRoutingWritesCannotPublishTheSameSchoolRevision()
    {
        var name = Guid.NewGuid().ToString();
        await using (var seed = Context(name))
        {
            var school = CreateSchool(); seed.Add(school); seed.Add(Endpoint(school)); InitializeRowVersions(seed);
            await seed.SaveChangesAsync();
        }
        await using var left = Context(name); await using var right = Context(name);
        await left.Schools.LoadAsync(); await right.Schools.LoadAsync();
        var a = await left.SchoolDatabaseEndpoints.SingleAsync(); var b = await right.SchoolDatabaseEndpoints.SingleAsync();
        a.UpdateTarget("left.internal", 5432, "fixture_school", "fixture-secret", true, null, null, Now.AddSeconds(1));
        b.UpdateTarget("right.internal", 5432, "fixture_school", "fixture-secret", true, null, null, Now.AddSeconds(1));
        await left.SaveChangesAsync();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => right.SaveChangesAsync());
    }
}
