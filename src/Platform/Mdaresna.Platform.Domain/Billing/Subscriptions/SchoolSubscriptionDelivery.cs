namespace Mdaresna.Platform.Domain.Billing.Subscriptions;

public sealed class SchoolSubscriptionDelivery
{
    public Guid SubscriptionId { get; set; }
    public long AppliedRevision { get; set; }
    public DateTimeOffset AppliedAtUtc { get; set; }
    public DateTimeOffset ReceivedAtUtc { get; set; }
}
