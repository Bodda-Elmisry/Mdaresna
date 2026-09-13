using Mdaresna.Platform.Application.Billing.Units;
using Mdaresna.Platform.Domain.Billing.Units;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;

internal sealed class UnitTypeRepository(PlatformDbContext dbContext) : IUnitTypeRepository
{
    public Task<UnitType?> FindByIdAsync(
        Guid unitTypeId,
        CancellationToken cancellationToken = default) =>
        dbContext.UnitTypes.SingleOrDefaultAsync(
            x => x.Id == unitTypeId, cancellationToken);

    public Task<UnitType?> FindByCodeAsync(
        string normalizedCode,
        CancellationToken cancellationToken = default) =>
        dbContext.UnitTypes.SingleOrDefaultAsync(
            x => x.Code == normalizedCode, cancellationToken);

    public async Task AddAsync(
        UnitType unitType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(unitType);
        await dbContext.UnitTypes.AddAsync(unitType, cancellationToken);
    }

    public async Task<UnitTypePage> ListAsync(
        UnitTypeListQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.PageNumber < 1 || query.PageSize is < 1 or > 100)
        {
            throw new ArgumentOutOfRangeException(nameof(query),
                "Page number must be positive and page size must be between 1 and 100.");
        }

        var unitTypes = dbContext.UnitTypes.AsNoTracking();
        if (query.IsActive is bool isActive)
        {
            unitTypes = unitTypes.Where(x => x.IsActive == isActive);
        }

        var search = query.Search?.Trim();
        if (!string.IsNullOrEmpty(search))
        {
            unitTypes = unitTypes.Where(x =>
                x.Code.Contains(search) || x.DisplayName.Contains(search));
        }

        var totalCount = await unitTypes.CountAsync(cancellationToken);
        var skip = ((long)query.PageNumber - 1) * query.PageSize;
        var items = skip > int.MaxValue
            ? []
            : await unitTypes.OrderBy(x => x.Code)
                .Skip((int)skip)
                .Take(query.PageSize)
                .Select(x => new UnitTypeReadModel(
                    x.Id, x.Code, x.DisplayName, x.UnitPrice, x.Currency,
                    x.IsActive, x.CreatedAtUtc, x.UpdatedAtUtc, x.Version))
                .ToArrayAsync(cancellationToken);

        return new UnitTypePage(items, totalCount, query.PageNumber, query.PageSize);
    }
}
