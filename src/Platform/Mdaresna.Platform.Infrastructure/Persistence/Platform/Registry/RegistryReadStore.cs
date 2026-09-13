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

        var page = await schools
            .OrderBy(school => school.DisplayName)
            .ThenBy(school => school.Id)
            .Skip((int)skip)
            .Take(query.PageSize)
            .Select(school => new SchoolReadModel(
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
                school.Version))
            .ToArrayAsync(cancellationToken);

        return new RegistryPage<SchoolReadModel>(page, totalCount, query.PageNumber, query.PageSize);
    }

    public Task<SchoolReadModel?> FindSchoolAsync(
        SchoolId schoolId,
        CancellationToken cancellationToken = default) =>
        dbContext.Schools.AsNoTracking()
            .Where(school => school.Id == schoolId)
            .Select(school => new SchoolReadModel(
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
                school.Version))
            .SingleOrDefaultAsync(cancellationToken);

    private static string ToPostgreSqlContainsPattern(string value) =>
        $"%{value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal)}%";
}
