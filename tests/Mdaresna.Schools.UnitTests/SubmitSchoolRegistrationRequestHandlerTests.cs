using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Schools.Application.Registration;
using Mdaresna.Schools.Contracts.Registration;
using Mdaresna.SharedKernel.Time;

namespace Mdaresna.Schools.UnitTests;

public sealed class SubmitSchoolRegistrationRequestHandlerTests
{
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
        public Task PublishAsync(IntegrationMessageEnvelope<SchoolRegistrationRequestedV2> envelope,
            CancellationToken cancellationToken = default)
        {
            Envelope = envelope;
            return Task.CompletedTask;
        }
    }

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;
    }
}
