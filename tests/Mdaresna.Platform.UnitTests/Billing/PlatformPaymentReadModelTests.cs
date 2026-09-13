using Mdaresna.Platform.Application.Billing.Read;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Billing;

public sealed class PlatformPaymentReadModelTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 13, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Pending_detail_does_not_claim_a_ledger_entry()
    {
        var request = CreateRequest();

        var detail = PlatformPaymentDetail.From(request, null);

        Assert.Equal(PlatformPaymentStatus.Pending, detail.Status);
        Assert.Null(detail.Ledger);
        Assert.Null(detail.ReviewedByAccountId);
    }

    [Fact]
    public void Approved_detail_contains_matching_posted_ledger()
    {
        var request = CreateRequest();
        request.Review(PlatformPaymentReviewDecision.Approve, IdentityAccountId.New(), Now.AddMinutes(1), null);
        var ledger = PlatformPaymentLedgerEntry.FromApprovedRequest(request);

        var detail = PlatformPaymentDetail.From(request, ledger);

        Assert.Equal(ledger.Id, detail.Ledger?.LedgerEntryId);
        Assert.Equal(request.Amount, detail.Ledger?.Amount);
        Assert.Equal(request.ReviewedByAccountId, detail.Ledger?.PostedByAccountId);
    }

    [Fact]
    public void Detail_rejects_a_ledger_from_another_payment()
    {
        var request = CreateRequest();
        var other = CreateRequest();
        other.Review(PlatformPaymentReviewDecision.Approve, IdentityAccountId.New(), Now.AddMinutes(1), null);

        Assert.Throws<ArgumentException>(() =>
            PlatformPaymentDetail.From(request, PlatformPaymentLedgerEntry.FromApprovedRequest(other)));
    }

    private static PlatformPaymentRequest CreateRequest() => PlatformPaymentRequest.Create(
        Guid.NewGuid(), TenantId.New(), SchoolId.New(), 42.50m, "EGP", "BANK-42",
        IdentityAccountId.New(), Now);
}
