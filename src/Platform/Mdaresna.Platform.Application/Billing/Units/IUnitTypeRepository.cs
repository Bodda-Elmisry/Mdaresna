using Mdaresna.Platform.Domain.Billing.Units;

namespace Mdaresna.Platform.Application.Billing.Units;

public interface IUnitTypeRepository
{
    Task<UnitType?> FindByIdAsync(Guid unitTypeId, CancellationToken cancellationToken = default);
    Task<UnitType?> FindByCodeAsync(string normalizedCode, CancellationToken cancellationToken = default);
    Task AddAsync(UnitType unitType, CancellationToken cancellationToken = default);
    Task<UnitTypePage> ListAsync(UnitTypeListQuery query, CancellationToken cancellationToken = default);
}

public sealed record UnitTypeListQuery(
    string? Search = null,
    bool? IsActive = null,
    int PageNumber = 1,
    int PageSize = 20);

public sealed record UnitTypeReadModel(
    Guid Id,
    string Code,
    string DisplayName,
    decimal UnitPrice,
    string Currency,
    bool IsActive,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    long Version);

public sealed record UnitTypePage(
    IReadOnlyList<UnitTypeReadModel> Items,
    int TotalCount,
    int PageNumber,
    int PageSize);
