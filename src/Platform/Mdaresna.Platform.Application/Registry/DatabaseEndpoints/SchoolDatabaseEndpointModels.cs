using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.DatabaseEndpoints;

public sealed record SchoolDatabaseEndpointReadModel(
    Guid Id,
    SchoolId SchoolId,
    SchoolDatabasePurpose Purpose,
    SchoolDatabaseProvider Provider,
    string Host,
    int Port,
    string DatabaseName,
    string CredentialSecretReference,
    bool RequireTls,
    bool IsPrimary,
    SchoolDatabaseEndpointStatus Status,
    string? Region,
    string? SchemaVersion,
    SchoolDatabaseMigrationStatus MigrationStatus,
    Guid? LastMigrationOperationId,
    DateTimeOffset? LastMigrationRequestedAtUtc,
    DateTimeOffset? LastMigrationCompletedAtUtc,
    string? LastMigrationError,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    long Version);

public sealed record SchoolDatabaseEndpointTarget(
    Guid EndpointId,
    SchoolId SchoolId,
    SchoolDatabasePurpose Purpose,
    SchoolDatabaseProvider Provider,
    string Host,
    int Port,
    string DatabaseName,
    string CredentialSecretReference,
    bool RequireTls,
    string? Region,
    string? SchemaVersion);

public interface ISchoolDatabaseEndpointResolver
{
    Task<SchoolDatabaseEndpointTarget> ResolvePrimaryAsync(
        SchoolId schoolId,
        SchoolDatabasePurpose purpose = SchoolDatabasePurpose.Operational,
        CancellationToken cancellationToken = default);
}

public sealed record RegisterSchoolDatabaseEndpointCommand(
    Guid EndpointId,
    SchoolId SchoolId,
    SchoolDatabasePurpose Purpose,
    SchoolDatabaseProvider Provider,
    string Host,
    int Port,
    string DatabaseName,
    string CredentialSecretReference,
    bool RequireTls,
    bool IsPrimary,
    string? Region,
    string? SchemaVersion,
    IdentityAccountId RequestedByAccountId,
    Guid CorrelationId);

public sealed record UpdateSchoolDatabaseEndpointCommand(
    Guid EndpointId,
    SchoolId SchoolId,
    long ExpectedVersion,
    string Host,
    int Port,
    string DatabaseName,
    string CredentialSecretReference,
    bool RequireTls,
    string? Region,
    string? SchemaVersion,
    IdentityAccountId RequestedByAccountId,
    Guid CorrelationId);

public sealed record ChangeSchoolDatabaseEndpointStatusCommand(
    Guid EndpointId,
    SchoolId SchoolId,
    long ExpectedVersion,
    SchoolDatabaseEndpointStatus Status,
    IdentityAccountId RequestedByAccountId,
    Guid CorrelationId);

public sealed record MakeSchoolDatabaseEndpointPrimaryCommand(
    Guid EndpointId,
    SchoolId SchoolId,
    long ExpectedVersion,
    IdentityAccountId RequestedByAccountId,
    Guid CorrelationId);

internal static class SchoolDatabaseEndpointMapping
{
    public static SchoolDatabaseEndpointReadModel ToReadModel(this SchoolDatabaseEndpoint endpoint) =>
        new(
            endpoint.Id,
            endpoint.SchoolId,
            endpoint.Purpose,
            endpoint.Provider,
            endpoint.Host,
            endpoint.Port,
            endpoint.DatabaseName,
            endpoint.CredentialSecretReference,
            endpoint.RequireTls,
            endpoint.IsPrimary,
            endpoint.Status,
            endpoint.Region,
            endpoint.SchemaVersion,
            endpoint.MigrationStatus,
            endpoint.LastMigrationOperationId,
            endpoint.LastMigrationRequestedAtUtc,
            endpoint.LastMigrationCompletedAtUtc,
            endpoint.LastMigrationError,
            endpoint.CreatedAtUtc,
            endpoint.UpdatedAtUtc,
            endpoint.Version);
}
