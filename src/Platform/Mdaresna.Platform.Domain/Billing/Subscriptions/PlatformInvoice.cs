namespace Mdaresna.Platform.Domain.Billing.Subscriptions;

public enum InvoicePaymentStatus { Unpaid = 1, PendingReview = 2, Paid = 3, Rejected = 4 }
public enum OfflinePaymentMethod { BankTransfer = 1, CashRepresentative = 2 }

public sealed class PlatformInvoice
{
    public Guid Id { get; set; }
    public string Number { get; set; } = "";
    public Guid SchoolId { get; set; }
    public Guid? RegistrationRequestId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "";
    public InvoicePaymentStatus PaymentStatus { get; set; }
    public Guid? CurrentPaymentRequestId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? PaidAtUtc { get; set; }
    public long Version { get; set; }
    public void AwaitReview(Guid paymentId)
    {
        if (paymentId == Guid.Empty || PaymentStatus is InvoicePaymentStatus.PendingReview or InvoicePaymentStatus.Paid) throw new ArgumentException("Invoice cannot accept another receipt.");
        CurrentPaymentRequestId = paymentId; PaymentStatus = InvoicePaymentStatus.PendingReview; Version++;
    }
    public void Review(Guid paymentId, bool approved, DateTimeOffset now)
    {
        if (CurrentPaymentRequestId != paymentId || PaymentStatus != InvoicePaymentStatus.PendingReview) throw new ArgumentException("Invoice receipt changed.");
        PaymentStatus = approved ? InvoicePaymentStatus.Paid : InvoicePaymentStatus.Rejected; PaidAtUtc = approved ? now : null; Version++;
    }
}
public sealed class PlatformInvoiceLine
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid QuoteId { get; set; }
    public string SystemNameAr { get; set; } = "";
    public string SystemNameEn { get; set; } = "";
    public string PlanNameAr { get; set; } = "";
    public string PlanNameEn { get; set; } = "";
}
public sealed class PlatformInvoiceReceipt
{
    public Guid PaymentRequestId { get; set; }
    public Guid InvoiceId { get; set; }
    public OfflinePaymentMethod Method { get; set; }
    public string Reference { get; set; } = "";
    public string? RepresentativeName { get; set; }
    public DateTimeOffset ReceivedAtUtc { get; set; }
    public Guid RecordedByAccountId { get; set; }
}
public sealed class SchoolRegistrationOffer
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public string RequestHash { get; set; } = "";
    public string QuoteIdsJson { get; set; } = "";
    public DateTimeOffset CreatedAtUtc { get; set; }
}
