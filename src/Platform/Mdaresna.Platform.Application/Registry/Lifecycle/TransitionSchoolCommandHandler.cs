using System.Text.Json;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Contracts.Registry;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Domain.Registry.Events;
using Mdaresna.SharedKernel.Time;

namespace Mdaresna.Platform.Application.Registry.Lifecycle;

public sealed class TransitionSchoolCommandHandler(
    ISchoolRegistrationRepository schools,
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
        var previousEventCount = school.DomainEvents.Count;
        ApplyAction(school, command, now);
        var lifecycleEvent = school.DomainEvents
            .Skip(previousEventCount)
            .OfType<SchoolLifecycleChangedDomainEvent>()
            .Single();

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
            new IntegrationAggregateReference(
                "school-registration",
                (Guid)school.Id,
                school.Version),
            command.CorrelationId,
            command.CausationId,
            command.TraceParent,
            integrationEvent));

        audit.Stage(new RegistryAuditRecord(
            lifecycleEvent.EventId,
            command.RequestedByAccountId,
            school.TenantId,
            $"platform.school.{command.Action.ToString().ToLowerInvariant()}",
            "school-registration",
            school.Id.ToString(),
            now,
            command.CorrelationId,
            JsonSerializer.Serialize(new
            {
                previousStatus = lifecycleEvent.PreviousStatus.ToString(),
                currentStatus = lifecycleEvent.CurrentStatus.ToString(),
                reason = lifecycleEvent.Reason
            })));

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
                school.Approve(actorId, occurredAtUtc);
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
    }
}
