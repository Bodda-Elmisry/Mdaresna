using Mdaresna.Platform.Application.Errors;

namespace Mdaresna.Platform.Application.Billing.Units;

public sealed class ListUnitTypesQueryHandler(IUnitTypeRepository types)
{
    public Task<UnitTypePage> HandleAsync(
        UnitTypeListQuery query,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(query);
        if (query.PageNumber < 1 || query.PageSize is < 1 or > 100 ||
            ((long)query.PageNumber - 1) * query.PageSize > int.MaxValue ||
            query.Search?.Trim().Length > 200)
        {
            throw new ArgumentException("Invalid unit type listing request.", nameof(query));
        }

        return types.ListAsync(query with
        {
            Search = string.IsNullOrWhiteSpace(query.Search) ? null : query.Search.Trim()
        }, cancellationToken);
    }

    public async Task<UnitTypeReadModel> GetAsync(
        Guid unitTypeId,
        CancellationToken cancellationToken = default)
    {
        if (unitTypeId == Guid.Empty)
        {
            throw new ArgumentException("UnitTypeId cannot be empty.", nameof(unitTypeId));
        }

        var unitType = await types.FindByIdAsync(unitTypeId, cancellationToken)
            ?? throw new PlatformResourceNotFoundException(
                "unit_type.not_found", "Unit type was not found.");
        return new UnitTypeReadModel(
            unitType.Id, unitType.Code, unitType.DisplayName,
            unitType.UnitPrice, unitType.Currency, unitType.IsActive,
            unitType.CreatedAtUtc, unitType.UpdatedAtUtc, unitType.Version);
    }
}
