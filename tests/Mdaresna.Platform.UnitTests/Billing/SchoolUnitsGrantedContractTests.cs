using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Platform.Contracts.Billing;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Billing;

public sealed class SchoolUnitsGrantedContractTests
{
    [Fact]
    public void Grant_round_trips_with_version_school_scope_and_all_credit_data()
    {
        var schoolGuid = Guid.NewGuid();
        var tenantId = TenantId.From(schoolGuid);
        var schoolId = SchoolId.From(schoolGuid);
        var grantId = Guid.NewGuid();
        var paymentRequestId = Guid.NewGuid();
        var unitTypeId = Guid.NewGuid();
        var transferOccurredAtUtc = new DateTimeOffset(2026, 9, 12, 10, 0, 0, TimeSpan.Zero);
        var grantedAtUtc = new DateTimeOffset(2026, 9, 13, 10, 0, 0, TimeSpan.Zero);
        var eventData = new SchoolUnitsGrantedV1(
            grantId, paymentRequestId, tenantId, schoolId, unitTypeId,
            "STUDENT_ACTIVATION", "Student activation", 10, 15.50m, 155.00m,
            "EGP", "bank-transfer", "BANK-123", transferOccurredAtUtc, grantedAtUtc);
        var envelope = IntegrationMessageEnvelope<SchoolUnitsGrantedV1>.Create(
            grantedAtUtc,
            "mdaresna-platform",
            IntegrationMessageScope.ForSchool(tenantId, schoolId),
            eventData);

        var json = IntegrationJsonSerializer.Serialize(envelope);
        var roundTrip = IntegrationJsonSerializer.Deserialize<SchoolUnitsGrantedV1>(json);

        Assert.Equal(SchoolUnitsGrantedV1.MessageType, roundTrip.MessageType);
        Assert.Equal((ushort)1, roundTrip.SchemaVersion);
        Assert.Equal(grantId, roundTrip.Data.GrantId);
        Assert.Equal(paymentRequestId, roundTrip.Data.PaymentRequestId);
        Assert.Equal(tenantId, roundTrip.Data.TenantId);
        Assert.Equal(schoolId, roundTrip.Data.SchoolId);
        Assert.Equal(tenantId, roundTrip.Scope.TenantId);
        Assert.Equal(schoolId, roundTrip.Scope.SchoolId);
        Assert.Equal(unitTypeId, roundTrip.Data.UnitTypeId);
        Assert.Equal("STUDENT_ACTIVATION", roundTrip.Data.UnitTypeCode);
        Assert.Equal("Student activation", roundTrip.Data.UnitTypeName);
        Assert.Equal(10, roundTrip.Data.Quantity);
        Assert.Equal(15.50m, roundTrip.Data.UnitPrice);
        Assert.Equal(155.00m, roundTrip.Data.Amount);
        Assert.Equal("EGP", roundTrip.Data.Currency);
        Assert.Equal("bank-transfer", roundTrip.Data.PaymentMethodCode);
        Assert.Equal("BANK-123", roundTrip.Data.TransferReference);
        Assert.Equal(transferOccurredAtUtc, roundTrip.Data.TransferOccurredAtUtc);
        Assert.Equal(grantedAtUtc, roundTrip.Data.GrantedAtUtc);
    }

    [Fact]
    public void Grant_requires_matching_tenant_and_school_identifiers()
    {
        Assert.Throws<ArgumentException>(() => ValidGrant(tenantId: TenantId.New(), schoolId: SchoolId.New()));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Grant_requires_positive_quantity(int quantity)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ValidGrant(quantity: quantity));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1.001)]
    public void Grant_requires_positive_two_decimal_unit_price(decimal unitPrice)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ValidGrant(unitPrice: unitPrice));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(1.001)]
    public void Grant_requires_positive_two_decimal_amount(decimal amount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => ValidGrant(amount: amount));
    }

    [Theory]
    [InlineData("egp")]
    [InlineData("EURO")]
    [InlineData("12A")]
    public void Grant_requires_three_uppercase_currency_letters(string currency)
    {
        Assert.ThrowsAny<ArgumentException>(() => ValidGrant(currency: currency));
    }

    [Fact]
    public void Grant_requires_amount_to_match_quantity_times_unit_price()
    {
        Assert.Throws<ArgumentException>(() => ValidGrant(quantity: 2, amount: 10.00m));
    }

    [Fact]
    public void Grant_rejects_product_above_supported_amount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            ValidGrant(quantity: 2, unitPrice: 9999999999999999.99m,
                amount: 9999999999999999.99m));
    }

    [Theory]
    [InlineData("Bank-Transfer")]
    [InlineData("a")]
    [InlineData("bank_transfer")]
    [InlineData("bank-")]
    public void Grant_requires_lowercase_payment_method_slug(string paymentMethodCode)
    {
        Assert.Throws<ArgumentException>(() => ValidGrant(paymentMethodCode: paymentMethodCode));
    }

    [Fact]
    public void Grant_requires_transfer_to_precede_grant()
    {
        var grantedAtUtc = new DateTimeOffset(2026, 9, 13, 10, 0, 0, TimeSpan.Zero);
        Assert.Throws<ArgumentException>(() =>
            ValidGrant(transferOccurredAtUtc: grantedAtUtc.AddSeconds(1),
                grantedAtUtc: grantedAtUtc));
    }

    private static SchoolUnitsGrantedV1 ValidGrant(
        TenantId? tenantId = null,
        SchoolId? schoolId = null,
        int quantity = 1,
        decimal unitPrice = 10.00m,
        decimal amount = 10.00m,
        string currency = "EGP",
        string paymentMethodCode = "bank-transfer",
        DateTimeOffset? transferOccurredAtUtc = null,
        DateTimeOffset? grantedAtUtc = null)
    {
        var schoolGuid = Guid.NewGuid();
        var granted = grantedAtUtc ?? DateTimeOffset.UtcNow;
        return new SchoolUnitsGrantedV1(
            Guid.NewGuid(), Guid.NewGuid(), tenantId ?? TenantId.From(schoolGuid),
            schoolId ?? SchoolId.From(schoolGuid), Guid.NewGuid(),
            "STUDENT_ACTIVATION", "Student activation", quantity, unitPrice, amount,
            currency, paymentMethodCode, "BANK-123", transferOccurredAtUtc ?? granted,
            granted);
    }
}
