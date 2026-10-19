using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Platform.Domain.Billing.Subscriptions;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform;

public partial class PlatformDbContext
{
    private async Task PrepareInvoiceEventsAsync(CancellationToken ct)
    {
        ChangeTracker.DetectChanges();
        var ids = ChangeTracker.Entries<PlatformInvoice>().Where(e => e.State is EntityState.Added or EntityState.Modified)
            .Select(e => e.Entity.SchoolId).Concat(ChangeTracker.Entries<SchoolRegistration>()
                .Where(e => e.State == EntityState.Modified && e.Property(s => s.Status).IsModified && e.Entity.Status == SchoolLifecycleStatus.Active)
                .Select(e => e.Entity.Id.Value)).Distinct().ToArray();
        foreach (var id in ids) await StageInvoiceSnapshotsForSchoolAsync(id, ct);
    }

    public async Task StageInvoiceSnapshotsForSchoolAsync(Guid schoolId, CancellationToken ct)
    {
        var school = await Schools.FindAsync([SchoolId.From(schoolId)], ct);
        // Registration invoices are first sent once the operational database is ready.
        if (school == null || school.Status != SchoolLifecycleStatus.Active) return;
        await Set<PlatformInvoice>().Where(x => x.SchoolId == schoolId).LoadAsync(ct);
        foreach (var invoice in Set<PlatformInvoice>().Local.Where(x => x.SchoolId == schoolId).ToArray())
        {
            await Set<PlatformInvoiceLine>().Where(x => x.InvoiceId == invoice.Id).LoadAsync(ct);
            var lines = Set<PlatformInvoiceLine>().Local.Where(x => x.InvoiceId == invoice.Id).OrderBy(x => x.Id).ToArray();
            var ids = lines.Select(x => x.QuoteId).ToArray();
            await SubscriptionQuotes.Where(x => ids.Contains(x.Id)).LoadAsync(ct);
            await Set<PlatformInvoiceReceipt>().Where(x => x.InvoiceId == invoice.Id).LoadAsync(ct);
            var receipts = Set<PlatformInvoiceReceipt>().Local.Where(x => x.InvoiceId == invoice.Id).OrderBy(x => x.PaymentRequestId).ToArray();
            var paymentIds = receipts.Select(x => x.PaymentRequestId).ToArray();
            await PlatformPaymentRequests.Where(x => paymentIds.Contains(x.Id)).LoadAsync(ct);
            var data = new SchoolInvoiceSnapshotChangedV1(schoolId, school.Code.Value, invoice.Id, invoice.Number, invoice.Version,
                invoice.Amount, invoice.Currency, invoice.PaymentStatus.ToString(),
                new DateTimeOffset(invoice.CreatedAtUtc.Ticks / 10 * 10, TimeSpan.Zero), invoice.PaidAtUtc,
                lines.Select(l => { var q = SubscriptionQuotes.Local.Single(x => x.Id == l.QuoteId);
                    return new SchoolInvoiceLineV1(q.Id, q.PlanId, l.SystemNameAr, l.SystemNameEn, l.PlanNameAr, l.PlanNameEn,
                        q.BaseCapacity + q.ResultAddOnCapacity, q.AmountDue, q.Kind.ToString()); }).ToArray(),
                receipts.Select(r => new SchoolInvoiceReceiptV1(r.PaymentRequestId, r.Method.ToString(), r.Reference,
                    r.RepresentativeName, r.ReceivedAtUtc, PlatformPaymentRequests.Local.Single(p => p.Id == r.PaymentRequestId).Status.ToString())).ToArray());
            var envelope = IntegrationMessageEnvelope<SchoolInvoiceSnapshotChangedV1>.Create(DateTimeOffset.UtcNow, "mdaresna-platform",
                IntegrationMessageScope.ForSchool(school.TenantId, school.Id), data,
                new IntegrationAggregateReference("school-invoice", invoice.Id, invoice.Version));
            OutboxMessages.Add(new PlatformOutboxMessage { Id = envelope.MessageId, MessageType = envelope.MessageType,
                SchemaVersion = envelope.SchemaVersion, PayloadJson = IntegrationJsonSerializer.Serialize(envelope),
                OccurredAtUtc = envelope.OccurredAtUtc, CorrelationId = envelope.CorrelationId });
        }
    }
}
