using System.Text.Json.Serialization;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.IntegrationContracts.Serialization;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.ArchitectureTests;

public sealed class IntegrationEnvelopeTests
{
    [Fact]
    public void Factory_creates_a_traceable_versioned_envelope()
    {
        var tenantId = TenantId.New();
        var schoolId = SchoolId.New();
        var branchId = BranchId.New();
        var scope = IntegrationMessageScope.ForBranch(tenantId, schoolId, branchId);
        var occurredAtUtc = DateTimeOffset.UtcNow;

        var envelope = IntegrationMessageEnvelope<TestEvent>.Create(
            occurredAtUtc,
            "architecture-tests",
            scope,
            new TestEvent("payload"),
            new IntegrationAggregateReference(
                "test-aggregate",
                Guid.NewGuid(),
                version: null));

        Assert.NotEqual(Guid.Empty, envelope.MessageId);
        Assert.NotEqual(Guid.Empty, envelope.CorrelationId);
        Assert.Equal((ushort)1, envelope.SchemaVersion);
        Assert.Equal(tenantId, envelope.Scope.TenantId);
        Assert.Equal(schoolId, envelope.Scope.SchoolId);
        Assert.Equal(branchId, envelope.Scope.BranchId);
    }

    [Fact]
    public void School_scope_requires_a_tenant_scope()
    {
        Assert.Throws<ArgumentException>(
            () => new IntegrationMessageScope(null, SchoolId.New(), null));
    }

    [Fact]
    public void Envelope_round_trips_without_changing_the_wire_contract()
    {
        var envelope = IntegrationMessageEnvelope<TestCommand>.Create(
            DateTimeOffset.UtcNow,
            "architecture-tests",
            IntegrationMessageScope.ForTenant(TenantId.New()),
            new TestCommand("payload"));

        var json = IntegrationJsonSerializer.Serialize(envelope);
        var roundTrip = IntegrationJsonSerializer.Deserialize<TestCommand>(json);

        Assert.Equal(envelope, roundTrip);
        Assert.Equal(TestCommand.MessageType, roundTrip!.MessageType);
        Assert.Equal(TestCommand.SchemaVersion, roundTrip.SchemaVersion);
    }

    [Fact]
    public void Canonical_serializer_matches_the_v1_golden_contract()
    {
        var envelope = new IntegrationMessageEnvelope<TestCommand>(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            TestCommand.MessageType,
            TestCommand.SchemaVersion,
            new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero),
            "architecture-tests",
            IntegrationMessageScope.ForTenant(
                TenantId.From(Guid.Parse("22222222-2222-2222-2222-222222222222"))),
            aggregate: null,
            correlationId: Guid.Parse("33333333-3333-3333-3333-333333333333"),
            causationId: null,
            traceParent: null,
            data: new TestCommand("payload"));

        var actual = IntegrationJsonSerializer.Serialize(envelope);
        var expected = File.ReadAllText(
            Path.Combine(
                AppContext.BaseDirectory,
                "Fixtures",
                "test-command-v1.json"));

        Assert.Equal(expected.Trim(), actual);
    }

    private sealed record TestEvent(
        [property: JsonPropertyName("value")] string Value) : IIntegrationEvent
    {
        public static string MessageType => "mdaresna.test.event";

        public static ushort SchemaVersion => 1;
    }

    private sealed record TestCommand(
        [property: JsonPropertyName("value")] string Value) : IIntegrationCommand
    {
        public static string MessageType => "mdaresna.test.command";

        public static ushort SchemaVersion => 1;
    }
}
