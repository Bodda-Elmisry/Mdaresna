using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Mdaresna.Platform.Application.Billing;
using Mdaresna.Platform.Application.Billing.Subscriptions;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Billing.Subscriptions;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Mdaresna.Schools.Contracts.Registration;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;

internal sealed class PlatformInvoiceService(PlatformDbContext db, IPlatformBillingUnitOfWork uow) : IPlatformInvoiceService
{
    public async Task<RegistrationPlanOffer[]> RegistrationPlansAsync(CancellationToken ct) => await (
        from plan in db.SystemPlans.AsNoTracking()
        join system in db.SchoolSystems on plan.SystemId equals system.Id
        join offer in db.SystemPlanRevisions on new { PlanId = plan.Id, Revision = plan.PublishedRevision } equals new { offer.PlanId, Revision = (int?)offer.Revision }
        where system.IsActive && !plan.IsArchived
        orderby system.Code, plan.SortOrder
        select new RegistrationPlanOffer(system.Id, system.NameAr, system.NameEn, system.CapacityMetric.ToString(), plan.Id, plan.NameAr, plan.NameEn,
            plan.SortOrder, offer.Revision, offer.Capacity, offer.BaseUnitPrice, offer.AddOnUnitPrice, offer.BaseAmount, offer.Currency)).ToArrayAsync(ct);

