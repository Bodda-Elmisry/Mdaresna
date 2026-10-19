using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Billing.Subscriptions;

namespace Mdaresna.Platform.Application.Billing.Subscriptions;

public sealed record CreateSubscriptionQuote(Guid Id, Guid SchoolId, Guid PlanId, SubscriptionChangeKind Kind, long ExpectedSubscriptionVersion, int AdditionalQuantity, DateTimeOffset StartsAtUtc);
public sealed record SubmitSubscriptionQuote(Guid PaymentRequestId, string TransferReference);
public interface ISubscriptionService
{
    Task<SchoolSubscription[]> ListAsync(Guid schoolId, CancellationToken ct);
    Task<SubscriptionQuote[]> QuotesAsync(Guid schoolId, CancellationToken ct);
    Task<SubscriptionQuote> QuoteAsync(CreateSubscriptionQuote input, CatalogActor actor, CancellationToken ct);
    Task<SubscriptionQuote> SubmitAsync(Guid quoteId, SubmitSubscriptionQuote input, CatalogActor actor, CancellationToken ct);
    Task<SubscriptionQuote> ApplyFreeAsync(Guid quoteId, CatalogActor actor, CancellationToken ct);
    Task SetSuspendedAsync(Guid id, long expectedVersion, bool suspended, CatalogActor actor, CancellationToken ct);
    Task<SchoolSubscriptionDelivery?> DeliveryAsync(Guid id, CancellationToken ct);
    Task ResynchronizeAsync(Guid id, long expectedVersion, CatalogActor actor, CancellationToken ct);
    Task RecordAppliedAsync(Guid schoolId, Guid systemId, Guid subscriptionId, long revision, DateTimeOffset appliedAtUtc, CancellationToken ct);
    Task PrepareInitialActivationAsync(Guid schoolId, CatalogActor actor, CancellationToken ct);
}
public interface ISubscriptionPaymentEffect
{
    Task BeforeReviewAsync(Guid paymentId, CancellationToken ct) => Task.CompletedTask;
    Task ApplyReviewAsync(PlatformPaymentRequest payment, CatalogActor actor, CancellationToken ct);
}
