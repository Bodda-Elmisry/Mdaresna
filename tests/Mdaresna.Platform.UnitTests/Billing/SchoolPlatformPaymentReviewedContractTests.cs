using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Platform.Contracts.Billing;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Billing;

public sealed class SchoolPlatformPaymentReviewedContractTests
{
    [Fact]
    public void Reviewed_event_round_trips_with_version_and_school_scope()
    {
        var tenantId = TenantId.New();
        var schoolId = SchoolId.New();
        var requestId = Guid.NewGuid();
        var eventData = new SchoolPlatformPaymentReviewedV1(
            requestId, tenantId, schoolId, 100.50m, "EGP", "BANK-123",
            SchoolPlatformPaymentStatusV1.Approved,
            new DateTimeOffset(2026, 9, 13, 10, 0, 0, TimeSpan.Zero));
        var envelope = IntegrationMessageEnvelope<SchoolPlatformPaymentReviewedV1>.Create(
            eventData.ReviewedAtUtc,
            "mdaresna-platform",
            IntegrationMessageScope.ForSchool(tenantId, schoolId),
            eventData);

        var json = IntegrationJsonSerializer.Serialize(envelope);
        var roundTrip = IntegrationJsonSerializer.Deserialize<SchoolPlatformPaymentReviewedV1>(json);

        Assert.Equal(SchoolPlatformPaymentReviewedV1.MessageType, roundTrip.MessageType);
        Assert.Equal((ushort)1, roundTrip.SchemaVersion);
        Assert.Equal(requestId, roundTrip.Data.PaymentRequestId);
        Assert.Equal(tenantId, roundTrip.Scope.TenantId);
        Assert.Equal(schoolId, roundTrip.Scope.SchoolId);
        Assert.Equal(100.50m, roundTrip.Data.Amount);
    }
}
