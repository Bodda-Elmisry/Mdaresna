using System.Text.Json.Serialization;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Contracts.Common;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Contracts.Billing;

/// <summary>
/// A durable, school-scoped grant of units after a platform payment is approved.
/// GrantId is the receiving School App's idempotency key.
/// </summary>
public sealed record SchoolUnitsGrantedV1 : IIntegrationEvent
{
    [JsonConstructor]
    public SchoolUnitsGrantedV1(
        Guid grantId,
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
        string paymentMethodCode,
        string transferReference,
        DateTimeOffset transferOccurredAtUtc,
        DateTimeOffset grantedAtUtc)
    {
        GrantId = ContractGuard.NonEmpty(grantId, nameof(grantId));
        PaymentRequestId = ContractGuard.NonEmpty(paymentRequestId, nameof(paymentRequestId));

        if (tenantId.IsEmpty || schoolId.IsEmpty || tenantId.Value != schoolId.Value)
        {
            throw new ArgumentException("TenantId and SchoolId must be the same non-empty school identifier.");
        }

        TenantId = tenantId;
        SchoolId = schoolId;
        UnitTypeId = ContractGuard.NonEmpty(unitTypeId, nameof(unitTypeId));
        UnitTypeCode = ContractGuard.Text(unitTypeCode, 32, nameof(unitTypeCode));
        UnitTypeName = ContractGuard.Text(unitTypeName, 200, nameof(unitTypeName));

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity));
        }

        Quantity = quantity;
        UnitPrice = ValidMoney(unitPrice, nameof(unitPrice));
        Amount = ValidMoney(amount, nameof(amount));

        decimal expectedAmount;
        try
        {
            expectedAmount = checked(UnitPrice * Quantity);
        }
        catch (OverflowException)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity,
                "Quantity multiplied by unit price exceeds the supported amount.");
        }

        if (expectedAmount > 9999999999999999.99m)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity,
                "Quantity multiplied by unit price exceeds decimal(18,2).");
        }

        if (expectedAmount != Amount)
        {
            throw new ArgumentException("Amount must equal quantity multiplied by unit price.", nameof(amount));
        }

        var normalizedCurrency = ContractGuard.Text(currency, 3, nameof(currency));
        if (normalizedCurrency.Length != 3 || normalizedCurrency.Any(ch => ch is < 'A' or > 'Z'))
        {
            throw new ArgumentException("Currency must be a three-letter uppercase ISO code.", nameof(currency));
        }

        Currency = normalizedCurrency;
        PaymentMethodCode = ContractGuard.Text(paymentMethodCode, 40, nameof(paymentMethodCode));
        if (PaymentMethodCode.Length < 3 ||
            PaymentMethodCode[0] is < 'a' or > 'z' ||
            PaymentMethodCode[^1] is '-' ||
            PaymentMethodCode.Any(ch => ch is not (>= 'a' and <= 'z') and not (>= '0' and <= '9') and not '-'))
        {
            throw new ArgumentException("Payment method code must be a 3-40 character lowercase slug.", nameof(paymentMethodCode));
        }

        TransferReference = ContractGuard.Text(transferReference, 100, nameof(transferReference));
        TransferOccurredAtUtc = ContractGuard.Utc(transferOccurredAtUtc, nameof(transferOccurredAtUtc));
        GrantedAtUtc = ContractGuard.Utc(grantedAtUtc, nameof(grantedAtUtc));
        if (TransferOccurredAtUtc > GrantedAtUtc)
        {
            throw new ArgumentException("Transfer time cannot be after grant time.", nameof(transferOccurredAtUtc));
        }
    }

    public static string MessageType => "mdaresna.platform.billing.school-units-granted";
    public static ushort SchemaVersion => 1;

    [JsonPropertyName("grantId")]
    public Guid GrantId { get; }

    [JsonPropertyName("paymentRequestId")]
    public Guid PaymentRequestId { get; }

    [JsonPropertyName("tenantId")]
    public TenantId TenantId { get; }

    [JsonPropertyName("schoolId")]
    public SchoolId SchoolId { get; }

    [JsonPropertyName("unitTypeId")]
    public Guid UnitTypeId { get; }

    [JsonPropertyName("unitTypeCode")]
    public string UnitTypeCode { get; }

    [JsonPropertyName("unitTypeName")]
    public string UnitTypeName { get; }

    [JsonPropertyName("quantity")]
    public int Quantity { get; }

    [JsonPropertyName("unitPrice")]
    public decimal UnitPrice { get; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; }

    [JsonPropertyName("currency")]
    public string Currency { get; }

    [JsonPropertyName("paymentMethodCode")]
    public string PaymentMethodCode { get; }

    [JsonPropertyName("transferReference")]
    public string TransferReference { get; }

    [JsonPropertyName("transferOccurredAtUtc")]
    public DateTimeOffset TransferOccurredAtUtc { get; }

    [JsonPropertyName("grantedAtUtc")]
    public DateTimeOffset GrantedAtUtc { get; }

    private static decimal ValidMoney(decimal value, string parameterName) =>
        value <= 0 || value > 9999999999999999.99m || decimal.Round(value, 2) != value
            ? throw new ArgumentOutOfRangeException(parameterName)
            : value;
}
