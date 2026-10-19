using System.Text.Json;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.Schools.Domain.School;
using Mdaresna.Schools.Domain.Students;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Mdaresna.Schools.UnitTests;

public sealed class SchoolInvoiceSyncTests
{
    private static SchoolsDbContext Db() => new(new DbContextOptionsBuilder<SchoolsDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static async Task<Guid> School(SchoolsDbContext db)
    {
        var id = Guid.NewGuid(); var now = DateTimeOffset.UtcNow;
        db.SchoolInformation.Add(SchoolInformation.CreateWithoutUnits(id, id, Guid.NewGuid(), "QA-SCHOOL", "QA", "Private", "SharedSaaS", null, null, Guid.NewGuid(), now, now));
        await db.SaveChangesAsync(); return id;
    }
    private static SchoolInvoiceSnapshotChangedV1 Snapshot(Guid school) => new(school, "QA-SCHOOL", Guid.NewGuid(), "INV-QA", 1,
        30000, "EGP", "Unpaid", DateTimeOffset.UtcNow, null,
        [new(Guid.NewGuid(), Guid.NewGuid(), "أكاديمي", "Academic", "انطلاق", "Launch", 300, 30000, "New")], []);

    [Fact]
    public async Task Invoice_copy_is_idempotent_updates_payment_and_rejects_other_school_and_modified_terms()
    {
        await using var db = Db(); var school = await School(db); var invoice = Snapshot(school);
        await SchoolInvoiceSync.ApplyAsync(db, invoice, default);
        var paid = invoice with { Revision = 3, PaymentStatus = "Paid", PaidAtUtc = DateTimeOffset.UtcNow,
            Receipts = [new(Guid.NewGuid(), "BankTransfer", "QA-REF", null, DateTimeOffset.UtcNow, "Approved")] };
        await SchoolInvoiceSync.ApplyAsync(db, paid, default);
        await SchoolInvoiceSync.ApplyAsync(db, invoice, default);
        await SchoolInvoiceSync.ApplyAsync(db, paid, default);
        var copy = Assert.Single(db.InvoiceCopies);
        Assert.Equal(3, copy.Revision);
        Assert.Equal("Paid", JsonSerializer.Deserialize<SchoolInvoiceSnapshotChangedV1>(copy.PayloadJson)!.PaymentStatus);
        await Assert.ThrowsAsync<ArgumentException>(() => SchoolInvoiceSync.ApplyAsync(db, paid with { SchoolId = Guid.NewGuid(), Revision = 4 }, default));
        await Assert.ThrowsAsync<ArgumentException>(() => SchoolInvoiceSync.ApplyAsync(db, paid with { Number = "CHANGED", Revision = 4 }, default));
        Assert.Equal(3, copy.Revision);
    }

    [Fact]
    public async Task Invalid_totals_dates_and_payment_states_are_rejected_before_writing()
    {
        await using var db = Db(); var invoice = Snapshot(await School(db));
        await Assert.ThrowsAsync<ArgumentException>(() => SchoolInvoiceSync.ApplyAsync(db, invoice with { Amount = 1 }, default));
        await Assert.ThrowsAsync<ArgumentException>(() => SchoolInvoiceSync.ApplyAsync(db, invoice with { PaymentStatus = "Paid" }, default));
        await Assert.ThrowsAsync<ArgumentException>(() => SchoolInvoiceSync.ApplyAsync(db, invoice with { Currency = "bad" }, default));
        Assert.Empty(db.InvoiceCopies);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Only_empty_student_database_gets_automatic_baseline_and_replay_does_not_reset_usage(bool hasStudent)
    {
        await using var db = Db(); var school = await School(db);
        if (hasStudent) { db.Students.Add(new Student { Id = Guid.NewGuid(), StudentCode = "QA-1" }); await db.SaveChangesAsync(); }
        var now = DateTimeOffset.UtcNow;
        var snapshot = new SchoolSubscriptionSnapshotChangedV1(school, "QA-SCHOOL", Guid.NewGuid(), Guid.NewGuid(), "ACADEMIC", "Student", 1,
            Guid.NewGuid(), 1, 300, 0, false, now.AddDays(-1), now.AddYears(1), ["academic.students"]);
        await SchoolSubscriptionSync.ApplyAsync(db, snapshot, default);
        var state = await db.Subscriptions.SingleAsync();
        Assert.Equal(!hasStudent, state.IsEnforced);
        if (!hasStudent) { state.UsedCapacity = 5; await db.SaveChangesAsync(); }
        await SchoolSubscriptionSync.ApplyAsync(db, snapshot, default);
        Assert.Equal(hasStudent ? 0 : 5, state.UsedCapacity);
        Assert.Equal(hasStudent ? 0 : 1, await db.CapacityMovements.CountAsync(x => x.Action == "EmptyBaselineInitialized"));
    }
}
