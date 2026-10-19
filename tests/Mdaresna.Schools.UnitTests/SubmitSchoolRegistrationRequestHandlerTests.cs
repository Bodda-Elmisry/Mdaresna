using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Schools.Application.Registration;
using Mdaresna.Schools.Contracts.Registration;
using Mdaresna.SharedKernel.Time;

namespace Mdaresna.Schools.UnitTests;

public sealed class SubmitSchoolRegistrationRequestHandlerTests
{
    [Fact]
    public async Task Selected_plans_are_prepared_in_platform_before_the_v3_registration_event_is_published()
    {
        var publisher = new CapturingPublisher(); var catalog = new CapturingCatalog(); var id = Guid.NewGuid(); var plan = Guid.NewGuid();
        var handler = new SubmitSchoolRegistrationRequestHandler(publisher, new FixedClock(DateTimeOffset.UtcNow), catalog);
        var input = new SubmitSchoolRegistrationRequest("مدرسة", "القاهرة", RequestedSchoolTypeV2.Private, "01000000000", "ولي المدرسة", "01000000001", [new(plan, 7, 25)], id);
        var result = await handler.HandleAsync(input, null);
        var envelope = Assert.IsType<IntegrationMessageEnvelope<SchoolRegistrationRequestedV3>>(publisher.Envelope);
        Assert.Equal(id, result.RegistrationRequestId); Assert.Equal(id, envelope.Data.OfferId);
        Assert.Equal(id, envelope.Data.Request.TenantId.Value); Assert.Null(envelope.Scope.SchoolId);
        Assert.Equal(new RegistrationPlanSelection(plan, 7, 25), Assert.Single(catalog.Input!.Plans));
        var roundtrip = Mdaresna.IntegrationContracts.Serialization.IntegrationJsonSerializer.Deserialize<SchoolRegistrationRequestedV3>(Mdaresna.IntegrationContracts.Serialization.IntegrationJsonSerializer.Serialize(envelope));
        Assert.Equal(envelope.Data, roundtrip.Data);
    }
    [Fact]
    public async Task New_registration_without_plan_choices_is_rejected_before_publishing()
    {
        var publisher = new CapturingPublisher();
        var handler = new SubmitSchoolRegistrationRequestHandler(publisher, new FixedClock(DateTimeOffset.UtcNow), new CapturingCatalog());
        await Assert.ThrowsAsync<ArgumentException>(() => handler.HandleAsync(new("مدرسة", "القاهرة", RequestedSchoolTypeV2.Private, "01000000000", "ولي المدرسة", "01000000001"), null));
        Assert.Null(publisher.Envelope);
    }
    [Fact]
    public async Task Submit_publishes_tenant_scoped_v2_event_with_owner_data()
    {
        var now = new DateTimeOffset(2026, 9, 17, 0, 0, 0, TimeSpan.Zero);
        var publisher = new CapturingPublisher();
        var handler = new SubmitSchoolRegistrationRequestHandler(publisher, new FixedClock(now));

        var result = await handler.HandleAsync(new SubmitSchoolRegistrationRequest(
            "مدرسة النور", "القاهرة", RequestedSchoolTypeV2.Private,
            "00201111111111", "أحمد علي", "00201222222222"), null);

        var envelope = Assert.IsType<IntegrationMessageEnvelope<SchoolRegistrationRequestedV2>>(
            publisher.Envelope);
        Assert.Equal(result.RegistrationRequestId, envelope.Data.RegistrationRequestId);
        Assert.Equal(envelope.Data.TenantId, envelope.Scope.TenantId);
        Assert.Null(envelope.Scope.SchoolId);
        Assert.Equal("00201222222222", envelope.Data.OwnerPhone);
        Assert.Equal(now, envelope.OccurredAtUtc);
        Assert.Equal((ushort)2, envelope.SchemaVersion);
    }

    private sealed class CapturingPublisher : ISchoolRegistrationRequestPublisher
    {
        public object? Envelope { get; private set; }
        public Task PublishAsync(IntegrationMessageEnvelope<SchoolRegistrationRequestedV3> envelope, CancellationToken cancellationToken = default)
        { Envelope = envelope; return Task.CompletedTask; }
        public Task PublishAsync(IntegrationMessageEnvelope<SchoolRegistrationRequestedV2> envelope,
            CancellationToken cancellationToken = default)
        {
            Envelope = envelope;
            return Task.CompletedTask;
        }
    }
    private sealed class CapturingCatalog : ISchoolRegistrationCatalog
    {
        public PrepareRegistrationOffer? Input;
        public Task<RegistrationPlanOffer[]> PlansAsync(CancellationToken ct) => Task.FromResult(Array.Empty<RegistrationPlanOffer>());
        public Task<Guid> PrepareAsync(PrepareRegistrationOffer input, CancellationToken ct) { Input = input; return Task.FromResult(input.Request.RegistrationRequestId); }
    }

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }
}
