using Mdaresna.Platform.Domain.Common;
using Mdaresna.Platform.Domain.Registry.Events;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Domain.Registry;

public sealed class SchoolRegistration : AggregateRoot
{
    private SchoolRegistration(
        SchoolId id,
        TenantId tenantId,
        Guid registrationRequestId,
        SchoolCode code,
        string displayName,
        SchoolType schoolType,
        DeploymentMode deploymentMode,
        SchoolLifecycleStatus status,
        Guid requestedByAccountId,
        Guid? provisioningOperationId,
        string? statusReason,
        string? address,
        Guid? unitTypeId,
        string? primaryPhone,
        DateTimeOffset? activatedAtUtc,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        long version)
    {
        Id = id;
        TenantId = tenantId;
        RegistrationRequestId = registrationRequestId;
        Code = code;
        DisplayName = displayName;
        SchoolType = schoolType;
        DeploymentMode = deploymentMode;
        Status = status;
        RequestedByAccountId = requestedByAccountId;
        ProvisioningOperationId = provisioningOperationId;
        StatusReason = statusReason;
        Address = address;
        UnitTypeId = unitTypeId;
        PrimaryPhone = primaryPhone;
        ActivatedAtUtc = activatedAtUtc;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        RestoreVersion(version);
    }

    public SchoolId Id { get; }

    public TenantId TenantId { get; }

    public Guid RegistrationRequestId { get; }

    public SchoolCode Code { get; }

    public string DisplayName { get; private set; }

    public SchoolType SchoolType { get; }

    public DeploymentMode DeploymentMode { get; private set; }

    public SchoolLifecycleStatus Status { get; private set; }

    public Guid RequestedByAccountId { get; }

    public Guid? ProvisioningOperationId { get; private set; }

    public string? StatusReason { get; private set; }

    public string? Address { get; private set; }

    public Guid? UnitTypeId { get; private set; }

    public string? PrimaryPhone { get; private set; }

