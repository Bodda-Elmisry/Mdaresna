using Mdaresna.IntegrationContracts.Messaging;

namespace Mdaresna.Schools.Contracts.Provisioning;

public sealed record SchoolInvoiceLineV1(Guid QuoteId, Guid PlanId, string SystemNameAr, string SystemNameEn,
    string PlanNameAr, string PlanNameEn, int Capacity, decimal Amount, string Kind);
public sealed record SchoolInvoiceReceiptV1(Guid PaymentRequestId, string Method, string Reference,
    string? RepresentativeName, DateTimeOffset ReceivedAtUtc, string Status);
public sealed record SchoolInvoiceSnapshotChangedV1(Guid SchoolId, string SchoolCode, Guid InvoiceId, string Number,
    long Revision, decimal Amount, string Currency, string PaymentStatus, DateTimeOffset CreatedAtUtc,
    DateTimeOffset? PaidAtUtc, SchoolInvoiceLineV1[] Lines, SchoolInvoiceReceiptV1[] Receipts) : IIntegrationEvent
{
    public static string MessageType => "mdaresna.schools.invoice.snapshot-changed";
    public static ushort SchemaVersion => 1;
}
