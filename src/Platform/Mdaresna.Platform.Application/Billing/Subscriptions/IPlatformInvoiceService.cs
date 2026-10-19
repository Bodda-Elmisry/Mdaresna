using Mdaresna.Platform.Domain.Billing.Subscriptions;
using Mdaresna.Schools.Contracts.Registration;

namespace Mdaresna.Platform.Application.Billing.Subscriptions;

public sealed record InvoiceLineView(Guid QuoteId, string SystemNameAr, string SystemNameEn, string PlanNameAr, string PlanNameEn,
    int Capacity, decimal Amount, SubscriptionChangeKind Kind);
public sealed record InvoiceReceiptView(Guid PaymentRequestId, OfflinePaymentMethod Method, string Reference, string? RepresentativeName,
    DateTimeOffset ReceivedAtUtc, string Status, string? ReviewNote, Guid RecordedByAccountId);
public sealed record InvoiceView(Guid Id, string Number, Guid SchoolId, string SchoolName, decimal Amount, string Currency,
    InvoicePaymentStatus PaymentStatus, long Version, Guid? RegistrationRequestId, DateTimeOffset CreatedAtUtc, DateTimeOffset? PaidAtUtc,
    InvoiceLineView[] Lines, InvoiceReceiptView[] Receipts);
public sealed record RecordInvoiceReceipt(Guid PaymentRequestId, long ExpectedVersion, decimal Amount, OfflinePaymentMethod Method,
    string Reference, string? RepresentativeName, DateTimeOffset ReceivedAtUtc);
public interface IPlatformInvoiceService
{
    Task<RegistrationPlanOffer[]> RegistrationPlansAsync(CancellationToken ct);
    Task<Guid> PrepareRegistrationAsync(PrepareRegistrationOffer input, CancellationToken ct);
    Task CreateRegistrationInvoiceAsync(Guid offerId, SchoolRegistrationRequestedV2 request, CatalogActor actor, CancellationToken ct);
    Task<InvoiceView[]> ListAsync(Guid? schoolId, int page, CancellationToken ct);
    Task<Guid> IssueQuoteAsync(Guid quoteId, CatalogActor actor, CancellationToken ct);
    Task RecordAsync(Guid invoiceId, RecordInvoiceReceipt input, CatalogActor actor, CancellationToken ct);
}
