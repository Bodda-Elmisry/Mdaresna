namespace Mdaresna.Platform.Domain.Billing.Subscriptions;

public enum SubscriptionChangeKind { New = 1, AddOn = 2, Upgrade = 3, Renew = 4 }
public enum SubscriptionQuoteStatus { Offered = 1, Submitted = 2, Applied = 3, Rejected = 4 }

public sealed class SchoolSubscription
{
    private SchoolSubscription() { }
    public Guid Id { get; private set; }
    public Guid SchoolId { get; private set; }
    public Guid SystemId { get; private set; }
    public Guid PlanId { get; private set; }
    public int PlanRevision { get; private set; }
    public int BaseCapacity { get; private set; }
    public int AddOnCapacity { get; private set; }
    public decimal BaseAmount { get; private set; }
    public decimal AddOnUnitPrice { get; private set; }
    public string Currency { get; private set; } = "";
    public string CapabilityCodes { get; private set; } = "";
    public DateTimeOffset ValidFromUtc { get; private set; }
    public DateTimeOffset ValidUntilUtc { get; private set; }
    public long Version { get; private set; }
    public bool IsSuspended { get; private set; }

    public static SchoolSubscription ApplyNew(SubscriptionQuote quote)
    {
        if (quote.Kind != SubscriptionChangeKind.New || quote.SourceVersion != 0) throw new ArgumentException("Invalid initial subscription.");
        var item = new SchoolSubscription { Id = quote.SubscriptionId, SchoolId = quote.SchoolId, SystemId = quote.SystemId };
        item.Apply(quote); return item;
    }
    public void Apply(SubscriptionQuote quote)
    {
        if (quote.Status != SubscriptionQuoteStatus.Submitted || quote.SubscriptionId != Id || quote.SchoolId != SchoolId || quote.SystemId != SystemId || quote.SourceVersion != Version)
            throw new ArgumentException("The source subscription changed.");
        PlanId = quote.PlanId; PlanRevision = quote.PlanRevision; BaseCapacity = quote.BaseCapacity; AddOnCapacity = quote.ResultAddOnCapacity;
        BaseAmount = quote.BaseAmount; AddOnUnitPrice = quote.AddOnUnitPrice; Currency = quote.Currency;
        ValidFromUtc = quote.ValidFromUtc; ValidUntilUtc = quote.ValidUntilUtc; Version = checked(Version + 1);
        CapabilityCodes = quote.CapabilityCodes;
    }
    public void SetSuspended(bool value) { if (IsSuspended != value) { IsSuspended = value; Version++; } }
}

/// <summary>Price and capacity snapshot. Submission/application only change its lifecycle, never its terms.</summary>
public sealed class SubscriptionQuote
{
    private SubscriptionQuote() { }
    public Guid Id { get; private set; }
    public Guid SchoolId { get; private set; }
    public Guid SystemId { get; private set; }
    public Guid SubscriptionId { get; private set; }
    public SubscriptionChangeKind Kind { get; private set; }
    public long SourceVersion { get; private set; }
    public long CatalogVersion { get; private set; }
    public Guid PlanId { get; private set; }
    public int PlanRevision { get; private set; }
    public int BaseCapacity { get; private set; }
    public int AdditionalQuantity { get; private set; }
    public int ResultAddOnCapacity { get; private set; }
    public decimal BaseAmount { get; private set; }
    public decimal AddOnUnitPrice { get; private set; }
    public decimal AmountDue { get; private set; }
    public string Currency { get; private set; } = "";
    public string CapabilityCodes { get; private set; } = "";
    public DateTimeOffset ValidFromUtc { get; private set; }
    public DateTimeOffset ValidUntilUtc { get; private set; }
    public DateTimeOffset ExpiresAtUtc { get; private set; }
    public Guid? PaymentRequestId { get; private set; }
    public Guid? InvoiceId { get; private set; }
    public SubscriptionQuoteStatus Status { get; private set; }
    public long Version { get; private set; }
    public DateTimeOffset? AppliedAtUtc { get; private set; }

