using Mdaresna.Platform.Domain.Common;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Domain.Billing.Units;

/// <summary>Immutable Platform entitlement issued only after an approved payment review.</summary>
public sealed class UnitGrant
{
    private UnitGrant(
        Guid id,
        Guid paymentRequestId,
        TenantId tenantId,
        SchoolId schoolId,
        Guid unitTypeId,
        string unitTypeCode,
        string unitTypeName,
        int quantity,
        decimal unitPrice,
        decimal amount,
        string currency,
        string transferReference,
        string paymentMethodCode,
        DateTimeOffset transferOccurredAtUtc,
        DateTimeOffset grantedAtUtc)
    {
        Id = id;
        PaymentRequestId = paymentRequestId;
        TenantId = tenantId;
        SchoolId = schoolId;
        UnitTypeId = unitTypeId;
        UnitTypeCode = unitTypeCode;
        UnitTypeName = unitTypeName;
        Quantity = quantity;
        UnitPrice = unitPrice;
        Amount = amount;
        Currency = currency;
        TransferReference = transferReference;
        PaymentMethodCode = paymentMethodCode;
        TransferOccurredAtUtc = transferOccurredAtUtc;
        GrantedAtUtc = grantedAtUtc;
    }

    public Guid Id { get; }
    public Guid PaymentRequestId { get; }
    public TenantId TenantId { get; }
    public SchoolId SchoolId { get; }
    public Guid UnitTypeId { get; }
    public string UnitTypeCode { get; }
    public string UnitTypeName { get; }
    public int Quantity { get; }
    public decimal UnitPrice { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string TransferReference { get; }
    public string PaymentMethodCode { get; }
    public DateTimeOffset TransferOccurredAtUtc { get; }
    public DateTimeOffset GrantedAtUtc { get; }

    public static UnitGrant FromApprovedPayment(
        PlatformPaymentRequest payment,
        UnitPurchaseIntent intent,
        DateTimeOffset grantedAtUtc)
    {
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(intent);
        if (payment.Status != PlatformPaymentStatus.Approved ||
            payment.Id != intent.PaymentRequestId ||
            payment.TenantId != intent.TenantId ||
            payment.SchoolId != intent.SchoolId ||
            payment.Amount != intent.Amount ||
            payment.Currency != intent.Currency)
        {
            throw new PlatformDomainException(
                "unit_grant.payment_mismatch",
                "An approved matching payment is required before granting units.");
        }

        var grantedAt = DomainGuard.UtcTimestamp(grantedAtUtc, nameof(grantedAtUtc));
        if (payment.ReviewedAtUtc is not { } reviewedAt || grantedAt < reviewedAt)
        {
            throw new ArgumentException("Grant time cannot precede payment approval.", nameof(grantedAtUtc));
        }

        return new UnitGrant(Guid.NewGuid(), payment.Id, intent.TenantId,
            intent.SchoolId, intent.UnitTypeId, intent.UnitTypeCode,
            intent.UnitTypeName, intent.Quantity, intent.UnitPrice,
            intent.Amount, intent.Currency, payment.TransferReference,
            intent.PaymentMethodCode, intent.TransferOccurredAtUtc, grantedAt);
    }
}
