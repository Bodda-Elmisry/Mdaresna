using System.Text.Json.Serialization;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Contracts.Common;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Contracts.Billing;

public sealed record SchoolPlatformPaymentReviewedV1 : IIntegrationEvent
{
    [JsonConstructor]
    public SchoolPlatformPaymentReviewedV1(
        Guid paymentRequestId,
        TenantId tenantId,
        SchoolId schoolId,
        decimal amount,
        string currency,
        string transferReference,
        SchoolPlatformPaymentStatusV1 status,
        DateTimeOffset reviewedAtUtc)
    {
        PaymentRequestId = ContractGuard.NonEmpty(paymentRequestId, nameof(paymentRequestId));
        if (tenantId.IsEmpty || schoolId.IsEmpty)
        {
            throw new ArgumentException("TenantId and SchoolId are required.");
        }

        if (amount <= 0 || amount > 9999999999999999.99m || decimal.Round(amount, 2) != amount)
        {
            throw new ArgumentOutOfRangeException(nameof(amount));
        }

        TenantId = tenantId;
        SchoolId = schoolId;
        Amount = amount;
        var normalizedCurrency = ContractGuard.Text(currency, 3, nameof(currency));
        if (normalizedCurrency.Length != 3 || normalizedCurrency.Any(ch => ch is < 'A' or > 'Z'))
        {
            throw new ArgumentException("Currency must be a three-letter uppercase ISO code.", nameof(currency));
        }

        Currency = normalizedCurrency;
        TransferReference = ContractGuard.Text(transferReference, 100, nameof(transferReference));
        Status = ContractGuard.Defined(status, nameof(status));
        ReviewedAtUtc = ContractGuard.Utc(reviewedAtUtc, nameof(reviewedAtUtc));
    }

    public static string MessageType => "mdaresna.platform.billing.school-platform-payment-reviewed";
    public static ushort SchemaVersion => 1;

    [JsonPropertyName("paymentRequestId")]
    public Guid PaymentRequestId { get; }

    [JsonPropertyName("tenantId")]
    public TenantId TenantId { get; }

    [JsonPropertyName("schoolId")]
    public SchoolId SchoolId { get; }

    [JsonPropertyName("amount")]
    public decimal Amount { get; }

    [JsonPropertyName("currency")]
    public string Currency { get; }

    [JsonPropertyName("transferReference")]
    public string TransferReference { get; }

    [JsonPropertyName("status")]
    public SchoolPlatformPaymentStatusV1 Status { get; }

    [JsonPropertyName("reviewedAtUtc")]
    public DateTimeOffset ReviewedAtUtc { get; }
}
