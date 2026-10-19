using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.Schools.Domain.School;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Mdaresna.Schools.UnitTests;

public sealed class SchoolInvoiceCopyRelationalTests
{
    [CapacityDatabaseTheory]
    [InlineData("PostgreSql")]
    [InlineData("SqlServer")]
    public async Task Migration_and_locked_upsert_preserve_latest_invoice_copy(string provider)
    {
        var database = "mdaresna_invoice_copy_qa_" + Guid.NewGuid().ToString("N");
        var source = Environment.GetEnvironmentVariable(provider == "PostgreSql" ? "MDARESNA_SUBSCRIPTIONS_PG_QA" : "MDARESNA_SUBSCRIPTIONS_SQL_QA")!;
        var connection = provider == "PostgreSql" ? new Npgsql.NpgsqlConnectionStringBuilder(source) { Database = database, Pooling = false }.ConnectionString
            : new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(source) { InitialCatalog = database, Pooling = false }.ConnectionString;
        SchoolsDbContext Create() => provider == "PostgreSql"
            ? new PostgreSqlSchoolsDbContext(new DbContextOptionsBuilder<PostgreSqlSchoolsDbContext>().UseNpgsql(connection).Options)
            : new SqlServerSchoolsDbContext(new DbContextOptionsBuilder<SqlServerSchoolsDbContext>().UseSqlServer(connection).Options);
        await using var db = Create();
        try
        {
            await db.Database.MigrateAsync();
            var id = Guid.NewGuid(); var now = DateTimeOffset.UtcNow;
            db.SchoolInformation.Add(SchoolInformation.CreateWithoutUnits(id, id, Guid.NewGuid(), "QA-SCHOOL", "QA", "Private", "SharedSaaS", null, null, Guid.NewGuid(), now, now));
            await db.SaveChangesAsync();
            var invoice = new SchoolInvoiceSnapshotChangedV1(id, "QA-SCHOOL", Guid.NewGuid(), "INV-QA", 1, 30000, "EGP", "Unpaid", now, null,
                [new(Guid.NewGuid(), Guid.NewGuid(), "أكاديمي", "Academic", "باقة", "Plan", 300, 30000, "New")], []);
            await SchoolInvoiceSync.ApplyAsync(db, invoice, default);
            await using (var update = Create()) await SchoolInvoiceSync.ApplyAsync(update,
                invoice with { Revision = 3, PaymentStatus = "Paid", PaidAtUtc = now }, default);
            await using (var replay = Create()) await SchoolInvoiceSync.ApplyAsync(replay, invoice, default);
            await using var verify = Create();
            var row = await verify.InvoiceCopies.SingleAsync();
            Assert.Equal(3, row.Revision);
            Assert.Contains("Paid", row.PayloadJson);
        }
        finally { await db.Database.EnsureDeletedAsync(); }
    }
}
