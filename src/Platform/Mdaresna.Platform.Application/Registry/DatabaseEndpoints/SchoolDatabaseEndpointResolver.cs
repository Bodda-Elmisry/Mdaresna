using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.DatabaseEndpoints;

public sealed class SchoolDatabaseEndpointResolver(
    ISchoolDatabaseEndpointRepository endpoints) : ISchoolDatabaseEndpointResolver
{
    public async Task<SchoolDatabaseEndpointTarget> ResolvePrimaryAsync(
        SchoolId schoolId,
        SchoolDatabasePurpose purpose = SchoolDatabasePurpose.Operational,
        CancellationToken cancellationToken = default)
    {
        if (schoolId.IsEmpty || !Enum.IsDefined(purpose))
        {
            throw new ArgumentException("A valid school and database purpose are required.");
        }

        var endpoint = await endpoints.FindPrimaryActiveAsync(
            schoolId, purpose, cancellationToken)
            ?? throw new PlatformResourceNotFoundException(
                "school_database_endpoint.not_available",
                "No active primary database endpoint is available for this school and purpose.");

        return new SchoolDatabaseEndpointTarget(
            endpoint.Id,
            endpoint.SchoolId,
            endpoint.Purpose,
            endpoint.Provider,
            endpoint.Host,
            endpoint.Port,
            endpoint.DatabaseName,
            endpoint.CredentialSecretReference,
            endpoint.RequireTls,
            endpoint.Region,
            endpoint.SchemaVersion);
    }
}
