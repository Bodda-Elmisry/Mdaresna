using Mdaresna.IntegrationContracts.Messaging;

namespace Mdaresna.Schools.Contracts.Provisioning;

public sealed record SchoolSubscriptionSnapshotChangedV1(Guid SchoolId, string SchoolCode, Guid SubscriptionId, Guid SystemId,
    string SystemCode, string CapacityMetric, long Revision, Guid PlanId, int PlanRevision, int BaseCapacity, int AddOnCapacity,
    bool IsSuspended, DateTimeOffset ValidFromUtc, DateTimeOffset ValidUntilUtc, string[] Capabilities) : IIntegrationEvent
{
    public static string MessageType => "mdaresna.schools.subscription.snapshot-changed";
    public static ushort SchemaVersion => 1;
}
