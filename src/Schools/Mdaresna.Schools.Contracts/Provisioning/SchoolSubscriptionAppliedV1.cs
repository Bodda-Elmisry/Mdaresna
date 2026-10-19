using Mdaresna.IntegrationContracts.Messaging;
namespace Mdaresna.Schools.Contracts.Provisioning;

public sealed record SchoolSubscriptionAppliedV1(Guid SchoolId, Guid SystemId, Guid SubscriptionId, long Revision, DateTimeOffset AppliedAtUtc) : IIntegrationEvent
{
    public static string MessageType => "mdaresna.schools.subscription.applied";
    public static ushort SchemaVersion => 1;
}
