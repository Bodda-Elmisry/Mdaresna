using System.Text.Json;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Schools.Contracts.Provisioning;
using Mdaresna.SharedKernel.Time;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.DatabaseMigrations;

public sealed record RequestSchoolDatabaseMigrationCommand(
    SchoolId SchoolId,
    IdentityAccountId RequestedByAccountId,
    Guid CorrelationId,
    string? TraceParent = null);

public sealed record SchoolDatabaseMigrationRequestResult(
    Guid OperationId,
    SchoolId SchoolId,
    SchoolDatabaseMigrationStatus Status);

public sealed class RequestSchoolDatabaseMigrationHandler(
    ISchoolRegistrationRepository schools,
    ISchoolDatabaseEndpointRepository endpoints,
    IPlatformOutboxWriter outbox,
    IPlatformRegistryAuditWriter audit,
    IPlatformUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<SchoolDatabaseMigrationRequestResult> HandleAsync(
        RequestSchoolDatabaseMigrationCommand command,
        CancellationToken cancellationToken = default)
    {
        var school = await schools.FindByIdAsync(command.SchoolId, cancellationToken)
            ?? throw new PlatformResourceNotFoundException("school.not_found", "School was not found.");
        if (school.Status != SchoolLifecycleStatus.Active)
            throw new PlatformConflictException("school.not_active", "Only an active school can be migrated.");
        var endpoint = await endpoints.FindPrimaryAsync(
            school.Id, SchoolDatabasePurpose.Operational, cancellationToken)
            ?? throw new PlatformConflictException("school_database_endpoint.not_found",
                "The active school database endpoint was not found.");
        var operationId = Guid.NewGuid();
        Queue(endpoint, school.TenantId, operationId, command.RequestedByAccountId,
            command.CorrelationId, command.TraceParent, clock.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new(operationId, school.Id, endpoint.MigrationStatus);
    }

    public async Task<IReadOnlyList<SchoolDatabaseMigrationRequestResult>> HandleAllAsync(
        IdentityAccountId requestedByAccountId,
        Guid correlationId,
        string? traceParent,
        CancellationToken cancellationToken = default)
    {
        var activeEndpoints = await endpoints.ListActivePrimaryOperationalAsync(cancellationToken);
        var now = clock.UtcNow;
        var results = new List<SchoolDatabaseMigrationRequestResult>(activeEndpoints.Count);
        foreach (var endpoint in activeEndpoints)
        {
            if (endpoint.MigrationStatus == SchoolDatabaseMigrationStatus.Pending) continue;
            var operationId = Guid.NewGuid();
            Queue(endpoint, endpoint.SchoolId.ToTenantId(), operationId, requestedByAccountId,
                correlationId, traceParent, now);
            results.Add(new(operationId, endpoint.SchoolId, endpoint.MigrationStatus));
        }
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return results;
    }

    private void Queue(SchoolDatabaseEndpoint endpoint, TenantId tenantId, Guid operationId,
        IdentityAccountId actorId, Guid correlationId, string? traceParent, DateTimeOffset now)
    {
        endpoint.QueueMigration(operationId, now);
        var command = new MigrateSchoolDatabaseV1(operationId, tenantId, endpoint.SchoolId,
            endpoint.DatabaseName);
        outbox.Enqueue(IntegrationMessageEnvelope<MigrateSchoolDatabaseV1>.Create(
            now, "mdaresna-platform", IntegrationMessageScope.ForSchool(tenantId, endpoint.SchoolId),
            command, new IntegrationAggregateReference("school-database-endpoint", endpoint.Id,
                endpoint.Version), correlationId, operationId, traceParent));
        audit.Stage(new RegistryAuditRecord(Guid.NewGuid(), actorId, tenantId,
            "platform.school.database_migration.requested", "school-database-endpoint",
            endpoint.Id.ToString("D"), now, correlationId,
            JsonSerializer.Serialize(new { operationId, endpoint.DatabaseName })));
    }
}

public sealed class CompleteSchoolDatabaseMigrationHandler(
    ISchoolDatabaseEndpointRepository endpoints,
    IPlatformUnitOfWork unitOfWork)
{
    public async Task HandleAsync(SchoolDatabaseMigratedV1 result,
        CancellationToken cancellationToken = default)
    {
        if (result.TenantId.Value != result.SchoolId.Value)
            throw new ArgumentException("School and tenant identifiers do not match.", nameof(result));
        var endpoint = await endpoints.FindPrimaryAsync(
            result.SchoolId, SchoolDatabasePurpose.Operational, cancellationToken)
            ?? throw new PlatformResourceNotFoundException("school_database_endpoint.not_found",
                "School database endpoint was not found.");
        if (endpoint.LastMigrationOperationId == result.OperationId &&
            endpoint.MigrationStatus != SchoolDatabaseMigrationStatus.Pending) return;
        endpoint.CompleteMigration(result.OperationId, result.Succeeded,
            result.CurrentSchemaVersion, result.Error, result.CompletedAtUtc);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

internal static class SchoolMigrationIdentifierExtensions
{
    public static TenantId ToTenantId(this SchoolId schoolId) => TenantId.From(schoolId.Value);
}
