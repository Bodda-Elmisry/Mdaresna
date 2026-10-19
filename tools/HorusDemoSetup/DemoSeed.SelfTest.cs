using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.School;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

internal static partial class DemoSeed
{
    public static async Task SelfTestAsync(SchoolsDbContext source)
    {
        var target = new Npgsql.NpgsqlConnectionStringBuilder(source.Database.GetConnectionString())
            { Database = "horus_demo_qa_" + Guid.NewGuid().ToString("N"), Pooling = false };
        await using var test = new PostgreSqlSchoolsDbContext(new DbContextOptionsBuilder<PostgreSqlSchoolsDbContext>().UseNpgsql(target.ConnectionString).Options);
        try
        {
            await test.Database.MigrateAsync();
            var school = Guid.NewGuid(); var owner = Guid.NewGuid(); var person = new Person { Id = Guid.NewGuid(), DisplayName = "QA owner", CreatedAtUtc = Now, UpdatedAtUtc = Now };
            test.Add(person); test.Add(new LocalUserAccount { Id = owner, Person = person, UserName = "owner", NormalizedUserName = "OWNER", Status = LocalUserStatus.Active, CreatedAtUtc = Now, UpdatedAtUtc = Now });
            test.Add(SchoolInformation.CreateWithoutUnits(school, school, Guid.NewGuid(), "SCH-1A769BC8A279", "حورس QA", "Private", "SharedSaaS", null, null, Guid.NewGuid(), Now, Now));
            test.Subscriptions.Add(new SchoolSubscriptionState { SystemId = Guid.NewGuid(), SubscriptionId = Guid.NewGuid(), SystemCode = "ACADEMIC", CapacityMetric = "Student", Revision = 1, LocalVersion = 1, BaseCapacity = 200, IsEnforced = true, CapabilityCodes = "academic.students", ValidFromUtc = Now.AddDays(-1), ValidUntilUtc = Now.AddYears(1) });
            await test.SaveChangesAsync();
            await using var tx = await test.Database.BeginTransactionAsync();
            await BuildAsync(test, new FakeGlobal(), new FakeIdentities(), owner, Convert.ToHexString(System.Security.Cryptography.RandomNumberGenerator.GetBytes(24)));
            await CompleteConfigurationAsync(test, owner);
            await test.SaveChangesAsync(); await VerifyAsync(test, false); await tx.CommitAsync();
            Console.WriteLine("Isolated PostgreSQL setup self-test passed; no external gateway was used.");
        }
        finally
        {
            if (!target.Database!.StartsWith("horus_demo_qa_", StringComparison.Ordinal)) throw new InvalidOperationException("Unexpected QA database name.");
            await test.Database.EnsureDeletedAsync();
        }
    }
    private sealed class FakeGlobal : IGlobalStudentGateway
    {
        public Task<GlobalStudentIdentity?> ResolveOrCreateAsync(Guid school, string? code, string name, DateOnly birth, string? nationalId, string? certificate, CancellationToken ct = default)
            => Task.FromResult<GlobalStudentIdentity?>(new(Id(certificate!), "STU-" + Id(certificate!).ToString("N")[..12].ToUpperInvariant()));
    }
    private sealed class FakeIdentities : ISchoolUserIdentityGateway
    {
        public Task<SchoolUserIdentity?> ResolveAsync(Guid school, string phone, string name, CancellationToken ct = default)
            => Task.FromResult<SchoolUserIdentity?>(new(Id(phone), name, phone, false));
        public Task<bool> SendInvitationAsync(Guid school, Guid account, string login, CancellationToken ct = default) => throw new InvalidOperationException("Demo setup must never send invitations.");
        public Task<SchoolUserPrimaryContact?> GetPrimaryContactAsync(Guid school, Guid account, CancellationToken ct = default) => throw new InvalidOperationException("Not used by setup.");
    }
}
