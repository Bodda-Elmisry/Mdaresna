using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Billing.Units;
using Mdaresna.Platform.Domain.Common;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Billing.Units;

public sealed class UnitCommerceDomainTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 13, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Unit_type_normalizes_offer_and_tracks_changes()
    {
        var type = UnitType.Create(Guid.NewGuid(), " lesson-unit ", " Lesson Unit ",
            12.50m, " egp ", Now);

        Assert.Equal("LESSON-UNIT", type.Code);
        Assert.Equal("EGP", type.Currency);
        Assert.Equal(0, type.Version);
        Assert.True(type.ChangeOffer("New Unit", 15m, "egp", Now.AddMinutes(1)));
        Assert.Equal(1, type.Version);
        Assert.False(type.ChangeOffer("New Unit", 15m, "EGP", Now.AddMinutes(2)));
        Assert.True(type.Deactivate(Now.AddMinutes(3)));
        Assert.Equal(2, type.Version);
        Assert.False(type.Deactivate(Now.AddMinutes(4)));
        Assert.Throws<PlatformDomainException>(() =>
            type.ChangeOffer("Another", 20m, "EGP", Now.AddMinutes(5)));
    }

    [Fact]
    public void Purchase_intent_snapshots_price_and_computes_amount()
    {
        var tenantId = TenantId.New();
        var type = UnitType.Create(Guid.NewGuid(), "UNIT-001", "Old Name", 42.50m, "EGP", Now);
        var intent = UnitPurchaseIntent.Create(Guid.NewGuid(), tenantId,
            SchoolId.From(tenantId.Value), type, 3,
            " bank-transfer ", Now.AddMinutes(1), Now.AddMinutes(2));
        type.ChangeOffer("New Name", 50m, "EGP", Now.AddMinutes(3));

        Assert.Equal("Old Name", intent.UnitTypeName);
        Assert.Equal(0, intent.UnitTypeVersion);
        Assert.Equal(42.50m, intent.UnitPrice);
        Assert.Equal(127.50m, intent.Amount);
        Assert.Equal("bank-transfer", intent.PaymentMethodCode);
    }

    [Fact]
    public void Intent_rejects_future_transfer_inactive_type_and_amount_overflow()
    {
        var tenantId = TenantId.New();
        var schoolId = SchoolId.From(tenantId.Value);
        var type = UnitType.Create(Guid.NewGuid(), "UNIT-002", "Unit", 1m, "EGP", Now);

        Assert.Throws<ArgumentException>(() => UnitPurchaseIntent.Create(
            Guid.NewGuid(), tenantId, schoolId, type, 1,
            "bank-transfer", Now.AddMinutes(2), Now.AddMinutes(1)));

        type.Deactivate(Now.AddMinutes(1));
        var error = Assert.Throws<PlatformDomainException>(() => UnitPurchaseIntent.Create(
            Guid.NewGuid(), tenantId, schoolId, type, 1,
            "bank-transfer", Now, Now.AddMinutes(2)));
        Assert.Equal("unit_type.inactive", error.Code);

        var expensive = UnitType.Create(Guid.NewGuid(), "UNIT-003", "Expensive",
            9999999999999999.99m, "EGP", Now);
        Assert.Throws<ArgumentOutOfRangeException>(() => UnitPurchaseIntent.Create(
            Guid.NewGuid(), tenantId, schoolId, expensive, 2,
            "bank-transfer", Now, Now.AddMinutes(1)));
    }

    [Fact]
    public void Grant_requires_approved_matching_payment_and_preserves_snapshot()
    {
        var tenantId = TenantId.New();
        var schoolId = SchoolId.From(tenantId.Value);
        var type = UnitType.Create(Guid.NewGuid(), "UNIT-004", "Unit", 25m, "EGP", Now);
        var requestId = Guid.NewGuid();
        var intent = UnitPurchaseIntent.Create(requestId, tenantId, schoolId, type, 4,
            "bank-transfer", Now.AddMinutes(1), Now.AddMinutes(2));
        var requester = IdentityAccountId.New();
        var payment = PlatformPaymentRequest.Create(requestId, tenantId, schoolId,
            100m, "EGP", " transfer-001 ", requester, Now.AddMinutes(2));

        Assert.Throws<PlatformDomainException>(() =>
            UnitGrant.FromApprovedPayment(payment, intent, Now.AddMinutes(4)));

        payment.Review(PlatformPaymentReviewDecision.Approve,
            IdentityAccountId.New(), Now.AddMinutes(3), "Verified");
        var grant = UnitGrant.FromApprovedPayment(payment, intent, Now.AddMinutes(4));

        Assert.Equal(4, grant.Quantity);
        Assert.Equal(25m, grant.UnitPrice);
        Assert.Equal(100m, grant.Amount);
        Assert.Equal("TRANSFER-001", grant.TransferReference);
        Assert.Equal(intent.PaymentMethodCode, grant.PaymentMethodCode);
        Assert.Equal(intent.TransferOccurredAtUtc, grant.TransferOccurredAtUtc);

        var wrongAmount = PlatformPaymentRequest.Create(Guid.NewGuid(), tenantId, schoolId,
            101m, "EGP", "transfer-002", IdentityAccountId.New(), Now.AddMinutes(2));
        wrongAmount.Review(PlatformPaymentReviewDecision.Approve,
            IdentityAccountId.New(), Now.AddMinutes(3), "Verified");
        var mismatch = Assert.Throws<PlatformDomainException>(() =>
            UnitGrant.FromApprovedPayment(wrongAmount, intent, Now.AddMinutes(4)));
        Assert.Equal("unit_grant.payment_mismatch", mismatch.Code);
    }
}
