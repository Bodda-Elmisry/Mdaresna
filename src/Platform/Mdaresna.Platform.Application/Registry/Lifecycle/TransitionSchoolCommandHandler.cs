using System.Text.Json;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Contracts.Registry;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Domain.Registry.Events;
using Mdaresna.SharedKernel.Time;
using Mdaresna.Platform.Application.Billing.Units;
using ProvisionSchoolCommand = Mdaresna.Schools.Contracts.Provisioning.ProvisionSchoolV1;
using Mdaresna.Platform.Domain.Billing.Units;

namespace Mdaresna.Platform.Application.Registry.Lifecycle;

public sealed class TransitionSchoolCommandHandler(
    ISchoolRegistrationRepository schools,
    IUnitTypeRepository unitTypes,
    IPlatformOutboxWriter outbox,
    IPlatformRegistryAuditWriter audit,
    IPlatformUnitOfWork unitOfWork,
    IClock clock)
{
    private const string Producer = "mdaresna-platform";

    public async Task<TransitionSchoolResult> HandleAsync(
        TransitionSchoolCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        Validate(command);

        var school = await schools.FindByIdAsync(command.SchoolId, cancellationToken)
            ?? throw new PlatformResourceNotFoundException(
                "school.not_found",
                $"School '{command.SchoolId}' was not found.");

        if (school.TenantId != command.TenantId)
        {
            throw new PlatformResourceNotFoundException(
                "school.not_found",
                $"School '{command.SchoolId}' was not found in this tenant.");
        }

        if (school.Version != command.ExpectedVersion)
        {
            throw new PlatformConflictException(
                "school.version_conflict",
                "School registration changed since it was last read.");
        }

        var now = clock.UtcNow;
        UnitType? selectedUnitType = null;
        if (command.Action == SchoolLifecycleAction.Approve)
        {
            selectedUnitType = await unitTypes.FindByIdAsync(command.UnitTypeId!.Value, cancellationToken)
                ?? throw new PlatformResourceNotFoundException("unit_type.not_found", "Unit type was not found.");
            if (!selectedUnitType.IsActive)
                throw new PlatformConflictException("unit_type.inactive", "The selected unit type is inactive.");
        }
        var previousEventCount = school.DomainEvents.Count;
        ApplyAction(school, command, now);
        if (command.Action == SchoolLifecycleAction.Approve)
            school.BeginProvisioning(command.CorrelationId, (Guid)command.RequestedByAccountId, now);

        var lifecycleEvents = school.DomainEvents
            .Skip(previousEventCount)
            .OfType<SchoolLifecycleChangedDomainEvent>()
            .ToArray();

        foreach (var lifecycleEvent in lifecycleEvents)
        {
            var integrationEvent = new SchoolLifecycleChangedV1(
                lifecycleEvent.TenantId,
                lifecycleEvent.SchoolId,
                lifecycleEvent.PreviousStatus.ToContract(),
                lifecycleEvent.CurrentStatus.ToContract(),
                lifecycleEvent.ProvisioningOperationId,
                lifecycleEvent.Reason,
                lifecycleEvent.OccurredAtUtc);

            outbox.Enqueue(new IntegrationMessageEnvelope<SchoolLifecycleChangedV1>(
                lifecycleEvent.EventId,
                SchoolLifecycleChangedV1.MessageType,
                SchoolLifecycleChangedV1.SchemaVersion,
                lifecycleEvent.OccurredAtUtc,
                Producer,
                IntegrationMessageScope.ForSchool(school.TenantId, school.Id),
                new IntegrationAggregateReference("school-registration", (Guid)school.Id, school.Version),
                command.CorrelationId,
                command.CausationId,
                command.TraceParent,
                integrationEvent));

            audit.Stage(new RegistryAuditRecord(
                lifecycleEvent.EventId,
                command.RequestedByAccountId,
                school.TenantId,
                lifecycleEvent.CurrentStatus == SchoolLifecycleStatus.Provisioning
                    ? "platform.school.begin_provisioning"
                    : $"platform.school.{command.Action.ToString().ToLowerInvariant()}",
                "school-registration",
                school.Id.ToString(),
                now,
                command.CorrelationId,
                JsonSerializer.Serialize(new
                {
                    previousStatus = lifecycleEvent.PreviousStatus.ToString(),
                    currentStatus = lifecycleEvent.CurrentStatus.ToString(),
                    reason = lifecycleEvent.Reason,
                    unitTypeId = school.UnitTypeId,
                    provisioningOperationId = lifecycleEvent.ProvisioningOperationId
                })));
        }

        if (command.Action == SchoolLifecycleAction.Approve)
        {
            var provision = new ProvisionSchoolCommand(
                command.CorrelationId,
                school.TenantId,
                school.Id,
                school.RegistrationRequestId,
                school.Code.Value,
                school.DisplayName,
                school.SchoolType.ToString(),
                school.DeploymentMode.ToString(),
                school.Address,
                school.PrimaryPhone,
                selectedUnitType!.Id,
                selectedUnitType.Code,
                selectedUnitType.DisplayName,
                selectedUnitType.UnitPrice,
                selectedUnitType.Currency,
                school.RequestedByAccountId,
                school.CreatedAtUtc,
                now);
            outbox.Enqueue(IntegrationMessageEnvelope<ProvisionSchoolCommand>.Create(
                now,
                Producer,
                IntegrationMessageScope.ForSchool(school.TenantId, school.Id),
                provision,
                new IntegrationAggregateReference("school-registration", (Guid)school.Id, school.Version),
                command.CorrelationId,
                command.CausationId,
                command.TraceParent));
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
        school.DequeueDomainEvents();
        return new TransitionSchoolResult(school.TenantId, school.Id, school.Status, school.Version);
    }

    private static void ApplyAction(
        SchoolRegistration school,
        TransitionSchoolCommand command,
        DateTimeOffset occurredAtUtc)
    {
        var actorId = (Guid)command.RequestedByAccountId;
        switch (command.Action)
        {
            case SchoolLifecycleAction.SubmitForVerification:
                school.SubmitForVerification(actorId, occurredAtUtc);
                break;
            case SchoolLifecycleAction.Approve:
                school.Approve(command.UnitTypeId!.Value, actorId, occurredAtUtc);
                break;
            case SchoolLifecycleAction.Suspend:
                school.Suspend(command.Reason!, actorId, occurredAtUtc);
                break;
            case SchoolLifecycleAction.Reinstate:
                school.Reinstate(actorId, occurredAtUtc);
                break;
            case SchoolLifecycleAction.Close:
                school.Close(command.Reason!, actorId, occurredAtUtc);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(command), "Unknown lifecycle action.");
        }
    }

    private static void Validate(TransitionSchoolCommand command)
    {
        if (command.TenantId.IsEmpty || command.SchoolId.IsEmpty ||
            command.RequestedByAccountId.IsEmpty || command.CorrelationId == Guid.Empty ||
            command.CausationId == Guid.Empty || command.ExpectedVersion < 0 ||
            !Enum.IsDefined(command.Action))
        {
            throw new ArgumentException("Invalid school lifecycle command.", nameof(command));
        }

        if (command.Action is SchoolLifecycleAction.Suspend or SchoolLifecycleAction.Close)
        {
            if (string.IsNullOrWhiteSpace(command.Reason))
            {
                throw new ArgumentException("A reason is required for this action.", nameof(command));
            }
        }
        else if (!string.IsNullOrWhiteSpace(command.Reason))
        {
            throw new ArgumentException("Reason is not accepted for this action.", nameof(command));
        }

        if (command.Action == SchoolLifecycleAction.Approve)
        {
            if (!command.UnitTypeId.HasValue || command.UnitTypeId.Value == Guid.Empty)
                throw new ArgumentException("An active unit type is required for approval.", nameof(command));
        }
        else if (command.UnitTypeId is not null)
        {
            throw new ArgumentException("Unit type is accepted only for approval.", nameof(command));
        }
    }
}
