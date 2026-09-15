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

    public async Task<bool> HasUsageAsync(
        Guid unitTypeId,
        CancellationToken cancellationToken = default)
    {
        if (await dbContext.UnitPurchaseIntents.AsNoTracking().AnyAsync(
                intent => intent.UnitTypeId == unitTypeId, cancellationToken))
        {
            return true;
        }

        return await dbContext.UnitGrants.AsNoTracking().AnyAsync(
            grant => grant.UnitTypeId == unitTypeId, cancellationToken);
    }

    public void Remove(UnitType unitType)
    {
        ArgumentNullException.ThrowIfNull(unitType);
        dbContext.UnitTypes.Remove(unitType);
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
            if (dbContext.Database.IsNpgsql())
            {
                var pattern = $"%{search.Replace("\\", "\\\\", StringComparison.Ordinal)
                    .Replace("%", "\\%", StringComparison.Ordinal)
                    .Replace("_", "\\_", StringComparison.Ordinal)}%";
                unitTypes = unitTypes.Where(x =>
                    EF.Functions.ILike(x.Code, pattern, "\\") ||
                    EF.Functions.ILike(x.DisplayName, pattern, "\\"));
            }
            else
            {
                unitTypes = unitTypes.Where(x =>
                    x.Code.Contains(search) || x.DisplayName.Contains(search));
            }
        }


        var code = query.Code?.Trim();
        if (!string.IsNullOrEmpty(code))
        {
            if (dbContext.Database.IsNpgsql())
            {
                var pattern = ContainsPattern(code);
                unitTypes = unitTypes.Where(x => EF.Functions.ILike(x.Code, pattern, "\\"));
            }
            else
            {
                unitTypes = unitTypes.Where(x => x.Code.Contains(code));
            }
        }

        var displayName = query.DisplayName?.Trim();
        if (!string.IsNullOrEmpty(displayName))
        {
            if (dbContext.Database.IsNpgsql())
            {
                var pattern = ContainsPattern(displayName);
                unitTypes = unitTypes.Where(x => EF.Functions.ILike(x.DisplayName, pattern, "\\"));
            }
            else
            {
                unitTypes = unitTypes.Where(x => x.DisplayName.Contains(displayName));
            }
        }

        if (query.UnitPrice is decimal unitPrice)
        {
            unitTypes = unitTypes.Where(x => x.UnitPrice == unitPrice);
        }

        var currency = query.Currency?.Trim();
        if (!string.IsNullOrEmpty(currency))
        {
            unitTypes = unitTypes.Where(x => x.Currency == currency);
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

    private static string ContainsPattern(string value) =>
        $"%{value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal)}%";
}
