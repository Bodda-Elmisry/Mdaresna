using Mdaresna.Platform.Application.Registry.Read;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Registry;

internal sealed class RegistryReadStore(PlatformDbContext dbContext) : IRegistryReadStore
{
    public async Task<RegistryPage<TenantReadModel>> ListTenantsAsync(
        ListTenantsQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Tenant> tenants = dbContext.Tenants.AsNoTracking();
        if (query.Status is { } status)
        {
            tenants = tenants.Where(tenant => tenant.Status == status);
        }

        if (query.Search is { } search)
        {
            if (dbContext.Database.IsNpgsql())
            {
                var pattern = ToPostgreSqlContainsPattern(search);
                tenants = tenants.Where(tenant => EF.Functions.ILike(tenant.DisplayName, pattern, "\\") ||
                    tenant.LegalName != null && EF.Functions.ILike(tenant.LegalName, pattern, "\\"));
            }
            else
            {
                tenants = tenants.Where(tenant => tenant.DisplayName.Contains(search) ||
                    tenant.LegalName != null && tenant.LegalName.Contains(search));
            }
        }

        var totalCount = await tenants.CountAsync(cancellationToken);
        var skip = (long)(query.PageNumber - 1) * query.PageSize;
        if (skip > int.MaxValue)
        {
            return new RegistryPage<TenantReadModel>([], totalCount, query.PageNumber, query.PageSize);
        }

        var page = await tenants
            .OrderBy(tenant => tenant.DisplayName)
            .ThenBy(tenant => tenant.Id)
            .Skip((int)skip)
            .Take(query.PageSize)
            .Select(tenant => new TenantReadModel(
                tenant.Id,
                tenant.DisplayName,
                tenant.LegalName,
                tenant.Status,
                tenant.SuspensionReason,
                tenant.CreatedAtUtc,
                tenant.UpdatedAtUtc,
                tenant.Version))
            .ToArrayAsync(cancellationToken);

        return new RegistryPage<TenantReadModel>(page, totalCount, query.PageNumber, query.PageSize);
    }

    public Task<TenantReadModel?> FindTenantAsync(
        TenantId tenantId,
        CancellationToken cancellationToken = default) =>
        dbContext.Tenants.AsNoTracking()
            .Where(tenant => tenant.Id == tenantId)
            .Select(tenant => new TenantReadModel(
                tenant.Id,
                tenant.DisplayName,
                tenant.LegalName,
                tenant.Status,
                tenant.SuspensionReason,
                tenant.CreatedAtUtc,
                tenant.UpdatedAtUtc,
                tenant.Version))
            .SingleOrDefaultAsync(cancellationToken);

    public async Task<RegistryPage<SchoolReadModel>> ListSchoolsAsync(
        ListSchoolsQuery query,
        CancellationToken cancellationToken = default)
    {
        IQueryable<SchoolRegistration> schools = dbContext.Schools.AsNoTracking();
        if (query.TenantId is { } tenantId)
        {
            schools = schools.Where(school => school.TenantId == tenantId);
        }

        if (query.Status is { } status)
        {
            schools = schools.Where(school => school.Status == status);
        }

        if (query.SchoolType is { } schoolType)
        {
            schools = schools.Where(school => school.SchoolType == schoolType);
        }

        if (query.UnitTypeId is { } unitTypeId)
        {
            schools = schools.Where(school => school.UnitTypeId == unitTypeId);
        }

        if (query.UnitType is { } unitTypeSearch)
        {
            if (dbContext.Database.IsNpgsql())
            {
                var pattern = ToPostgreSqlContainsPattern(unitTypeSearch);
                schools = schools.Where(school => school.UnitTypeId != null &&
                    dbContext.UnitTypes.Any(unitType => unitType.Id == school.UnitTypeId &&
                        (EF.Functions.ILike(unitType.Code, pattern, "\\") ||
                         EF.Functions.ILike(unitType.DisplayName, pattern, "\\") ||
                         EF.Functions.ILike(unitType.Currency, pattern, "\\"))));
            }
            else
            {
                schools = schools.Where(school => school.UnitTypeId != null &&
                    dbContext.UnitTypes.Any(unitType => unitType.Id == school.UnitTypeId &&
                        (unitType.Code.Contains(unitTypeSearch) ||
                         unitType.DisplayName.Contains(unitTypeSearch) ||
                         unitType.Currency.Contains(unitTypeSearch))));
            }
        }

        if (query.OwnerAccountIds is { } ownerIds)
        {
            schools = ownerIds.Count == 0
                ? schools.Where(_ => false)
                : schools.Where(school => ownerIds.Contains(school.RequestedByAccountId));
        }

        schools = ApplySchoolNameFilter(schools, query.DisplayName);
        schools = ApplySchoolAddressFilter(schools, query.Address);

        if (query.CreatedFrom is { } createdFrom)
        {
            var start = UtcStart(createdFrom);
            schools = schools.Where(school => school.CreatedAtUtc >= start);
        }

        if (query.CreatedTo is { } createdTo)
        {
            var end = UtcStart(createdTo.AddDays(1));
            schools = schools.Where(school => school.CreatedAtUtc < end);
        }

        if (query.ActivatedFrom is { } activatedFrom)
        {
            var start = UtcStart(activatedFrom);
            schools = schools.Where(school => school.ActivatedAtUtc >= start);
        }

        if (query.ActivatedTo is { } activatedTo)
        {
            var end = UtcStart(activatedTo.AddDays(1));
            schools = schools.Where(school => school.ActivatedAtUtc < end);
        }

        if (query.Search is { } search)
        {
            SchoolCode? code = null;
            try
            {
                code = SchoolCode.Create(search);
            }
            catch (ArgumentException)
            {
                // Free-text school-name searches need not be valid school codes.
            }

            if (dbContext.Database.IsNpgsql())
            {
                var pattern = ToPostgreSqlContainsPattern(search);
                schools = code is { } schoolCode
                    ? schools.Where(school => EF.Functions.ILike(school.DisplayName, pattern, "\\") || school.Code == schoolCode)
                    : schools.Where(school => EF.Functions.ILike(school.DisplayName, pattern, "\\"));
            }
            else
            {
                schools = code is { } schoolCode
                    ? schools.Where(school => school.DisplayName.Contains(search) || school.Code == schoolCode)
                    : schools.Where(school => school.DisplayName.Contains(search));
            }
        }

        var totalCount = await schools.CountAsync(cancellationToken);
        var skip = (long)(query.PageNumber - 1) * query.PageSize;
        if (skip > int.MaxValue)
        {
            return new RegistryPage<SchoolReadModel>([], totalCount, query.PageNumber, query.PageSize);
        }

        var page = await (from school in schools
            join unitType in dbContext.UnitTypes.AsNoTracking()
                on school.UnitTypeId equals (Guid?)unitType.Id into unitTypes
            from unitType in unitTypes.DefaultIfEmpty()
            orderby school.DisplayName, school.Id
            select new SchoolReadModel(
                school.Id,
                school.TenantId,
                school.RegistrationRequestId,
                school.Code.Value,
                school.DisplayName,
                school.SchoolType,
                school.DeploymentMode,
                school.Status,
                school.RequestedByAccountId,
                school.ProvisioningOperationId,
                school.StatusReason,
                school.CreatedAtUtc,
                school.UpdatedAtUtc,
                school.Version,
                school.Address,
                school.ActivatedAtUtc,
                school.UnitTypeId,
                unitType == null ? null : unitType.Code,
                unitType == null ? null : unitType.DisplayName,
                unitType == null ? null : unitType.Currency,
                null))
            .Skip((int)skip)
            .Take(query.PageSize)
            .ToArrayAsync(cancellationToken);

        return new RegistryPage<SchoolReadModel>(page, totalCount, query.PageNumber, query.PageSize);
    }

