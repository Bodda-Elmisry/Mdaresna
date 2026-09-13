using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Billing.Read;

public sealed record PlatformPaymentDetail(
    Guid RequestId,
    TenantId TenantId,
    SchoolId SchoolId,
    decimal Amount,
    string Currency,
    string TransferReference,
    PlatformPaymentStatus Status,
    IdentityAccountId RequestedByAccountId,
    DateTimeOffset RequestedAtUtc,
    IdentityAccountId? ReviewedByAccountId,
    DateTimeOffset? ReviewedAtUtc,
    string? ReviewNote,
    PlatformPaymentLedgerDetail? Ledger)
{
    public static PlatformPaymentDetail From(
        PlatformPaymentRequest request,
        PlatformPaymentLedgerEntry? ledger)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (ledger is not null && ledger.PaymentRequestId != request.Id)
        {
            throw new ArgumentException("Ledger entry belongs to a different payment request.", nameof(ledger));
        }

        return new PlatformPaymentDetail(
            request.Id,
            request.TenantId,
            request.SchoolId,
            request.Amount,
            request.Currency,
            request.TransferReference,
            request.Status,
            request.RequestedByAccountId,
            request.RequestedAtUtc,
            request.ReviewedByAccountId,
            request.ReviewedAtUtc,
            request.ReviewNote,
            ledger is null ? null : new PlatformPaymentLedgerDetail(
                ledger.Id,
                ledger.Amount,
                ledger.Currency,
                ledger.TransferReference,
                ledger.PostedByAccountId,
                ledger.PostedAtUtc));
    }
}

public sealed record PlatformPaymentLedgerDetail(
    Guid LedgerEntryId,
    decimal Amount,
    string Currency,
    string TransferReference,
    IdentityAccountId PostedByAccountId,
    DateTimeOffset PostedAtUtc);
