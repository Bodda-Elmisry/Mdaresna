using System.Text.Json;
using Mdaresna.Platform.Application.Billing;
using Mdaresna.Platform.Application.Billing.Subscriptions;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Billing.Subscriptions;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.IntegrationContracts.Serialization;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;

internal sealed class SubscriptionService(PlatformDbContext db, IPlatformBillingUnitOfWork uow) : ISubscriptionService, ISubscriptionPaymentEffect
{
    public Task<SchoolSubscription[]> ListAsync(Guid schoolId, CancellationToken ct) => db.SchoolSubscriptions.AsNoTracking().Where(x => x.SchoolId == schoolId).OrderBy(x => x.SystemId).ToArrayAsync(ct);
    public Task<SubscriptionQuote[]> QuotesAsync(Guid schoolId, CancellationToken ct) => db.SubscriptionQuotes.AsNoTracking().Where(x => x.SchoolId == schoolId).OrderByDescending(x => x.ExpiresAtUtc).Take(100).ToArrayAsync(ct);
    public Task<SchoolSubscriptionDelivery?> DeliveryAsync(Guid id, CancellationToken ct) => db.Set<SchoolSubscriptionDelivery>().AsNoTracking().SingleOrDefaultAsync(x => x.SubscriptionId == id, ct);
    public Task ResynchronizeAsync(Guid id, long expectedVersion, CatalogActor actor, CancellationToken ct) => uow.ExecuteInTransactionAsync(async token => {
        var subscription = await db.SchoolSubscriptions.SingleOrDefaultAsync(x => x.Id == id, token) ?? throw Missing();
        if (subscription.Version != expectedVersion) throw Conflict("subscription.version_conflict");
        await StageSnapshot(subscription, token); Audit(actor, "subscription.snapshot.resent", id, new { subscription.Version });
        await db.StageInvoiceSnapshotsForSchoolAsync(subscription.SchoolId, token);
        await uow.SaveChangesAsync(token); return true;
    }, ct);
    public async Task RecordAppliedAsync(Guid schoolId, Guid systemId, Guid subscriptionId, long revision, DateTimeOffset appliedAtUtc, CancellationToken ct)
    {
        if (revision < 1 || appliedAtUtc.Offset != TimeSpan.Zero || appliedAtUtc > DateTimeOffset.UtcNow.AddMinutes(5)) throw new ArgumentException("Invalid subscription receipt.");
        var contract = await db.SchoolSubscriptions.AsNoTracking().SingleOrDefaultAsync(x => x.Id == subscriptionId, ct);
        if (contract == null) throw new InvalidOperationException("Subscription is not committed yet.");
        if (contract.SchoolId != schoolId || contract.SystemId != systemId || revision > contract.Version) throw new ArgumentException("Subscription receipt scope mismatch.");
        var delivery = await db.Set<SchoolSubscriptionDelivery>().SingleOrDefaultAsync(x => x.SubscriptionId == subscriptionId, ct);
        if (delivery?.AppliedRevision >= revision) return;
        if (delivery == null) { delivery = new() { SubscriptionId = subscriptionId }; db.Add(delivery); }
        delivery.AppliedRevision = revision; delivery.AppliedAtUtc = appliedAtUtc; delivery.ReceivedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
    }
    public async Task<SubscriptionQuote> QuoteAsync(CreateSubscriptionQuote input, CatalogActor actor, CancellationToken ct)
    {
        var previous = await db.SubscriptionQuotes.AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.Id, ct);
        if (previous != null)
        {
            if (previous.SchoolId != input.SchoolId || previous.PlanId != input.PlanId || previous.Kind != input.Kind || previous.AdditionalQuantity != input.AdditionalQuantity ||
                previous.SourceVersion != input.ExpectedSubscriptionVersion || input.Kind is SubscriptionChangeKind.New or SubscriptionChangeKind.Renew && previous.ValidFromUtc != input.StartsAtUtc)
                throw Conflict("request.idempotency_key_reused");
            return previous;
        }
        if (input.SchoolId == Guid.Empty || !await db.Schools.AnyAsync(x => x.Id == SchoolId.From(input.SchoolId), ct)) throw Missing();
        var plan = await db.SystemPlans.AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.PlanId, ct) ?? throw Missing();
        var system = await db.SchoolSystems.AsNoTracking().SingleAsync(x => x.Id == plan.SystemId, ct);
        var current = await db.SchoolSubscriptions.AsNoTracking().SingleOrDefaultAsync(x => x.SchoolId == input.SchoolId && x.SystemId == system.Id, ct);
        var revision = input.Kind == SubscriptionChangeKind.AddOn ? current?.PlanRevision : plan.PublishedRevision;
        var offer = await db.SystemPlanRevisions.AsNoTracking().SingleOrDefaultAsync(x => x.PlanId == plan.Id && x.Revision == revision, ct) ?? throw Missing();
        if ((current?.Version ?? 0) != input.ExpectedSubscriptionVersion) throw Conflict("subscription.version_conflict");
        if (input.Kind == SubscriptionChangeKind.Upgrade)
        {
            if (current == null) throw Conflict("subscription.not_found");
            var source = await db.SystemPlans.AsNoTracking().SingleAsync(x => x.Id == current!.PlanId, ct);
            if (source.SortOrder >= plan.SortOrder) throw Conflict("subscription.upgrade_not_allowed");
        }
        var quote = SubscriptionQuote.Create(input.Id, input.SchoolId, system, plan, offer, input.Kind, current, input.AdditionalQuantity, input.StartsAtUtc, DateTimeOffset.UtcNow);
        db.SubscriptionQuotes.Add(quote); Audit(actor, "subscription.quote.created", quote.Id, new { quote.AmountDue, quote.Currency, quote.BaseCapacity, quote.ResultAddOnCapacity });
        await uow.SaveChangesAsync(ct); return quote;
    }
    public Task<SubscriptionQuote> SubmitAsync(Guid quoteId, SubmitSubscriptionQuote input, CatalogActor actor, CancellationToken ct) =>
        uow.ExecuteInTransactionAsync(async token => {
            var quote = await Quote(quoteId, token);
            if (quote.Status is SubscriptionQuoteStatus.Submitted or SubscriptionQuoteStatus.Applied)
            {
                if (quote.AmountDue > 0)
                {
                    var saved = await db.PlatformPaymentRequests.SingleAsync(x => x.Id == quote.PaymentRequestId, token);
                    if (saved.Id != input.PaymentRequestId || saved.TransferReference != (input.TransferReference ?? "").Trim().ToUpperInvariant()) throw Conflict("request.idempotency_key_reused");
                }
                return quote;
            }
            if (quote.InvoiceId != null) throw Conflict("invoice.receipt_required");
            await CheckCurrent(quote, token);
            if (quote.AmountDue > 0)
            {
                if (await db.PlatformPaymentRequests.AnyAsync(x => x.Id == input.PaymentRequestId, token)) throw Conflict("request.idempotency_key_reused");
                var payment = PlatformPaymentRequest.Create(input.PaymentRequestId, TenantId.From(quote.SchoolId), SchoolId.From(quote.SchoolId),
                    quote.AmountDue, quote.Currency, input.TransferReference, actor.AccountId, DateTimeOffset.UtcNow);
                db.PlatformPaymentRequests.Add(payment);
                var invoice = await PlatformInvoiceService.StageInvoiceAsync(db, [quote], null, token);
                invoice.AwaitReview(payment.Id);
                db.Add(new PlatformInvoiceReceipt { PaymentRequestId = payment.Id, InvoiceId = invoice.Id, Method = OfflinePaymentMethod.BankTransfer,
                    Reference = payment.TransferReference, ReceivedAtUtc = DateTimeOffset.UtcNow, RecordedByAccountId = actor.AccountId.Value });
                quote.Submit(payment.Id, DateTimeOffset.UtcNow);
            }
            else quote.Submit(null, DateTimeOffset.UtcNow);
            Audit(actor, "subscription.quote.submitted", quote.Id, new { quote.PaymentRequestId, quote.AmountDue });
            await uow.SaveChangesAsync(token); return quote;
        }, ct);
    public Task<SubscriptionQuote> ApplyFreeAsync(Guid quoteId, CatalogActor actor, CancellationToken ct) =>
        uow.ExecuteInTransactionAsync(async token => {
            var quote = await Quote(quoteId, token);
            if (quote.Status == SubscriptionQuoteStatus.Applied) return quote;
            if (quote.AmountDue != 0 || quote.Status != SubscriptionQuoteStatus.Submitted || quote.PaymentRequestId != null) throw Conflict("subscription.payment_required");
            await Apply(quote, actor, token); await uow.SaveChangesAsync(token); return quote;
        }, ct);
    public async Task BeforeReviewAsync(Guid paymentId, CancellationToken ct)
    {
        var invoiceId = await db.Set<PlatformInvoiceReceipt>().AsNoTracking().Where(x => x.PaymentRequestId == paymentId).Select(x => (Guid?)x.InvoiceId).SingleOrDefaultAsync(ct);
        if (invoiceId.HasValue) await PlatformInvoiceService.LockAsync(db, invoiceId.Value, ct);
    }
    public async Task ApplyReviewAsync(PlatformPaymentRequest payment, CatalogActor actor, CancellationToken ct)
    {
        var quotes = await db.SubscriptionQuotes.Where(x => x.PaymentRequestId == payment.Id).ToArrayAsync(ct);
        if (quotes.Length == 0) return;
        if (payment.Amount != quotes.Sum(x => x.AmountDue) || quotes.Any(x => payment.Currency != x.Currency || payment.SchoolId.Value != x.SchoolId)) throw Conflict("subscription.payment_mismatch");
        PlatformInvoice? invoice = null;
        if (quotes[0].InvoiceId is Guid invoiceId)
        {
            if (quotes.Any(x => x.InvoiceId != invoiceId)) throw Conflict("subscription.payment_mismatch");
            invoice = await db.Set<PlatformInvoice>().SingleAsync(x => x.Id == invoiceId, ct);
            invoice.Review(payment.Id, payment.Status == PlatformPaymentStatus.Approved, DateTimeOffset.UtcNow);
            Audit(actor, "invoice.offline_receipt.reviewed", invoice.Id, new { payment.Id, payment.Status });
        }
        foreach (var quote in quotes)
        {
            if (payment.Status == PlatformPaymentStatus.Approved)
            {
                if (invoice?.RegistrationRequestId == null) await Apply(quote, actor, ct);
            }
            else if (invoice != null) quote.RejectInvoicedPayment();
            else { quote.Complete(false, DateTimeOffset.UtcNow); Audit(actor, "subscription.quote.rejected", quote.Id, new { payment.Id }); }
        }
    }
    public async Task PrepareInitialActivationAsync(Guid schoolId, CatalogActor actor, CancellationToken ct)
    {
        var invoice = await db.Set<PlatformInvoice>().SingleOrDefaultAsync(x => x.SchoolId == schoolId && x.RegistrationRequestId != null, ct);
        if (invoice == null)
        {
            // Existing manually contracted registrations can proceed only after an approved invoice-backed contract.
            var hasPaidContract = await db.SchoolSubscriptions.AnyAsync(x => x.SchoolId == schoolId && !x.IsSuspended && x.ValidUntilUtc > DateTimeOffset.UtcNow, ct)
                && await db.Set<PlatformInvoice>().AnyAsync(x => x.SchoolId == schoolId && x.PaymentStatus == InvoicePaymentStatus.Paid, ct);
            if (!hasPaidContract) throw Conflict("school.subscription_payment_required");
            return;
        }
        if (invoice.PaymentStatus != InvoicePaymentStatus.Paid) throw Conflict("school.subscription_payment_required");
        var ids = await db.Set<PlatformInvoiceLine>().Where(x => x.InvoiceId == invoice.Id).Select(x => x.QuoteId).ToArrayAsync(ct);
        if (ids.Length == 0) throw Conflict("invoice.invalid_terms");
        foreach (var quote in await db.SubscriptionQuotes.Where(x => ids.Contains(x.Id)).ToArrayAsync(ct))
        {
            if (quote.Status == SubscriptionQuoteStatus.Applied) continue;
            quote.StartInvoicedInitialTerm(DateTimeOffset.UtcNow);
            await Apply(quote, actor, ct);
        }
    }
    public Task SetSuspendedAsync(Guid id, long expectedVersion, bool suspended, CatalogActor actor, CancellationToken ct) =>
        uow.ExecuteInTransactionAsync(async token => {
            var subscription = await db.SchoolSubscriptions.SingleOrDefaultAsync(x => x.Id == id, token) ?? throw Missing();
            if (subscription.Version != expectedVersion) throw Conflict("subscription.version_conflict");
            var before = subscription.Version; subscription.SetSuspended(suspended);
            if (before != subscription.Version) { await StageSnapshot(subscription, token); Audit(actor, "subscription.status.changed", id, new { suspended }); }
            await uow.SaveChangesAsync(token); return true;
        }, ct);
    private async Task Apply(SubscriptionQuote quote, CatalogActor actor, CancellationToken ct)
    {
        // Serialize all subscription changes for a school, including two initial purchases
        // for different catalog definitions with the same capacity metric.
        if (db.Database.IsNpgsql())
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM registry.schools WHERE \"Id\" = {quote.SchoolId} FOR UPDATE", ct);
        else if (db.Database.IsSqlServer())
            await db.Database.ExecuteSqlInterpolatedAsync($"SELECT 1 FROM registry.schools WITH (UPDLOCK, HOLDLOCK) WHERE Id = {quote.SchoolId}", ct);
        await CheckCurrent(quote, ct);
        var metric = await db.SchoolSystems.Where(x => x.Id == quote.SystemId).Select(x => x.CapacityMetric).SingleAsync(ct);
        if (await (from subscriptionRow in db.SchoolSubscriptions
                   join systemRow in db.SchoolSystems on subscriptionRow.SystemId equals systemRow.Id
                   where subscriptionRow.SchoolId == quote.SchoolId && subscriptionRow.SystemId != quote.SystemId && systemRow.CapacityMetric == metric
                   select subscriptionRow.Id).AnyAsync(ct)) throw Conflict("subscription.capacity_metric_already_contracted");
        if (quote.Kind is SubscriptionChangeKind.AddOn or SubscriptionChangeKind.Upgrade && DateTimeOffset.UtcNow >= quote.ValidUntilUtc)
            throw Conflict("subscription.expired");
        var subscription = await db.SchoolSubscriptions.SingleOrDefaultAsync(x => x.SchoolId == quote.SchoolId && x.SystemId == quote.SystemId, ct);
        if (subscription == null) { subscription = SchoolSubscription.ApplyNew(quote); db.SchoolSubscriptions.Add(subscription); }
        else subscription.Apply(quote);
        quote.Complete(true, DateTimeOffset.UtcNow);
        await StageSnapshot(subscription, ct);
        Audit(actor, "subscription.change.applied", subscription.Id, new { quote.Id, quote.Kind, subscription.Version, subscription.BaseCapacity, subscription.AddOnCapacity });
    }
    private async Task CheckCurrent(SubscriptionQuote quote, CancellationToken ct)
    {
        var system = await db.SchoolSystems.AsNoTracking().SingleAsync(x => x.Id == quote.SystemId, ct);
        if (!system.IsActive) throw Conflict("subscription.system_inactive");
        var current = await db.SchoolSubscriptions.SingleOrDefaultAsync(x => x.SchoolId == quote.SchoolId && x.SystemId == quote.SystemId, ct);
        if ((current?.Version ?? 0) != quote.SourceVersion || current?.IsSuspended == true) throw Conflict("subscription.version_conflict");
        // Accepted quotes retain their prices. A catalog edit cannot silently reprice a pending bank transfer.
    }
    private async Task StageSnapshot(SchoolSubscription subscription, CancellationToken ct)
    {
        var school = await db.Schools.AsNoTracking().SingleAsync(x => x.Id == SchoolId.From(subscription.SchoolId), ct);
        var system = await db.SchoolSystems.AsNoTracking().SingleAsync(x => x.Id == subscription.SystemId, ct);
        var data = new SchoolSubscriptionSnapshotChangedV1(subscription.SchoolId, school.Code.Value, subscription.Id, system.Id, system.Code,
            system.CapacityMetric.ToString(), subscription.Version, subscription.PlanId, subscription.PlanRevision,
            subscription.BaseCapacity, subscription.AddOnCapacity, subscription.IsSuspended, subscription.ValidFromUtc, subscription.ValidUntilUtc, subscription.CapabilityCodes.Split('\n'));
        var envelope = IntegrationMessageEnvelope<SchoolSubscriptionSnapshotChangedV1>.Create(DateTimeOffset.UtcNow, "mdaresna-platform",
            IntegrationMessageScope.ForSchool(school.TenantId, school.Id), data, new IntegrationAggregateReference("school-subscription", subscription.Id, subscription.Version));
        db.OutboxMessages.Add(new PlatformOutboxMessage { Id = envelope.MessageId, MessageType = envelope.MessageType, SchemaVersion = envelope.SchemaVersion,
            PayloadJson = IntegrationJsonSerializer.Serialize(envelope), OccurredAtUtc = envelope.OccurredAtUtc, CorrelationId = envelope.CorrelationId });
    }
    private async Task<SubscriptionQuote> Quote(Guid id, CancellationToken ct) => await db.SubscriptionQuotes.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw Missing();
    private void Audit(CatalogActor actor, string action, Guid id, object data) => db.AuditEntries.Add(new PlatformAuditEntry { Id = Guid.NewGuid(), AccountId = actor.AccountId,
        Action = action, ResourceType = "subscription", ResourceId = id.ToString("D"), OccurredAtUtc = DateTimeOffset.UtcNow, CorrelationId = actor.CorrelationId.ToString("D"), MetadataJson = JsonSerializer.Serialize(data) });
    private static PlatformConflictException Conflict(string code) => new(code, "The subscription request cannot be applied in its current state.");
    private static PlatformResourceNotFoundException Missing() => new("subscription.not_found", "Subscription, school or published plan was not found.");
}