    private static string Hash(SchoolRegistrationRequestedV2 request) => Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { request.RegistrationRequestId, request.TenantId, request.SchoolCode, request.SchoolName, request.SchoolType, request.Address, request.SchoolPrimaryPhone, request.OwnerName, request.OwnerPhone }))));
    public async Task<Guid> PrepareRegistrationAsync(PrepareRegistrationOffer input, CancellationToken ct)
    {
        if (input.Request == null || input.Plans == null || input.Plans.Length is < 1 or > 3 || input.Plans.Select(x => x.PlanId).Distinct().Count() != input.Plans.Length)
            throw new ArgumentException("Select one published plan per requested system.");
        var old = await db.Set<SchoolRegistrationOffer>().AsNoTracking().SingleOrDefaultAsync(x => x.Id == input.Request.RegistrationRequestId, ct);
        if (old != null)
        {
            var ids = JsonSerializer.Deserialize<Guid[]>(old.QuoteIdsJson)!;
            var previous = await db.SubscriptionQuotes.AsNoTracking().Where(x => ids.Contains(x.Id)).ToArrayAsync(ct);
            if (old.RequestHash != Hash(input.Request) || previous.Length != input.Plans.Length || previous.Any(q => !input.Plans.Any(p => p.PlanId == q.PlanId && p.Revision == q.PlanRevision && p.AdditionalQuantity == q.AdditionalQuantity)))
                throw Conflict("request.idempotency_key_reused");
            return old.Id;
        }
        var now = DateTimeOffset.UtcNow; var quotes = new List<SubscriptionQuote>(); var metrics = new HashSet<CapacityMetric>();
        foreach (var selection in input.Plans)
        {
            var plan = await db.SystemPlans.AsNoTracking().SingleOrDefaultAsync(x => x.Id == selection.PlanId, ct) ?? throw Conflict("registration.plan_changed");
            if (plan.PublishedRevision != selection.Revision) throw Conflict("registration.plan_changed");
            var system = await db.SchoolSystems.AsNoTracking().SingleAsync(x => x.Id == plan.SystemId, ct);
            if (!metrics.Add(system.CapacityMetric)) throw new ArgumentException("Select at most one plan for each system capacity metric.");
            var offer = await db.SystemPlanRevisions.AsNoTracking().SingleAsync(x => x.PlanId == plan.Id && x.Revision == selection.Revision, ct);
            quotes.Add(SubscriptionQuote.Create(Guid.NewGuid(), input.Request.TenantId.Value, system, plan, offer, SubscriptionChangeKind.New, null, selection.AdditionalQuantity, now, now));
        }
        if (quotes.Select(x => x.Currency).Distinct().Count() != 1 || quotes.Sum(x => x.AmountDue) > 9999999999999999.99m) throw new ArgumentException("Selected plans must share a supported invoice currency and total.");
        db.SubscriptionQuotes.AddRange(quotes);
        db.Add(new SchoolRegistrationOffer { Id = input.Request.RegistrationRequestId, SchoolId = input.Request.TenantId.Value,
            RequestHash = Hash(input.Request), QuoteIdsJson = JsonSerializer.Serialize(quotes.Select(x => x.Id)), CreatedAtUtc = now });
        await db.SaveChangesAsync(ct); return input.Request.RegistrationRequestId;
    }

    public async Task CreateRegistrationInvoiceAsync(Guid offerId, SchoolRegistrationRequestedV2 request, CatalogActor actor, CancellationToken ct)
    {
        if (offerId != request.RegistrationRequestId) throw new ArgumentException("Registration offer scope mismatch.");
        if (await db.Set<PlatformInvoice>().AnyAsync(x => x.RegistrationRequestId == request.RegistrationRequestId, ct)) return;
        var offer = await db.Set<SchoolRegistrationOffer>().SingleOrDefaultAsync(x => x.Id == offerId, ct) ?? throw new InvalidOperationException("Registration offer is not committed yet.");
        if (offer.SchoolId != request.TenantId.Value || offer.RequestHash != Hash(request)) throw new ArgumentException("Registration offer payload mismatch.");
        var ids = JsonSerializer.Deserialize<Guid[]>(offer.QuoteIdsJson)!;
        var quotes = await db.SubscriptionQuotes.Where(x => ids.Contains(x.Id)).ToArrayAsync(ct);
        if (quotes.Length != ids.Length || quotes.Any(x => x.SchoolId != offer.SchoolId || x.InvoiceId != null)) throw new ArgumentException("Registration offer changed.");
        var invoice = await StageInvoiceAsync(db, quotes, request.RegistrationRequestId, ct);
        Audit(actor, "invoice.registration.issued", invoice.Id, new { invoice.SchoolId, invoice.Amount, invoice.Currency });
    }

    internal static async Task<PlatformInvoice> StageInvoiceAsync(PlatformDbContext db, SubscriptionQuote[] quotes, Guid? registrationId, CancellationToken ct)
    {
        if (quotes.Length == 0 || quotes.Any(x => x.AmountDue <= 0 || x.SchoolId != quotes[0].SchoolId || x.Currency != quotes[0].Currency || x.InvoiceId != null)) throw new ArgumentException("Invoice terms are invalid.");
        var id = Guid.NewGuid(); var invoice = new PlatformInvoice { Id = id, Number = "INV-" + id.ToString("N").ToUpperInvariant(),
            SchoolId = quotes[0].SchoolId, RegistrationRequestId = registrationId, Amount = quotes.Sum(x => x.AmountDue), Currency = quotes[0].Currency,
            PaymentStatus = InvoicePaymentStatus.Unpaid, Version = 1, CreatedAtUtc = DateTimeOffset.UtcNow };
        db.Add(invoice);
        foreach (var quote in quotes)
        {
            var system = await db.SchoolSystems.AsNoTracking().SingleAsync(x => x.Id == quote.SystemId, ct);
            var plan = await db.SystemPlans.AsNoTracking().SingleAsync(x => x.Id == quote.PlanId, ct);
            quote.AssignInvoice(id);
            db.Add(new PlatformInvoiceLine { Id = Guid.NewGuid(), InvoiceId = id, QuoteId = quote.Id, SystemNameAr = system.NameAr, SystemNameEn = system.NameEn, PlanNameAr = plan.NameAr, PlanNameEn = plan.NameEn });
        }
        return invoice;
    }

    public async Task<InvoiceView[]> ListAsync(Guid? schoolId, int page, CancellationToken ct)
    {
        if (page is < 1 or > 100000) throw new ArgumentException("Invalid invoice page.");
        var invoices = await db.Set<PlatformInvoice>().AsNoTracking().Where(x => schoolId == null || x.SchoolId == schoolId).OrderByDescending(x => x.CreatedAtUtc).Skip((page - 1) * 30).Take(30).ToArrayAsync(ct);
        var ids = invoices.Select(x => x.Id).ToArray();
        var lines = await (from line in db.Set<PlatformInvoiceLine>().AsNoTracking() join quote in db.SubscriptionQuotes on line.QuoteId equals quote.Id where ids.Contains(line.InvoiceId)
            select new { line.InvoiceId, Row = new InvoiceLineView(quote.Id, line.SystemNameAr, line.SystemNameEn, line.PlanNameAr, line.PlanNameEn, quote.BaseCapacity + quote.ResultAddOnCapacity, quote.AmountDue, quote.Kind) }).ToArrayAsync(ct);
        var receipts = await (from receipt in db.Set<PlatformInvoiceReceipt>().AsNoTracking() join payment in db.PlatformPaymentRequests on receipt.PaymentRequestId equals payment.Id where ids.Contains(receipt.InvoiceId)
            orderby payment.RequestedAtUtc descending
            select new { receipt.InvoiceId, Row = new InvoiceReceiptView(receipt.PaymentRequestId, receipt.Method, receipt.Reference, receipt.RepresentativeName, receipt.ReceivedAtUtc, payment.Status.ToString(), payment.ReviewNote, receipt.RecordedByAccountId) }).ToArrayAsync(ct);
        var schoolIds = invoices.Select(x => SchoolId.From(x.SchoolId)).ToArray();
        var schools = await db.Schools.AsNoTracking().Where(x => schoolIds.Contains(x.Id)).ToDictionaryAsync(x => x.Id.Value, x => x.DisplayName, ct);
        return invoices.Select(x => new InvoiceView(x.Id, x.Number, x.SchoolId, schools.GetValueOrDefault(x.SchoolId, ""), x.Amount, x.Currency, x.PaymentStatus, x.Version, x.RegistrationRequestId,
            x.CreatedAtUtc, x.PaidAtUtc, lines.Where(y => y.InvoiceId == x.Id).Select(y => y.Row).ToArray(), receipts.Where(y => y.InvoiceId == x.Id).Select(y => y.Row).ToArray())).ToArray();
    }
    public Task<Guid> IssueQuoteAsync(Guid quoteId, CatalogActor actor, CancellationToken ct) => uow.ExecuteInTransactionAsync(async token => {
        var quote = await db.SubscriptionQuotes.SingleOrDefaultAsync(x => x.Id == quoteId, token)
            ?? throw new PlatformResourceNotFoundException("subscription.quote_not_found", "Quote was not found.");
        if (quote.InvoiceId is { } existing) return existing;
        if (quote.Status != SubscriptionQuoteStatus.Offered || quote.ExpiresAtUtc <= DateTimeOffset.UtcNow || quote.AmountDue <= 0)
            throw Conflict("invoice.invalid_quote");
        // Registration offers become invoices only after the registry request is accepted.
        if (!await db.Schools.AnyAsync(x => x.Id == SchoolId.From(quote.SchoolId), token)) throw Conflict("invoice.invalid_quote");
        var invoice = await StageInvoiceAsync(db, [quote], null, token);
        Audit(actor, "invoice.subscription.issued", invoice.Id, new { quote.Id, invoice.Amount, invoice.Currency });
        await uow.SaveChangesAsync(token); return invoice.Id;
    }, ct);
    public Task RecordAsync(Guid invoiceId, RecordInvoiceReceipt input, CatalogActor actor, CancellationToken ct) => uow.ExecuteInTransactionAsync(async token => {
        ArgumentException.ThrowIfNullOrWhiteSpace(input.Reference);
        if (input.ReceivedAtUtc.Offset != TimeSpan.Zero) throw new ArgumentException("A UTC payment date is required.");
        // PostgreSQL stores microseconds; normalize both providers before idempotency comparison.
        var receivedAt = new DateTimeOffset(input.ReceivedAtUtc.Ticks / 10 * 10, TimeSpan.Zero);
        var invoice = await LockAsync(db, invoiceId, token);
        var old = await db.Set<PlatformInvoiceReceipt>().AsNoTracking().SingleOrDefaultAsync(x => x.PaymentRequestId == input.PaymentRequestId, token);
        if (old != null)
        {
            if (old.InvoiceId != invoiceId || old.Method != input.Method || old.Reference != input.Reference.Trim().ToUpperInvariant() || old.RepresentativeName != input.RepresentativeName?.Trim() || old.ReceivedAtUtc != receivedAt || input.Amount != invoice.Amount)
                throw Conflict("request.idempotency_key_reused");
            return true;
        }
        if (invoice.Version != input.ExpectedVersion) throw Conflict("invoice.version_conflict");
        if (input.Amount != invoice.Amount || !Enum.IsDefined(input.Method) || input.ReceivedAtUtc.Offset != TimeSpan.Zero || input.ReceivedAtUtc > DateTimeOffset.UtcNow.AddMinutes(5) || input.ReceivedAtUtc.Year < 2000)
            throw new ArgumentException("Full invoice amount and a valid offline payment date are required.");
        if (input.Method == OfflinePaymentMethod.CashRepresentative && (string.IsNullOrWhiteSpace(input.RepresentativeName) || input.RepresentativeName.Trim().Length > 200)) throw new ArgumentException("Cash receipts require the representative name or code.");
        if (input.Method == OfflinePaymentMethod.BankTransfer && !string.IsNullOrWhiteSpace(input.RepresentativeName)) throw new ArgumentException("Bank transfer receipts do not carry a cash representative.");
        var payment = PlatformPaymentRequest.Create(input.PaymentRequestId, TenantId.From(invoice.SchoolId), SchoolId.From(invoice.SchoolId), invoice.Amount, invoice.Currency,
            input.Reference, actor.AccountId, DateTimeOffset.UtcNow);
        invoice.AwaitReview(payment.Id); db.PlatformPaymentRequests.Add(payment);
        db.Add(new PlatformInvoiceReceipt { PaymentRequestId = payment.Id, InvoiceId = invoice.Id, Method = input.Method, Reference = payment.TransferReference,
            RepresentativeName = input.RepresentativeName?.Trim(), ReceivedAtUtc = receivedAt, RecordedByAccountId = actor.AccountId.Value });
        var quoteIds = await db.Set<PlatformInvoiceLine>().Where(x => x.InvoiceId == invoice.Id).Select(x => x.QuoteId).ToArrayAsync(token);
        foreach (var quote in await db.SubscriptionQuotes.Where(x => quoteIds.Contains(x.Id)).ToArrayAsync(token)) quote.SubmitInvoiced(payment.Id);
        Audit(actor, "invoice.offline_receipt.recorded", invoice.Id, new { payment.Id, input.Method, invoice.Amount });
        await uow.SaveChangesAsync(token); return true;
    }, ct);
    internal static async Task<PlatformInvoice> LockAsync(PlatformDbContext db, Guid id, CancellationToken ct)
    {
        if (db.Database.IsRelational() && db.Database.CurrentTransaction == null) throw new InvalidOperationException("Invoice changes require a transaction.");
        var query = db.Database.IsNpgsql() ? db.Set<PlatformInvoice>().FromSqlInterpolated($"SELECT * FROM billing.invoices WHERE \"Id\" = {id} FOR UPDATE")
            : db.Database.IsSqlServer() ? db.Set<PlatformInvoice>().FromSqlInterpolated($"SELECT * FROM billing.invoices WITH (UPDLOCK, HOLDLOCK) WHERE Id = {id}") : db.Set<PlatformInvoice>();
        return await query.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw new PlatformResourceNotFoundException("invoice.not_found", "Invoice was not found.");
    }
    private void Audit(CatalogActor actor, string action, Guid id, object data) => db.AuditEntries.Add(new PlatformAuditEntry { Id = Guid.NewGuid(), AccountId = actor.AccountId,
        Action = action, ResourceType = "invoice", ResourceId = id.ToString("D"), OccurredAtUtc = DateTimeOffset.UtcNow, CorrelationId = actor.CorrelationId.ToString("D"), MetadataJson = JsonSerializer.Serialize(data) });
    private static PlatformConflictException Conflict(string code) => new(code, "Invoice or registration terms changed; refresh and retry.");
}
