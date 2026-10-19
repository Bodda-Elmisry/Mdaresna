namespace Mdaresna.Schools.Domain.School;

public sealed class SchoolSubscriptionState
{
    public Guid SystemId { get; set; }
    public Guid SubscriptionId { get; set; }
    public string SystemCode { get; set; } = "";
    public string CapacityMetric { get; set; } = "";
    public long Revision { get; set; }
    public long LocalVersion { get; set; }
    public Guid PlanId { get; set; }
    public int BaseCapacity { get; set; }
    public int AddOnCapacity { get; set; }
    public int UsedCapacity { get; set; }
    public bool IsSuspended { get; set; }
    public bool IsEnforced { get; set; }
    public DateTimeOffset ValidFromUtc { get; set; }
    public DateTimeOffset ValidUntilUtc { get; set; }
    public DateTimeOffset SyncedAtUtc { get; set; }
    public string CapabilityCodes { get; set; } = "";
}
public sealed class SchoolCapacityAllocation
{
    public Guid Id { get; set; }
    public Guid SystemId { get; set; }
    public Guid SubjectId { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
public sealed class SchoolCapacityMovement
{
    public Guid Id { get; set; }
    public Guid SystemId { get; set; }
    public Guid? SubjectId { get; set; }
    public Guid ActorId { get; set; }
    public string Action { get; set; } = "";
    public DateTimeOffset OccurredAtUtc { get; set; }
}
