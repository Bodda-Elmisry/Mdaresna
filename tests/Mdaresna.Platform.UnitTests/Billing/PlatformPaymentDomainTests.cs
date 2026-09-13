using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Common;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Billing;

public sealed class PlatformPaymentDomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 13, 9, 0, 0, TimeSpan.Zero);

    [Theory]
    [InlineData("0")]
    [InlineData("-1")]
    [InlineData("1.001")]
    [InlineData("10000000000000000")]
    public void Amount_must_be_positive_and_fit_decimal_18_2(string amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => Create(decimal.Parse(amount)));
    }

    [Fact]
    public void Request_normalizes_currency_and_reference()
    {
        var request = Create(currency: " egp ", reference: " bank-001 ");

        Assert.Equal("EGP", request.Currency);
        Assert.Equal("BANK-001", request.TransferReference);
        Assert.Equal(PlatformPaymentStatus.Pending, request.Status);
        Assert.Equal(0, request.Version);
    }

    [Theory]
    [InlineData("E")]
    [InlineData("E1P")]
    [InlineData("EGPT")]
    public void Currency_must_have_three_letters(string currency)
    {
        Assert.ThrowsAny<ArgumentException>(() => Create(currency: currency));
    }

    [Fact]
    public void Approval_is_one_time_and_creates_only_an_immutable_ledger_entry()
    {
        var request = Create();
        var reviewer = IdentityAccountId.New();

        request.Review(PlatformPaymentReviewDecision.Approve, reviewer, Now.AddMinutes(1), "Bank verified");
        var ledger = PlatformPaymentLedgerEntry.FromApprovedRequest(request);

        Assert.Equal(PlatformPaymentStatus.Approved, request.Status);
        Assert.Equal(1, request.Version);
        Assert.Equal(request.Id, ledger.PaymentRequestId);
        Assert.Equal(request.Amount, ledger.Amount);
        Assert.Equal(reviewer, ledger.PostedByAccountId);
        Assert.Throws<PlatformDomainException>(() =>
            request.Review(PlatformPaymentReviewDecision.Reject, IdentityAccountId.New(), Now.AddMinutes(2), "No"));
    }

    [Fact]
    public void Rejection_requires_reason_and_never_creates_ledger_entry()
    {
        var request = Create();
        Assert.Throws<PlatformDomainException>(() =>
            request.Review(PlatformPaymentReviewDecision.Reject, IdentityAccountId.New(), Now, null));
        Assert.Equal(PlatformPaymentStatus.Pending, request.Status);

        request.Review(PlatformPaymentReviewDecision.Reject, IdentityAccountId.New(), Now.AddMinutes(1), "No bank receipt");

        Assert.Equal(PlatformPaymentStatus.Rejected, request.Status);
        Assert.Throws<PlatformDomainException>(() => PlatformPaymentLedgerEntry.FromApprovedRequest(request));
    }

    [Fact]
    public void Requester_cannot_review_their_own_payment()
    {
        var requester = IdentityAccountId.New();
        var request = Create(requester: requester);

        var exception = Assert.Throws<PlatformDomainException>(() =>
            request.Review(PlatformPaymentReviewDecision.Approve, requester, Now.AddMinutes(1), null));

        Assert.Equal("platform_payment.self_review_forbidden", exception.Code);
        Assert.Equal(PlatformPaymentStatus.Pending, request.Status);
    }

    private static PlatformPaymentRequest Create(
        decimal amount = 125.50m,
        string currency = "EGP",
        string reference = "BANK-001",
        IdentityAccountId? requester = null) => PlatformPaymentRequest.Create(
            Guid.NewGuid(),
            TenantId.New(),
            SchoolId.New(),
            amount,
            currency,
            reference,
            requester ?? IdentityAccountId.New(),
            Now);
}
