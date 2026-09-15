using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Abstractions.Persistence;

public interface ISchoolDatabaseEndpointRepository
{
    Task<SchoolDatabaseEndpoint?> FindByIdAsync(
        Guid endpointId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SchoolDatabaseEndpoint>> ListAsync(
        SchoolId schoolId,
        CancellationToken cancellationToken = default);

    Task<SchoolDatabaseEndpoint?> FindPrimaryActiveAsync(
        SchoolId schoolId,
        SchoolDatabasePurpose purpose,
        CancellationToken cancellationToken = default);

    Task<SchoolDatabaseEndpoint?> FindPrimaryAsync(
        SchoolId schoolId,
        SchoolDatabasePurpose purpose,
        CancellationToken cancellationToken = default);

    Task<bool> TargetExistsAsync(
        SchoolId schoolId,
        string host,
        int port,
        string databaseName,
        Guid? excludingEndpointId = null,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        SchoolDatabaseEndpoint endpoint,
        CancellationToken cancellationToken = default);
}