    public static SubscriptionQuote Create(Guid id, Guid schoolId, SchoolSystemDefinition system, SystemPlan plan, SystemPlanRevision offer,
        SubscriptionChangeKind kind, SchoolSubscription? current, int additionalQuantity, DateTimeOffset startsAt, DateTimeOffset now)
    {
        if (id == Guid.Empty || schoolId == Guid.Empty || !Enum.IsDefined(kind) || !system.IsActive ||
            (kind != SubscriptionChangeKind.AddOn && plan.IsArchived) || plan.SystemId != system.Id || offer.PlanId != plan.Id ||
            (kind != SubscriptionChangeKind.AddOn && offer.Revision != plan.PublishedRevision) ||
            (kind == SubscriptionChangeKind.AddOn && offer.Revision != current?.PlanRevision))
            throw new ArgumentException("An active system and published plan are required.");
        if (kind == SubscriptionChangeKind.New && current != null || kind != SubscriptionChangeKind.New && current == null)
            throw new ArgumentException("Invalid subscription operation.");
        if (current is not null && (current.SchoolId != schoolId || current.SystemId != system.Id || current.IsSuspended)) throw new ArgumentException("Invalid source subscription.");
        if (current is not null && current.Currency != offer.Currency) throw new ArgumentException("Subscription currency cannot change.");
        if (startsAt.Offset != TimeSpan.Zero || startsAt.Year >= 9999) throw new ArgumentException("A supported UTC start is required.");
        var extra = current?.AddOnCapacity ?? 0;
        decimal due;
        if (kind == SubscriptionChangeKind.AddOn)
        {
            if (additionalQuantity is < 1 or > 1_000_000 || current!.PlanId != plan.Id || now < current.ValidFromUtc || now >= current.ValidUntilUtc)
                throw new ArgumentException("Additional capacity requires an active current subscription.");
            extra = checked(extra + additionalQuantity); due = decimal.Round(additionalQuantity * current.AddOnUnitPrice, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
            if (kind is not (SubscriptionChangeKind.New or SubscriptionChangeKind.Renew) && additionalQuantity != 0) throw new ArgumentException("Additional quantity only applies to initial subscriptions, add-ons or renewal.");
            due = kind == SubscriptionChangeKind.Upgrade ? offer.BaseAmount - current!.BaseAmount : offer.BaseAmount;
        }
        if (kind == SubscriptionChangeKind.Upgrade && (offer.Capacity <= current!.BaseCapacity || now < current.ValidFromUtc || now >= current.ValidUntilUtc || due < 0))
            throw new ArgumentException("Upgrade must increase base capacity without creating an implicit refund.");
        if (kind == SubscriptionChangeKind.New && startsAt < now.AddMinutes(-5)) throw new ArgumentException("Initial subscription cannot be backdated.");
        if (kind == SubscriptionChangeKind.Renew && startsAt != current!.ValidUntilUtc) throw new ArgumentException("Renewal must start at the existing term end.");
        if (kind == SubscriptionChangeKind.Renew && now < current!.ValidUntilUtc) throw new ArgumentException("Early renewal scheduling is not enabled yet.");
        if (kind is SubscriptionChangeKind.New or SubscriptionChangeKind.Renew)
        {
            if (additionalQuantity is < 0 or > 1_000_000) throw new ArgumentException("Select a valid add-on capacity.");
            extra = additionalQuantity;
            due = decimal.Round(checked(offer.BaseAmount + extra * offer.AddOnUnitPrice), 2, MidpointRounding.AwayFromZero);
        }
        if (due < 0 || due > 9999999999999999.99m || extra > 1_000_000 || (long)offer.Capacity + extra > 2_000_000)
            throw new ArgumentException("The resulting amount or capacity exceeds the supported range.");
        var useCurrentOffer = kind == SubscriptionChangeKind.AddOn;
        return new() { Id = id, SchoolId = schoolId, SystemId = system.Id, SubscriptionId = current?.Id ?? Guid.NewGuid(), Kind = kind,
            SourceVersion = current?.Version ?? 0, CatalogVersion = system.Version, PlanId = plan.Id,
            PlanRevision = useCurrentOffer ? current!.PlanRevision : offer.Revision,
            BaseCapacity = useCurrentOffer ? current!.BaseCapacity : offer.Capacity,
            BaseAmount = useCurrentOffer ? current!.BaseAmount : offer.BaseAmount,
            AddOnUnitPrice = useCurrentOffer ? current!.AddOnUnitPrice : offer.AddOnUnitPrice,
            AdditionalQuantity = additionalQuantity, ResultAddOnCapacity = extra, AmountDue = due, Currency = offer.Currency,
            CapabilityCodes = kind is SubscriptionChangeKind.New or SubscriptionChangeKind.Renew ? system.CapabilityCodes : current!.CapabilityCodes,
            ValidFromUtc = kind is SubscriptionChangeKind.AddOn or SubscriptionChangeKind.Upgrade ? current!.ValidFromUtc : startsAt,
            ValidUntilUtc = kind is SubscriptionChangeKind.AddOn or SubscriptionChangeKind.Upgrade ? current!.ValidUntilUtc : startsAt.AddYears(1),
            ExpiresAtUtc = now.AddMinutes(30), Status = SubscriptionQuoteStatus.Offered, Version = 1 };
    }
    public void Submit(Guid? paymentId, DateTimeOffset now)
    {
        if (Status != SubscriptionQuoteStatus.Offered || now >= ExpiresAtUtc || (AmountDue > 0) != paymentId.HasValue)
            throw new ArgumentException("Quote expired or is not available for submission.");
        PaymentRequestId = paymentId; Status = SubscriptionQuoteStatus.Submitted; Version++;
    }
    public void AssignInvoice(Guid id)
    {
        if (id == Guid.Empty || InvoiceId != null || Status != SubscriptionQuoteStatus.Offered) throw new ArgumentException("Quote already invoiced.");
        InvoiceId = id; Version++;
    }
    public void SubmitInvoiced(Guid paymentId)
    {
        if (InvoiceId == null || paymentId == Guid.Empty || Status != SubscriptionQuoteStatus.Offered) throw new ArgumentException("Invalid invoice submission.");
        PaymentRequestId = paymentId; Status = SubscriptionQuoteStatus.Submitted; Version++;
    }
    public void RejectInvoicedPayment()
    {
        if (InvoiceId == null || Status != SubscriptionQuoteStatus.Submitted) throw new ArgumentException("Invalid invoice rejection.");
        PaymentRequestId = null; Status = SubscriptionQuoteStatus.Offered; Version++;
    }
    public void StartInvoicedInitialTerm(DateTimeOffset now)
    {
        if (InvoiceId == null || Kind != SubscriptionChangeKind.New || Status != SubscriptionQuoteStatus.Submitted || now.Offset != TimeSpan.Zero)
            throw new ArgumentException("Invalid initial contract activation.");
        ValidFromUtc = now; ValidUntilUtc = now.AddYears(1); Version++;
    }
    public void Complete(bool approved, DateTimeOffset now)
    {
        if (Status != SubscriptionQuoteStatus.Submitted) throw new ArgumentException("Quote was already reviewed.");
        Status = approved ? SubscriptionQuoteStatus.Applied : SubscriptionQuoteStatus.Rejected; AppliedAtUtc = approved ? now : null; Version++;
    }
}
