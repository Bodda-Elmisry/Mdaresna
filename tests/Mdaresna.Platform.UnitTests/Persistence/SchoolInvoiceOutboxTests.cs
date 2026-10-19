using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Billing.Subscriptions;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.UnitTests.Persistence;

public sealed class SchoolInvoiceOutboxTests
{
    [Fact]
    public async Task Issuing_receipting_and_approving_stage_frozen_invoice_copies_in_same_save()
    {
        await using var db = new PlatformDbContext(new DbContextOptionsBuilder<PlatformDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var clock = DateTimeOffset.UtcNow;
        var now = new DateTimeOffset(clock.Ticks / 10 * 10 + 7, TimeSpan.Zero);
        var id = Guid.NewGuid(); var actor = Guid.NewGuid(); var operation = Guid.NewGuid();
        var school = SchoolRegistration.Create(SchoolId.From(id), TenantId.From(id), Guid.NewGuid(), SchoolCode.Create("QA-SCHOOL"), "QA", SchoolType.Private, DeploymentMode.SharedSaaS, actor, now);
        school.SubmitForVerification(actor, now); school.Approve(Guid.NewGuid(), actor, now); school.BeginProvisioning(operation, actor, now);
        var system = SchoolSystemDefinition.Create(Guid.NewGuid(), "ACADEMIC", "أكاديمي", "Academic", CapacityMetric.Student, ["academic.students"], now);
        var plan = SystemPlan.Create(Guid.NewGuid(), system.Id, "PLAN-300", "باقة", "Plan", 1, now);
        var offer = SystemPlanRevision.Create(plan.Id, plan.NextRevision(), 300, 100, 100, "EGP", now); plan.Publish(1);
        var quote = SubscriptionQuote.Create(Guid.NewGuid(), id, system, plan, offer, SubscriptionChangeKind.New, null, 0, now, now);
        var invoice = new PlatformInvoice { Id = Guid.NewGuid(), Number = "INV-QA", SchoolId = id, Amount = 30000, Currency = "EGP", Version = 1, CreatedAtUtc = now, PaymentStatus = InvoicePaymentStatus.Unpaid };
        quote.AssignInvoice(invoice.Id);
        db.AddRange(school, system, plan, offer, quote, invoice, new PlatformInvoiceLine { Id = Guid.NewGuid(), InvoiceId = invoice.Id, QuoteId = quote.Id, SystemNameAr = "أكاديمي", SystemNameEn = "Academic", PlanNameAr = "باقة", PlanNameEn = "Plan" });
        foreach (var e in db.ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
            if (e.Metadata.FindProperty("RowVersion") != null) e.Property("RowVersion").CurrentValue = new byte[8];
        await db.SaveChangesAsync();
        Assert.Empty(db.OutboxMessages.Where(x => x.MessageType == SchoolInvoiceSnapshotChangedV1.MessageType));
        school.Activate(operation, actor, now); await db.SaveChangesAsync();
        Assert.Single(db.OutboxMessages.Where(x => x.MessageType == SchoolInvoiceSnapshotChangedV1.MessageType));
        var payment = PlatformPaymentRequest.Create(Guid.NewGuid(), TenantId.From(id), SchoolId.From(id), 30000, "EGP", "QA-REF", IdentityAccountId.New(), now);
        invoice.AwaitReview(payment.Id); db.Add(payment); db.Add(new PlatformInvoiceReceipt { InvoiceId = invoice.Id, PaymentRequestId = payment.Id, Method = OfflinePaymentMethod.BankTransfer, Reference = "QA-REF", ReceivedAtUtc = now, RecordedByAccountId = Guid.NewGuid() });
        foreach (var e in db.ChangeTracker.Entries().Where(e => e.State == EntityState.Added))
            if (e.Metadata.FindProperty("RowVersion") != null) e.Property("RowVersion").CurrentValue = new byte[8];
        await db.SaveChangesAsync();
        payment.Review(PlatformPaymentReviewDecision.Approve, IdentityAccountId.New(), now, null); invoice.Review(payment.Id, true, now);
        await db.SaveChangesAsync();
        var copies = db.OutboxMessages.Where(x => x.MessageType == SchoolInvoiceSnapshotChangedV1.MessageType).AsEnumerable()
            .Select(x => IntegrationJsonSerializer.Deserialize<SchoolInvoiceSnapshotChangedV1>(x.PayloadJson).Data).OrderBy(x => x.Revision).ToArray();
        Assert.Equal(new long[] { 1, 2, 3 }, copies.Select(x => x.Revision));
        Assert.Equal("Paid", copies[2].PaymentStatus); Assert.Equal("Approved", Assert.Single(copies[2].Receipts).Status);
        Assert.Equal(30000, Assert.Single(copies[2].Lines).Amount);
        Assert.Equal("Unpaid", copies[0].PaymentStatus); Assert.Empty(copies[0].Receipts);
        Assert.All(copies, copy => Assert.Equal(0, copy.CreatedAtUtc.Ticks % 10));
    }
}
