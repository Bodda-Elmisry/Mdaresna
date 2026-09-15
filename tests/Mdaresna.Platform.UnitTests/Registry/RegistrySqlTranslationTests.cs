using Mdaresna.Platform.Application.Registry.Read;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.UnitTests.Registry;

public sealed class RegistrySqlTranslationTests
{
    [Fact]
    public void School_list_query_translates_for_sql_server()
    {
        using var db = CreateDbContext();
        var code = SchoolCode.Create("ABC-123");
        var search = "ABC-123";

        var sql = db.Schools.AsNoTracking()
            .Where(school => school.DisplayName.Contains(search) || school.Code == code)
            .OrderBy(school => school.DisplayName)
            .ThenBy(school => school.Id)
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
                school.Version,
                school.Address,
                school.ActivatedAtUtc,
                school.UnitTypeId,
                null,
                null,
                null,
                null))
            .ToQueryString();

        Assert.Contains("[registry].[schools]", sql);
    }

    [Fact]
    public void Tenant_list_query_translates_for_sql_server()
    {
        using var db = CreateDbContext();
        var search = "School";

        var sql = db.Tenants.AsNoTracking()
            .Where(tenant => tenant.DisplayName.Contains(search) ||
                tenant.LegalName != null && tenant.LegalName.Contains(search))
            .OrderBy(tenant => tenant.DisplayName)
            .ThenBy(tenant => tenant.Id)
            .Select(tenant => new TenantReadModel(
                tenant.Id,
                tenant.DisplayName,
                tenant.LegalName,
                tenant.Status,
                tenant.SuspensionReason,
                tenant.CreatedAtUtc,
                tenant.UpdatedAtUtc,
                tenant.Version))
            .ToQueryString();

        Assert.Contains("[registry].[tenants]", sql);
    }

    [Fact]
    public void School_database_endpoint_primary_lookup_translates_for_sql_server()
    {
        using var db = CreateDbContext();
        var schoolId = SchoolId.New();

        var sql = db.SchoolDatabaseEndpoints.AsNoTracking()
            .Where(endpoint => endpoint.SchoolId == schoolId &&
                endpoint.Purpose == SchoolDatabasePurpose.Operational &&
                endpoint.IsPrimary &&
                endpoint.Status == SchoolDatabaseEndpointStatus.Active)
            .ToQueryString();

        Assert.Contains("[registry].[school_database_endpoints]", sql);
    }

    private static PlatformDbContext CreateDbContext() => new(
        new DbContextOptionsBuilder<PlatformDbContext>()
            .UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=PlatformSqlTranslationOnly;Trusted_Connection=True")
            .Options);
}
