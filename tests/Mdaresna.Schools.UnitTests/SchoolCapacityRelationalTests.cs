using Mdaresna.Schools.Domain.School;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Mdaresna.Schools.UnitTests;

public sealed class SchoolCapacityRelationalTests
{
    [CapacityDatabaseTheory]
    [InlineData("PostgreSql")]
    [InlineData("SqlServer")]
    public async Task Last_seat_is_atomic_across_connections_and_rollback_returns_the_seat(string provider)
    {
        var database = "mdaresna_subscriptions_" + Guid.NewGuid().ToString("N");
        var source = Environment.GetEnvironmentVariable(provider == "PostgreSql" ? "MDARESNA_SUBSCRIPTIONS_PG_QA" : "MDARESNA_SUBSCRIPTIONS_SQL_QA")!;
        string connection;
        if (provider == "PostgreSql") { var b = new Npgsql.NpgsqlConnectionStringBuilder(source) { Database = database, Pooling = false }; connection = b.ConnectionString; }
        else { var b = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(source) { InitialCatalog = database, Pooling = false }; connection = b.ConnectionString; }
        SchoolsDbContext Create() => provider == "PostgreSql"
            ? new PostgreSqlSchoolsDbContext(new DbContextOptionsBuilder<PostgreSqlSchoolsDbContext>().UseNpgsql(connection).Options)
            : new SqlServerSchoolsDbContext(new DbContextOptionsBuilder<SqlServerSchoolsDbContext>().UseSqlServer(connection).Options);
        await using var owner = Create();
        var systemId = Guid.NewGuid();
        try
        {
            await owner.Database.MigrateAsync();
            var schoolId = Guid.NewGuid();
            var now = DateTimeOffset.UtcNow;
            owner.SchoolInformation.Add(SchoolInformation.CreateWithoutUnits(schoolId, schoolId, Guid.NewGuid(), "QA-SCHOOL", "QA School", "Private", "SharedSaaS", null, null, Guid.NewGuid(), now, now));
            owner.Subscriptions.Add(new SchoolSubscriptionState { SystemId = systemId, SubscriptionId = Guid.NewGuid(), SystemCode = "ACADEMIC",
                CapacityMetric = "Student", Revision = 1, LocalVersion = 1, IsEnforced = true, BaseCapacity = 1,
                CapabilityCodes = "academic.students", ValidFromUtc = DateTimeOffset.UtcNow.AddDays(-1), ValidUntilUtc = DateTimeOffset.UtcNow.AddYears(1) });
            await owner.SaveChangesAsync();
            var start = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var results = Enumerable.Range(0, 8).Select(async _ => {
                await using var db = Create(); await start.Task;
                await using var tx = await db.Database.BeginTransactionAsync();
                try
                {
                    await SchoolCapacityService.ChangeAsync(db, "Student", Guid.NewGuid(), true, Guid.NewGuid(), true, default);
                    await db.SaveChangesAsync(); await tx.CommitAsync(); return true;
                }
                catch (SchoolCapacityException error) { Assert.Equal("subscription.capacity_exhausted", error.Code); return false; }
            }).ToArray();
            start.SetResult(); var applied = await Task.WhenAll(results); Assert.Single(applied, x => x);
            await using (var verify = Create())
            {
                Assert.Equal(1, (await verify.Subscriptions.SingleAsync()).UsedCapacity);
                Assert.Equal(1, await verify.CapacityAllocations.CountAsync(x => x.IsActive));
                Assert.Equal(1, await verify.CapacityMovements.CountAsync());
            }
            await using (var releasing = Create())
            {
                await using var tx = await releasing.Database.BeginTransactionAsync();
                var id = await releasing.CapacityAllocations.Select(x => x.SubjectId).SingleAsync();
                await SchoolCapacityService.ChangeAsync(releasing, "Student", id, false, Guid.NewGuid(), true, default);
                await releasing.SaveChangesAsync();
                await tx.RollbackAsync();
            }
            await using var final = Create(); Assert.Equal(1, (await final.Subscriptions.SingleAsync()).UsedCapacity);
            var before = await final.Subscriptions.SingleAsync();
            var snapshot = new SchoolSubscriptionSnapshotChangedV1(schoolId, "QA-SCHOOL", before.SubscriptionId, systemId, "ACADEMIC", "Student", 3,
                before.PlanId == Guid.Empty ? Guid.NewGuid() : before.PlanId, 1, 3, 0, false, before.ValidFromUtc, before.ValidUntilUtc, ["academic.students"]);
            await SchoolSubscriptionSync.ApplyAsync(final, snapshot, default);
            await SchoolSubscriptionSync.ApplyAsync(final, snapshot, default);
            await SchoolSubscriptionSync.ApplyAsync(final, snapshot with { Revision = 2, BaseCapacity = 2 }, default);
            Assert.Equal(3, before.Revision); Assert.Equal(3, before.BaseCapacity); Assert.Equal(1, before.UsedCapacity); Assert.True(before.IsEnforced);
            Assert.Equal(1, await final.CapacityMovements.CountAsync(x => x.Action == "SnapshotApplied"));
            await Assert.ThrowsAsync<ArgumentException>(() => SchoolSubscriptionSync.ApplyAsync(final, snapshot with { SchoolId = Guid.NewGuid(), Revision = 4 }, default));
        }
        finally
        {
            // Only the random database created by this test is ever dropped.
            if (!database.StartsWith("mdaresna_subscriptions_", StringComparison.Ordinal)) throw new InvalidOperationException("Invalid fixture database.");
            await owner.Database.EnsureDeletedAsync();
        }
    }
}

public sealed class CapacityDatabaseTheoryAttribute : TheoryAttribute
{
    public CapacityDatabaseTheoryAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MDARESNA_SUBSCRIPTIONS_PG_QA")) ||
            string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MDARESNA_SUBSCRIPTIONS_SQL_QA")))
            Skip = "Requires isolated PostgreSQL and SQL Server QA endpoints; never defaults to an application database.";
    }
}
