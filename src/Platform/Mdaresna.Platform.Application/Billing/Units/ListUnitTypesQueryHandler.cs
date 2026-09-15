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
            query.Search?.Trim().Length > 200 ||
            query.Code?.Trim().Length > 32 ||
            query.DisplayName?.Trim().Length > 200 ||
            query.Currency?.Trim().Length > 3 ||
            query.UnitPrice is <= 0 or > 9999999999999999.99m)
        {
            throw new ArgumentException("Invalid unit type listing request.", nameof(query));
        }

        return types.ListAsync(query with
        {
            Search = Normalize(query.Search),
            Code = Normalize(query.Code),
            DisplayName = Normalize(query.DisplayName),
            Currency = Normalize(query.Currency)?.ToUpperInvariant()
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

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