    public DateTimeOffset? ActivatedAtUtc { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static SchoolRegistration Create(
        SchoolId id,
        TenantId tenantId,
        Guid registrationRequestId,
        SchoolCode code,
        string displayName,
        SchoolType schoolType,
        DeploymentMode deploymentMode,
        Guid requestedByAccountId,
        DateTimeOffset occurredAtUtc,
        string? address = null,
        Guid? unitTypeId = null,
        string? primaryPhone = null)
    {
        if (id.IsEmpty)
        {
            throw new ArgumentException("SchoolId cannot be empty.", nameof(id));
        }

        if (tenantId.IsEmpty)
        {
            throw new ArgumentException("TenantId cannot be empty.", nameof(tenantId));
        }

        EnsureSchoolIsTenant(id, tenantId);

        if (string.IsNullOrWhiteSpace(code.Value))
        {
            throw new ArgumentException("SchoolCode cannot be empty.", nameof(code));
        }

        DomainGuard.NonEmptyGuid(registrationRequestId, nameof(registrationRequestId));
        DomainGuard.NonEmptyGuid(requestedByAccountId, nameof(requestedByAccountId));
        EnsureDefined(schoolType, nameof(schoolType));
        EnsureDefined(deploymentMode, nameof(deploymentMode));
        EnsureDeploymentMatchesSchoolType(schoolType, deploymentMode);
        var timestamp = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        EnsureOptionalId(unitTypeId, nameof(unitTypeId));

        var registration = new SchoolRegistration(
            id,
            tenantId,
            registrationRequestId,
            code,
            DomainGuard.RequiredText(displayName, 200, nameof(displayName)),
            schoolType,
            deploymentMode,
            SchoolLifecycleStatus.Draft,
            requestedByAccountId,
            provisioningOperationId: null,
            statusReason: null,
            DomainGuard.OptionalText(address, 500, nameof(address)),
            unitTypeId,
            NormalizeOptionalPhone(primaryPhone),
            activatedAtUtc: null,
            timestamp,
            timestamp,
            version: 0);

        registration.Raise(new SchoolRegistrationCreatedDomainEvent(
            Guid.NewGuid(),
            timestamp,
            registrationRequestId,
            tenantId,
            id,
            code));

        return registration;
    }

    public void Rename(string displayName, DateTimeOffset occurredAtUtc)
    {
        EnsureNotClosed();
        DisplayName = DomainGuard.RequiredText(displayName, 200, nameof(displayName));
        UpdatedAtUtc = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        MarkChanged();
    }

    public void ChangeDeploymentMode(DeploymentMode deploymentMode, DateTimeOffset occurredAtUtc)
    {
        if (Status is not (SchoolLifecycleStatus.Draft or
            SchoolLifecycleStatus.PendingVerification or
            SchoolLifecycleStatus.Approved or
            SchoolLifecycleStatus.ProvisioningFailed))
        {
            throw new PlatformDomainException(
                "school.deployment_locked",
                "Deployment mode cannot be changed in the current school state.");
        }

        EnsureDefined(deploymentMode, nameof(deploymentMode));
        EnsureDeploymentMatchesSchoolType(SchoolType, deploymentMode);
        DeploymentMode = deploymentMode;
        UpdatedAtUtc = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        MarkChanged();
    }

    public void SubmitForVerification(Guid changedByAccountId, DateTimeOffset occurredAtUtc) =>
        TransitionTo(
            SchoolLifecycleStatus.PendingVerification,
            changedByAccountId,
            occurredAtUtc,
            reason: null,
            provisioningOperationId: null,
            SchoolLifecycleStatus.Draft);

    public void Approve(Guid changedByAccountId, DateTimeOffset occurredAtUtc) =>
        TransitionTo(
            SchoolLifecycleStatus.Approved,
            changedByAccountId,
            occurredAtUtc,
            reason: null,
            provisioningOperationId: null,
            SchoolLifecycleStatus.PendingVerification);

    public void BeginProvisioning(
        Guid operationId,
        Guid changedByAccountId,
        DateTimeOffset occurredAtUtc)
    {
        DomainGuard.NonEmptyGuid(operationId, nameof(operationId));
        TransitionTo(
            SchoolLifecycleStatus.Provisioning,
            changedByAccountId,
            occurredAtUtc,
            reason: null,
            operationId,
            SchoolLifecycleStatus.Approved,
            SchoolLifecycleStatus.ProvisioningFailed);
    }

    public void MarkProvisioningFailed(
        Guid operationId,
        string reason,
        Guid changedByAccountId,
        DateTimeOffset occurredAtUtc)
    {
        EnsureCurrentProvisioningOperation(operationId);
        TransitionTo(
            SchoolLifecycleStatus.ProvisioningFailed,
            changedByAccountId,
            occurredAtUtc,
            DomainGuard.RequiredText(reason, 1000, nameof(reason)),
            operationId,
            SchoolLifecycleStatus.Provisioning);
    }

    public void Activate(
        Guid operationId,
        Guid changedByAccountId,
        DateTimeOffset occurredAtUtc)
    {
        EnsureCurrentProvisioningOperation(operationId);
        var activatedAt = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        TransitionTo(
            SchoolLifecycleStatus.Active,
            changedByAccountId,
            activatedAt,
            reason: null,
            operationId,
            SchoolLifecycleStatus.Provisioning);
        ActivatedAtUtc ??= activatedAt;
    }

    public void Suspend(string reason, Guid changedByAccountId, DateTimeOffset occurredAtUtc) =>
        TransitionTo(
            SchoolLifecycleStatus.Suspended,
            changedByAccountId,
            occurredAtUtc,
            DomainGuard.RequiredText(reason, 1000, nameof(reason)),
            ProvisioningOperationId,
            SchoolLifecycleStatus.Active);

    public void Reinstate(Guid changedByAccountId, DateTimeOffset occurredAtUtc) =>
        TransitionTo(
            SchoolLifecycleStatus.Active,
            changedByAccountId,
            occurredAtUtc,
            reason: null,
            ProvisioningOperationId,
            SchoolLifecycleStatus.Suspended);

    public void Close(string reason, Guid changedByAccountId, DateTimeOffset occurredAtUtc)
    {
        EnsureNotClosed();
        TransitionTo(
            SchoolLifecycleStatus.Closed,
            changedByAccountId,
            occurredAtUtc,
            DomainGuard.RequiredText(reason, 1000, nameof(reason)),
            ProvisioningOperationId,
            Status);
    }

    internal static SchoolRegistration Rehydrate(
        SchoolId id,
        TenantId tenantId,
        Guid registrationRequestId,
        string schoolCode,
        string displayName,
        SchoolType schoolType,
        DeploymentMode deploymentMode,
        SchoolLifecycleStatus status,
        Guid requestedByAccountId,
        Guid? provisioningOperationId,
        string? statusReason,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        long version,
        string? address = null,
        Guid? unitTypeId = null,
        string? primaryPhone = null,
        DateTimeOffset? activatedAtUtc = null)
    {
        if (id.IsEmpty || tenantId.IsEmpty)
        {
            throw new ArgumentException("School and tenant identifiers cannot be empty.");
        }

        EnsureSchoolIsTenant(id, tenantId);

        EnsureDefined(schoolType, nameof(schoolType));
        EnsureDefined(deploymentMode, nameof(deploymentMode));
        EnsureDefined(status, nameof(status));
        EnsureDeploymentMatchesSchoolType(schoolType, deploymentMode);

        if (provisioningOperationId == Guid.Empty)
        {
            throw new ArgumentException(
                "ProvisioningOperationId cannot be empty when supplied.",
                nameof(provisioningOperationId));
        }

        if (status is SchoolLifecycleStatus.Provisioning or
            SchoolLifecycleStatus.ProvisioningFailed or
            SchoolLifecycleStatus.Active or
            SchoolLifecycleStatus.Suspended && !provisioningOperationId.HasValue)
        {
            throw new ArgumentException(
                "The persisted lifecycle state requires a provisioning operation.",
                nameof(provisioningOperationId));
        }

        var createdAt = DomainGuard.UtcTimestamp(createdAtUtc, nameof(createdAtUtc));
        var updatedAt = DomainGuard.UtcTimestamp(updatedAtUtc, nameof(updatedAtUtc));
        DateTimeOffset? activatedAt = activatedAtUtc.HasValue
            ? DomainGuard.UtcTimestamp(activatedAtUtc.Value, nameof(activatedAtUtc))
            : null;
        EnsureOptionalId(unitTypeId, nameof(unitTypeId));

        if (updatedAt < createdAt)
        {
            throw new ArgumentException("UpdatedAtUtc cannot precede CreatedAtUtc.");
        }

        if (activatedAt < createdAt)
        {
            throw new ArgumentException("ActivatedAtUtc cannot precede CreatedAtUtc.");
        }

        return new SchoolRegistration(
            id,
            tenantId,
            DomainGuard.NonEmptyGuid(registrationRequestId, nameof(registrationRequestId)),
            SchoolCode.Create(schoolCode),
            DomainGuard.RequiredText(displayName, 200, nameof(displayName)),
            schoolType,
            deploymentMode,
            status,
            DomainGuard.NonEmptyGuid(requestedByAccountId, nameof(requestedByAccountId)),
            provisioningOperationId,
            DomainGuard.OptionalText(statusReason, 1000, nameof(statusReason)),
            DomainGuard.OptionalText(address, 500, nameof(address)),
            unitTypeId,
            NormalizeOptionalPhone(primaryPhone),
            activatedAt,
            createdAt,
            updatedAt,
            version);
    }

    private void TransitionTo(
        SchoolLifecycleStatus next,
        Guid changedByAccountId,
        DateTimeOffset occurredAtUtc,
        string? reason,
        Guid? provisioningOperationId,
        params SchoolLifecycleStatus[] allowedCurrentStates)
    {
        DomainGuard.NonEmptyGuid(changedByAccountId, nameof(changedByAccountId));

        if (!allowedCurrentStates.Contains(Status))
        {
            throw new PlatformDomainException(
                "school.invalid_status_transition",
                $"School cannot transition from {Status} to {next}.");
        }

        var timestamp = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        var previous = Status;
        Status = next;
        ProvisioningOperationId = provisioningOperationId;
        StatusReason = reason;
        UpdatedAtUtc = timestamp;
        Raise(new SchoolLifecycleChangedDomainEvent(
            Guid.NewGuid(),
            timestamp,
            TenantId,
            Id,
            previous,
            next,
            changedByAccountId,
            provisioningOperationId,
            reason));
    }

    private void EnsureCurrentProvisioningOperation(Guid operationId)
    {
        DomainGuard.NonEmptyGuid(operationId, nameof(operationId));

        if (ProvisioningOperationId != operationId)
        {
            throw new PlatformDomainException(
                "school.provisioning_operation_mismatch",
                "The result does not match the current provisioning operation.");
        }
    }

    private void EnsureNotClosed()
    {
        if (Status == SchoolLifecycleStatus.Closed)
        {
            throw new PlatformDomainException(
                "school.closed",
                "A closed school registration cannot be changed.");
        }
    }

    private static void EnsureDefined<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Unknown enum value.");
        }
    }

    private static void EnsureOptionalId(Guid? value, string parameterName)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException($"{parameterName} cannot be empty when supplied.", parameterName);
        }
    }

    private static string? NormalizeOptionalPhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var phone = value.Trim();
        return phone.Length is >= 8 and <= 16 && phone.All(char.IsAsciiDigit)
            ? phone
            : throw new ArgumentException("Primary phone must contain 8-16 ASCII digits.", nameof(value));
    }

    private static void EnsureDeploymentMatchesSchoolType(
        SchoolType schoolType,
        DeploymentMode deploymentMode)
    {
        if (deploymentMode == DeploymentMode.GovernmentOnPremises &&
            schoolType != SchoolType.Government)
        {
            throw new PlatformDomainException(
                "school.invalid_deployment_mode",
                "Government on-premises deployment is available only to government schools.");
        }
    }

    private static void EnsureSchoolIsTenant(SchoolId schoolId, TenantId tenantId)
    {
        if (schoolId.Value != tenantId.Value)
        {
            throw new PlatformDomainException(
                "school.tenant_id_mismatch",
                "A school must use its tenant identifier as its school identifier.");
        }
    }
}
