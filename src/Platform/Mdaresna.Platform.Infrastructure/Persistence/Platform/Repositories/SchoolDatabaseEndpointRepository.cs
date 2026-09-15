using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Repositories;

internal sealed class SchoolDatabaseEndpointRepository(PlatformDbContext dbContext) :
    ISchoolDatabaseEndpointRepository
{
    public Task<SchoolDatabaseEndpoint?> FindByIdAsync(
        Guid endpointId,
        CancellationToken cancellationToken = default) =>
        dbContext.SchoolDatabaseEndpoints.SingleOrDefaultAsync(
            endpoint => endpoint.Id == endpointId,
            cancellationToken);

    public async Task<IReadOnlyList<SchoolDatabaseEndpoint>> ListAsync(
        SchoolId schoolId,
        CancellationToken cancellationToken = default) =>
        await dbContext.SchoolDatabaseEndpoints.AsNoTracking()
            .Where(endpoint => endpoint.SchoolId == schoolId)
            .OrderBy(endpoint => endpoint.Purpose)
            .ThenByDescending(endpoint => endpoint.IsPrimary)
            .ThenBy(endpoint => endpoint.CreatedAtUtc)
            .ToArrayAsync(cancellationToken);

    public Task<SchoolDatabaseEndpoint?> FindPrimaryActiveAsync(
        SchoolId schoolId,
        SchoolDatabasePurpose purpose,
        CancellationToken cancellationToken = default) =>
        dbContext.SchoolDatabaseEndpoints.AsNoTracking().SingleOrDefaultAsync(
            endpoint => endpoint.SchoolId == schoolId &&
                        endpoint.Purpose == purpose &&
                        endpoint.IsPrimary &&
                        endpoint.Status == SchoolDatabaseEndpointStatus.Active,
            cancellationToken);

    public Task<SchoolDatabaseEndpoint?> FindPrimaryAsync(
        SchoolId schoolId,
        SchoolDatabasePurpose purpose,
        CancellationToken cancellationToken = default) =>
        dbContext.SchoolDatabaseEndpoints.SingleOrDefaultAsync(
            endpoint => endpoint.SchoolId == schoolId &&
                        endpoint.Purpose == purpose &&
                        endpoint.IsPrimary,
            cancellationToken);

    public Task<bool> TargetExistsAsync(
        SchoolId schoolId,
        string host,
        int port,
        string databaseName,
        Guid? excludingEndpointId = null,
        CancellationToken cancellationToken = default) =>
        dbContext.SchoolDatabaseEndpoints.AnyAsync(
            endpoint => endpoint.SchoolId == schoolId &&
                        endpoint.Host == host &&
                        endpoint.Port == port &&
                        endpoint.DatabaseName == databaseName &&
                        (!excludingEndpointId.HasValue || endpoint.Id != excludingEndpointId.Value),
            cancellationToken);

    public Task AddAsync(
        SchoolDatabaseEndpoint endpoint,
        CancellationToken cancellationToken = default) =>
        dbContext.SchoolDatabaseEndpoints.AddAsync(endpoint, cancellationToken).AsTask();
}
