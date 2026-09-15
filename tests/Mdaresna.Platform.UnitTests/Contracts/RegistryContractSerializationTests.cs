using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Platform.Contracts.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Contracts;

public sealed class RegistryContractSerializationTests
{
    [Fact]
    public void School_registration_requested_v1_round_trips_with_tenant_scope()
    {
        var tenantId = TenantId.New();
        var requestedAtUtc = new DateTimeOffset(2026, 9, 15, 12, 0, 0, TimeSpan.Zero);
        var data = new SchoolRegistrationRequestedV1(
            Guid.NewGuid(),
            tenantId,
            Guid.NewGuid(),
            "SCH-100",
            "Future School",
            "Future School LLC",
            SchoolTypeV1.Private,
            SchoolDeploymentModeV1.SharedSaaS,
            "Cairo",
            Guid.NewGuid(),
            requestedAtUtc);
        var envelope = IntegrationMessageEnvelope<SchoolRegistrationRequestedV1>.Create(
            requestedAtUtc,
            "schools",
            IntegrationMessageScope.ForTenant(tenantId),
            data);

        var json = IntegrationJsonSerializer.Serialize(envelope);
        var roundTrip = IntegrationJsonSerializer
            .Deserialize<SchoolRegistrationRequestedV1>(json);

        Assert.Equal(envelope, roundTrip);
        Assert.Contains(
            "\"messageType\":\"mdaresna.schools.registry.school-registration-requested\"",
            json,
            StringComparison.Ordinal);
        Assert.Contains("\"schoolType\":\"Private\"", json, StringComparison.Ordinal);
        Assert.Contains("\"tenantId\"", json, StringComparison.Ordinal);
    }

    [Fact]
    public void School_registered_v1_round_trips_with_stable_string_enums()
    {
        var tenantId = TenantId.New();
        var schoolId = SchoolId.New();
        var occurredAtUtc = new DateTimeOffset(2026, 9, 12, 11, 0, 0, TimeSpan.Zero);
        var data = new SchoolRegisteredV1(
            Guid.NewGuid(),
            tenantId,
            schoolId,
            "GOV-100",
            "Government School",
            SchoolTypeV1.Government,
            SchoolDeploymentModeV1.GovernmentOnPremises,
            occurredAtUtc);
        var envelope = IntegrationMessageEnvelope<SchoolRegisteredV1>.Create(
            occurredAtUtc,
            "platform-unit-tests",
            IntegrationMessageScope.ForSchool(tenantId, schoolId),
            data);

        var json = IntegrationJsonSerializer.Serialize(envelope);
        var roundTrip = IntegrationJsonSerializer.Deserialize<SchoolRegisteredV1>(json);

        Assert.Equal(envelope, roundTrip);
        Assert.Contains("\"schoolType\":\"Government\"", json, StringComparison.Ordinal);
        Assert.Contains(
            "\"deploymentMode\":\"GovernmentOnPremises\"",
            json,
            StringComparison.Ordinal);
        Assert.DoesNotContain("SchoolType", json, StringComparison.Ordinal);
    }
}
