using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Common;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Domain.Billing;

/// <summary>Immutable record that the Platform accepted a school's reported payment.</summary>
public sealed class PlatformPaymentLedgerEntry
{
    private PlatformPaymentLedgerEntry(
        Guid id,
        Guid paymentRequestId,
        TenantId tenantId,
        SchoolId schoolId,
        decimal amount,
        string currency,
        string transferReference,
        IdentityAccountId postedByAccountId,
        DateTimeOffset postedAtUtc)
    {
        Id = id;
        PaymentRequestId = paymentRequestId;
        TenantId = tenantId;
        SchoolId = schoolId;
        Amount = amount;
        Currency = currency;
        TransferReference = transferReference;
        PostedByAccountId = postedByAccountId;
        PostedAtUtc = postedAtUtc;
    }

    public Guid Id { get; }
    public Guid PaymentRequestId { get; }
    public TenantId TenantId { get; }
    public SchoolId SchoolId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string TransferReference { get; }
    public IdentityAccountId PostedByAccountId { get; }
    public DateTimeOffset PostedAtUtc { get; }

    public static PlatformPaymentLedgerEntry FromApprovedRequest(PlatformPaymentRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Status != PlatformPaymentStatus.Approved ||
            !request.ReviewedAtUtc.HasValue ||
            !request.ReviewedByAccountId.HasValue)
        {
            throw new PlatformDomainException(
                "platform_payment.not_approved",
                "Only an approved platform payment can be posted to the ledger.");
        }

        return new PlatformPaymentLedgerEntry(
            Guid.NewGuid(),
            request.Id,
            request.TenantId,
            request.SchoolId,
            request.Amount,
            request.Currency,
            request.TransferReference,
            request.ReviewedByAccountId.Value,
            request.ReviewedAtUtc.Value);
    }
}
