using Mdaresna.Platform.Domain.Billing.Units;

namespace Mdaresna.Platform.Application.Billing.Units;

public interface IUnitPurchaseRepository
{
    Task<UnitPurchaseIntent?> FindIntentAsync(
        Guid paymentRequestId,
        CancellationToken cancellationToken = default);

    Task AddIntentAsync(
        UnitPurchaseIntent intent,
        CancellationToken cancellationToken = default);

    Task<UnitGrant?> FindGrantAsync(
        Guid paymentRequestId,
        CancellationToken cancellationToken = default);

    void AddGrant(UnitGrant grant);
}