    public async Task<SchoolDirectorySummary> GetSchoolSummaryAsync(
        CancellationToken cancellationToken = default)
    {
        var groups = await dbContext.Schools.AsNoTracking()
            .GroupBy(school => new { school.Status, school.SchoolType })
            .Select(group => new { group.Key.Status, group.Key.SchoolType, Count = group.Count() })
            .ToArrayAsync(cancellationToken);
        var statuses = groups.GroupBy(group => group.Status)
            .OrderBy(group => group.Key)
            .Select(group => new SchoolStatusSummary(
                group.Key,
                group.Sum(item => item.Count),
                group.OrderBy(item => item.SchoolType)
                    .Select(item => new SchoolTypeSummary(item.SchoolType, item.Count))
                    .ToArray()))
            .ToArray();
        return new SchoolDirectorySummary(groups.Sum(group => group.Count), statuses);
    }

    public Task<SchoolReadModel?> FindSchoolAsync(
        SchoolId schoolId,
        CancellationToken cancellationToken = default) =>
        (from school in dbContext.Schools.AsNoTracking()
            join unitType in dbContext.UnitTypes.AsNoTracking()
                on school.UnitTypeId equals (Guid?)unitType.Id into unitTypes
            from unitType in unitTypes.DefaultIfEmpty()
            where school.Id == schoolId
            select new SchoolReadModel(
                school.Id,
                school.TenantId,
                school.RegistrationRequestId,
                school.Code.Value,
                school.DisplayName,
                school.SchoolType,
                school.DeploymentMode,
                school.Status,
                school.RequestedByAccountId,
                school.ProvisioningOperationId,
                school.StatusReason,
                school.CreatedAtUtc,
                school.UpdatedAtUtc,
                school.Version,
                school.Address,
                school.ActivatedAtUtc,
                school.UnitTypeId,
                unitType == null ? null : unitType.Code,
                unitType == null ? null : unitType.DisplayName,
                unitType == null ? null : unitType.Currency,
                null))
            .SingleOrDefaultAsync(cancellationToken);

    private IQueryable<SchoolRegistration> ApplySchoolNameFilter(
        IQueryable<SchoolRegistration> schools,
        string? value)
    {
        if (value is null) return schools;
        if (dbContext.Database.IsNpgsql())
        {
            var pattern = ToPostgreSqlContainsPattern(value);
            return schools.Where(school => EF.Functions.ILike(school.DisplayName, pattern, "\\"));
        }
        return schools.Where(school => school.DisplayName.Contains(value));
    }

    private IQueryable<SchoolRegistration> ApplySchoolAddressFilter(
        IQueryable<SchoolRegistration> schools,
        string? value)
    {
        if (value is null) return schools;
        if (dbContext.Database.IsNpgsql())
        {
            var pattern = ToPostgreSqlContainsPattern(value);
            return schools.Where(school => school.Address != null &&
                EF.Functions.ILike(school.Address, pattern, "\\"));
        }
        return schools.Where(school => school.Address != null && school.Address.Contains(value));
    }

    private static DateTimeOffset UtcStart(DateOnly date) =>
        new(date.ToDateTime(TimeOnly.MinValue), TimeSpan.Zero);

    private static string ToPostgreSqlContainsPattern(string value) =>
        $"%{value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal)}%";
}
