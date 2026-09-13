using Mdaresna.Platform.Domain.Common;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Domain.Billing.Units;

/// <summary>Immutable price snapshot attached one-to-one to a pending payment request.</summary>
public sealed class UnitPurchaseIntent
{
    private UnitPurchaseIntent(
        Guid paymentRequestId,
        TenantId tenantId,
        SchoolId schoolId,
        Guid unitTypeId,
        long unitTypeVersion,
        string unitTypeCode,
        string unitTypeName,
        decimal unitPrice,
        string currency,
        int quantity,
        decimal amount,
        string paymentMethodCode,
        DateTimeOffset transferOccurredAtUtc,
        DateTimeOffset createdAtUtc)
    {
        PaymentRequestId = paymentRequestId;
        TenantId = tenantId;
        SchoolId = schoolId;
        UnitTypeId = unitTypeId;
        UnitTypeVersion = unitTypeVersion;
        UnitTypeCode = unitTypeCode;
        UnitTypeName = unitTypeName;
        UnitPrice = unitPrice;
        Currency = currency;
        Quantity = quantity;
        Amount = amount;
        PaymentMethodCode = paymentMethodCode;
        TransferOccurredAtUtc = transferOccurredAtUtc;
        CreatedAtUtc = createdAtUtc;
    }

    public Guid PaymentRequestId { get; }
    public TenantId TenantId { get; }
    public SchoolId SchoolId { get; }
    public Guid UnitTypeId { get; }
    public long UnitTypeVersion { get; }
    public string UnitTypeCode { get; }
    public string UnitTypeName { get; }
    public decimal UnitPrice { get; }
    public string Currency { get; }
    public int Quantity { get; }
    public decimal Amount { get; }
    public string PaymentMethodCode { get; }
    public DateTimeOffset TransferOccurredAtUtc { get; }
    public DateTimeOffset CreatedAtUtc { get; }

    public static UnitPurchaseIntent Create(
        Guid paymentRequestId,
        TenantId tenantId,
        SchoolId schoolId,
        UnitType unitType,
        int quantity,
        string paymentMethodCode,
        DateTimeOffset transferOccurredAtUtc,
        DateTimeOffset createdAtUtc)
    {
        ArgumentNullException.ThrowIfNull(unitType);
        if (!unitType.IsActive)
        {
            throw new PlatformDomainException(
                "unit_type.inactive", "An inactive unit type cannot be purchased.");
        }

        return Build(paymentRequestId, tenantId, schoolId, unitType.Id, unitType.Version,
            unitType.Code, unitType.DisplayName, unitType.UnitPrice, unitType.Currency,
            quantity, paymentMethodCode, transferOccurredAtUtc, createdAtUtc);
    }

    internal static UnitPurchaseIntent Rehydrate(
        Guid paymentRequestId,
        TenantId tenantId,
        SchoolId schoolId,
        Guid unitTypeId,
        long unitTypeVersion,
        string unitTypeCode,
        string unitTypeName,
        decimal unitPrice,
        string currency,
        int quantity,
        decimal amount,
        string paymentMethodCode,
        DateTimeOffset transferOccurredAtUtc,
        DateTimeOffset createdAtUtc)
    {
        var intent = Build(paymentRequestId, tenantId, schoolId, unitTypeId, unitTypeVersion,
            unitTypeCode, unitTypeName, unitPrice, currency, quantity,
            paymentMethodCode, transferOccurredAtUtc, createdAtUtc);
        if (intent.Amount != amount)
        {
            throw new ArgumentException("Persisted purchase amount does not match its price snapshot.");
        }

        return intent;
    }

    private static UnitPurchaseIntent Build(
        Guid paymentRequestId,
        TenantId tenantId,
        SchoolId schoolId,
        Guid unitTypeId,
        long unitTypeVersion,
        string unitTypeCode,
        string unitTypeName,
        decimal unitPrice,
        string currency,
        int quantity,
        string paymentMethodCode,
        DateTimeOffset transferOccurredAtUtc,
        DateTimeOffset createdAtUtc)
    {
        DomainGuard.NonEmptyGuid(paymentRequestId, nameof(paymentRequestId));
        DomainGuard.NonEmptyGuid(unitTypeId, nameof(unitTypeId));
        if (unitTypeVersion < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(unitTypeVersion));
        }
        if (tenantId.IsEmpty || schoolId.IsEmpty || tenantId.Value != schoolId.Value)
        {
            throw new ArgumentException("School and tenant must share one identifier.");
        }

        if (quantity is < 1 or > 1_000_000)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity),
                "Quantity must be between 1 and 1,000,000.");
        }

        var price = UnitType.ValidatePrice(unitPrice);
        var amount = price * quantity;
        if (amount > 9999999999999999.99m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity),
                "Purchase total exceeds decimal(18,2).");
        }

        var created = DomainGuard.UtcTimestamp(createdAtUtc, nameof(createdAtUtc));
        var transferred = DomainGuard.UtcTimestamp(
            transferOccurredAtUtc, nameof(transferOccurredAtUtc));
        if (transferred > created)
        {
            throw new ArgumentException(
                "Transfer date cannot be later than purchase submission.",
                nameof(transferOccurredAtUtc));
        }

        return new UnitPurchaseIntent(
            paymentRequestId, tenantId, schoolId, unitTypeId, unitTypeVersion,
            UnitType.NormalizeCode(unitTypeCode),
            DomainGuard.RequiredText(unitTypeName, 200, nameof(unitTypeName)),
            price,
            UnitType.NormalizeCurrency(currency),
            quantity,
            amount,
            NormalizePaymentMethodCode(paymentMethodCode),
            transferred,
            created);
    }

    public static string NormalizePaymentMethodCode(string value)
    {
        var normalized = DomainGuard.RequiredText(value, 40, nameof(value)).ToLowerInvariant();
        if (normalized.Length < 3 ||
            normalized[0] is < 'a' or > 'z' ||
            normalized[^1] is '-' ||
            normalized.Any(ch => ch is not (>= 'a' and <= 'z') and not (>= '0' and <= '9') and not '-'))
        {
            throw new ArgumentException(
                "Payment method code must be a 3-40 character lowercase slug.",
                nameof(value));
        }

        return normalized;
    }
}
