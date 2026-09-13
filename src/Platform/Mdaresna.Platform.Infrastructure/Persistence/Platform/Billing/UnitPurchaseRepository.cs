using Mdaresna.Platform.Application.Billing.Units;
using Mdaresna.Platform.Domain.Billing.Units;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;

internal sealed class UnitPurchaseRepository(PlatformDbContext dbContext) : IUnitPurchaseRepository
{
    public Task<UnitPurchaseIntent?> FindIntentAsync(
        Guid paymentRequestId,
        CancellationToken cancellationToken = default) =>
        dbContext.UnitPurchaseIntents.AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.PaymentRequestId == paymentRequestId, cancellationToken);

    public async Task AddIntentAsync(
        UnitPurchaseIntent intent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(intent);
        await dbContext.UnitPurchaseIntents.AddAsync(intent, cancellationToken);
    }

    public Task<UnitGrant?> FindGrantAsync(
        Guid paymentRequestId,
        CancellationToken cancellationToken = default) =>
        dbContext.UnitGrants.AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.PaymentRequestId == paymentRequestId, cancellationToken);

    public void AddGrant(UnitGrant grant)
    {
        ArgumentNullException.ThrowIfNull(grant);
        dbContext.UnitGrants.Add(grant);
    }
}
