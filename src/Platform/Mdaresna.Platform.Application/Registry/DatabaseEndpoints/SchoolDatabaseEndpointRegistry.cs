using System.Text.Json;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.SharedKernel.Time;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.DatabaseEndpoints;

public sealed class SchoolDatabaseEndpointRegistry(
    ISchoolDatabaseEndpointRepository endpoints,
    ISchoolRegistrationRepository schools,
    IPlatformRegistryAuditWriter audit,
    IPlatformUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<IReadOnlyList<SchoolDatabaseEndpointReadModel>> ListAsync(
        SchoolId schoolId,
        CancellationToken cancellationToken = default)
    {
        await RequireSchoolAsync(schoolId, cancellationToken);
        return (await endpoints.ListAsync(schoolId, cancellationToken))
            .Select(endpoint => endpoint.ToReadModel())
            .ToArray();
    }

    public async Task<SchoolDatabaseEndpointReadModel> RegisterAsync(
        RegisterSchoolDatabaseEndpointCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommon(command.EndpointId, command.SchoolId,
            command.RequestedByAccountId.IsEmpty, command.CorrelationId);
        var school = await RequireSchoolAsync(command.SchoolId, cancellationToken);
        if (await endpoints.FindByIdAsync(command.EndpointId, cancellationToken) is not null)
        {
            throw new PlatformConflictException(
                "school_database_endpoint.id_conflict",
                "A school database endpoint with this ID already exists.");
        }

        var currentPrimary = await endpoints.FindPrimaryAsync(
            command.SchoolId, command.Purpose, cancellationToken);
        var makePrimary = command.IsPrimary || currentPrimary is null;
        var endpoint = SchoolDatabaseEndpoint.Create(
            command.EndpointId,
            command.SchoolId,
            command.Purpose,
            command.Provider,
            command.Host,
            command.Port,
            command.DatabaseName,
            command.CredentialSecretReference,
            command.RequireTls,
            makePrimary,
            command.Region,
            command.SchemaVersion,
            clock.UtcNow);

        if (await endpoints.TargetExistsAsync(
                command.SchoolId,
                endpoint.Host,
                endpoint.Port,
                endpoint.DatabaseName,
                cancellationToken: cancellationToken))
        {
            throw DuplicateTarget();
        }

        if (makePrimary && currentPrimary is not null)
        {
            currentPrimary.SetPrimary(false, clock.UtcNow);
            await unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
            {
                // Filtered unique indexes are immediate in both supported databases.
                // Persist the demotion before inserting the replacement, atomically.
                await unitOfWork.SaveChangesAsync(transactionToken);
                await endpoints.AddAsync(endpoint, transactionToken);
                StageRegistrationAudit(command, school.TenantId, endpoint);
                await unitOfWork.SaveChangesAsync(transactionToken);
            }, cancellationToken);
        }
        else
        {
            await endpoints.AddAsync(endpoint, cancellationToken);
            StageRegistrationAudit(command, school.TenantId, endpoint);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return endpoint.ToReadModel();
    }

    public async Task<SchoolDatabaseEndpointReadModel> UpdateAsync(
        UpdateSchoolDatabaseEndpointCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommon(command.EndpointId, command.SchoolId,
            command.RequestedByAccountId.IsEmpty, command.CorrelationId);
        EnsureVersion(command.ExpectedVersion);
        var school = await RequireSchoolAsync(command.SchoolId, cancellationToken);
        var endpoint = await RequireEndpointAsync(
            command.EndpointId, command.SchoolId, cancellationToken);
        EnsureVersion(endpoint, command.ExpectedVersion);
        var changed = endpoint.UpdateTarget(
            command.Host,
            command.Port,
            command.DatabaseName,
            command.CredentialSecretReference,
            command.RequireTls,
            command.Region,
            command.SchemaVersion,
            clock.UtcNow);

        if (changed && await endpoints.TargetExistsAsync(
                command.SchoolId,
                endpoint.Host,
                endpoint.Port,
                endpoint.DatabaseName,
                endpoint.Id,
                cancellationToken))
        {
            throw DuplicateTarget();
        }

        if (changed)
        {
            StageAudit(
                command.RequestedByAccountId,
                school.TenantId,
                "platform.school-database-endpoint.update",
                endpoint,
                command.CorrelationId,
                new
                {
                    endpoint.Host,
                    endpoint.Port,
                    endpoint.DatabaseName,
                    endpoint.RequireTls,
                    endpoint.Region,
                    endpoint.SchemaVersion
                });
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return endpoint.ToReadModel();
    }

    public async Task<SchoolDatabaseEndpointReadModel> ChangeStatusAsync(
        ChangeSchoolDatabaseEndpointStatusCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommon(command.EndpointId, command.SchoolId,
            command.RequestedByAccountId.IsEmpty, command.CorrelationId);
        EnsureVersion(command.ExpectedVersion);
        if (!Enum.IsDefined(command.Status))
        {
            throw new ArgumentOutOfRangeException(nameof(command.Status));
        }

        var school = await RequireSchoolAsync(command.SchoolId, cancellationToken);
        var endpoint = await RequireEndpointAsync(
            command.EndpointId, command.SchoolId, cancellationToken);
        EnsureVersion(endpoint, command.ExpectedVersion);
        var previousStatus = endpoint.Status;
        if (endpoint.ChangeStatus(command.Status, clock.UtcNow))
        {
            StageAudit(
                command.RequestedByAccountId,
                school.TenantId,
                "platform.school-database-endpoint.status-changed",
                endpoint,
                command.CorrelationId,
                new { previousStatus, currentStatus = endpoint.Status, endpoint.IsPrimary });
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return endpoint.ToReadModel();
    }

    public async Task<SchoolDatabaseEndpointReadModel> MakePrimaryAsync(
        MakeSchoolDatabaseEndpointPrimaryCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateCommon(command.EndpointId, command.SchoolId,
            command.RequestedByAccountId.IsEmpty, command.CorrelationId);
        EnsureVersion(command.ExpectedVersion);
        var school = await RequireSchoolAsync(command.SchoolId, cancellationToken);
        var endpoint = await RequireEndpointAsync(
            command.EndpointId, command.SchoolId, cancellationToken);
        EnsureVersion(endpoint, command.ExpectedVersion);
        if (endpoint.Status != SchoolDatabaseEndpointStatus.Active)
        {
            throw new PlatformConflictException(
                "school_database_endpoint.not_active",
                "Only an active school database endpoint can become primary.");
        }

        var currentPrimary = await endpoints.FindPrimaryAsync(
            endpoint.SchoolId, endpoint.Purpose, cancellationToken);
        if (currentPrimary?.Id == endpoint.Id)
        {
            return endpoint.ToReadModel();
        }

        if (currentPrimary is not null)
        {
            currentPrimary.SetPrimary(false, clock.UtcNow);
            await unitOfWork.ExecuteInTransactionAsync(async transactionToken =>
            {
                await unitOfWork.SaveChangesAsync(transactionToken);
                endpoint.SetPrimary(true, clock.UtcNow);
                StagePrimaryAudit(command, school.TenantId, endpoint, currentPrimary.Id);
                await unitOfWork.SaveChangesAsync(transactionToken);
            }, cancellationToken);
        }
        else
        {
            endpoint.SetPrimary(true, clock.UtcNow);
            StagePrimaryAudit(command, school.TenantId, endpoint, previousEndpointId: null);
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return endpoint.ToReadModel();
    }

    private void StageRegistrationAudit(
        RegisterSchoolDatabaseEndpointCommand command,
        TenantId tenantId,
        SchoolDatabaseEndpoint endpoint) => StageAudit(
        command.RequestedByAccountId,
        tenantId,
        "platform.school-database-endpoint.register",
        endpoint,
        command.CorrelationId,
        new
        {
            endpoint.Purpose,
            endpoint.Provider,
            endpoint.Host,
            endpoint.Port,
            endpoint.DatabaseName,
            endpoint.RequireTls,
            endpoint.IsPrimary,
            endpoint.Status,
            endpoint.Region,
            endpoint.SchemaVersion
        });

    private void StagePrimaryAudit(
        MakeSchoolDatabaseEndpointPrimaryCommand command,
        TenantId tenantId,
        SchoolDatabaseEndpoint endpoint,
        Guid? previousEndpointId) => StageAudit(
        command.RequestedByAccountId,
        tenantId,
        "platform.school-database-endpoint.primary-changed",
        endpoint,
        command.CorrelationId,
        new { previousEndpointId, currentEndpointId = endpoint.Id });

    private async Task<SchoolRegistration> RequireSchoolAsync(
        SchoolId schoolId,
        CancellationToken cancellationToken)
    {
        if (schoolId.IsEmpty)
        {
            throw new ArgumentException("SchoolId cannot be empty.", nameof(schoolId));
        }

        return await schools.FindByIdAsync(schoolId, cancellationToken)
            ?? throw new PlatformResourceNotFoundException(
                "school.not_found",
                "School was not found.");
    }

    private async Task<SchoolDatabaseEndpoint> RequireEndpointAsync(
        Guid endpointId,
        SchoolId schoolId,
        CancellationToken cancellationToken)
    {
        var endpoint = await endpoints.FindByIdAsync(endpointId, cancellationToken);
        if (endpoint is null || endpoint.SchoolId != schoolId)
        {
            throw new PlatformResourceNotFoundException(
                "school_database_endpoint.not_found",
                "School database endpoint was not found.");
        }

        return endpoint;
    }

    private void StageAudit(
        IdentityAccountId actorId,
        TenantId tenantId,
        string action,
        SchoolDatabaseEndpoint endpoint,
        Guid correlationId,
        object metadata)
    {
        audit.Stage(new RegistryAuditRecord(
            Guid.NewGuid(),
            actorId,
            tenantId,
            action,
            "school-database-endpoint",
            endpoint.Id.ToString("D"),
            clock.UtcNow,
            correlationId,
            JsonSerializer.Serialize(metadata)));
    }

    private static PlatformConflictException DuplicateTarget() => new(
        "school_database_endpoint.target_conflict",
        "This school database target is already registered.");

    private static void EnsureVersion(SchoolDatabaseEndpoint endpoint, long expectedVersion)
    {
        if (endpoint.Version != expectedVersion)
        {
            throw new PlatformConflictException(
                "school_database_endpoint.version_conflict",
                "School database endpoint changed since it was last read.");
        }
    }

    private static void EnsureVersion(long expectedVersion)
    {
        if (expectedVersion < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(expectedVersion));
        }
    }

    private static void ValidateCommon(
        Guid endpointId,
        SchoolId schoolId,
        bool actorIdIsEmpty,
        Guid correlationId)
    {
        if (endpointId == Guid.Empty || schoolId.IsEmpty || actorIdIsEmpty ||
            correlationId == Guid.Empty)
        {
            throw new ArgumentException("School database endpoint command is invalid.");
        }
    }
}
