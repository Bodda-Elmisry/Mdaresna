using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Contracts.Registry;
using Mdaresna.Platform.Domain.Registry.Events;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using System.Text.Json;
using Mdaresna.SharedKernel.Time;

namespace Mdaresna.Platform.Application.Registry.BeginSchoolProvisioning;

public sealed class BeginSchoolProvisioningCommandHandler
{
    private const string Producer = "mdaresna-platform";
    private readonly ISchoolRegistrationRepository _schools;
    private readonly IPlatformOutboxWriter _outbox;
    private readonly IPlatformRegistryAuditWriter _audit;
    private readonly IPlatformUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public BeginSchoolProvisioningCommandHandler(
        ISchoolRegistrationRepository schools,
        IPlatformOutboxWriter outbox,
        IPlatformRegistryAuditWriter audit,
        IPlatformUnitOfWork unitOfWork,
        IClock clock)
    {
        _schools = schools;
        _outbox = outbox;
        _audit = audit;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task HandleAsync(
        BeginSchoolProvisioningCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.TenantId.IsEmpty || command.SchoolId.IsEmpty ||
            command.OperationId == Guid.Empty || command.RequestedByAccountId.IsEmpty ||
            command.CorrelationId == Guid.Empty || command.CausationId == Guid.Empty)
        {
            throw new ArgumentException("Provisioning identifiers cannot be empty.", nameof(command));
        }

        var school = await _schools.FindByIdAsync(command.SchoolId, cancellationToken)
            ?? throw new PlatformResourceNotFoundException(
                "school.not_found",
                $"School '{command.SchoolId}' was not found.");

        if (school.TenantId != command.TenantId)
        {
            throw new PlatformResourceNotFoundException(
                "school.not_found",
                $"School '{command.SchoolId}' was not found in this tenant.");
        }

        if (school.ProvisioningOperationId == command.OperationId)
        {
            return;
        }

        var now = _clock.UtcNow;
        var existingDomainEventCount = school.DomainEvents.Count;
        school.BeginProvisioning(
            command.OperationId,
            (Guid)command.RequestedByAccountId,
            now);
        var lifecycleDomainEvent = school.DomainEvents
            .Skip(existingDomainEventCount)
            .OfType<SchoolLifecycleChangedDomainEvent>()
            .Single();

        var integrationCommand = new ProvisionSchoolV1(
            command.OperationId,
            school.TenantId,
            school.Id,
            school.Code.Value,
            school.DisplayName,
            school.SchoolType.ToContract(),
            school.DeploymentMode.ToContract(),
            now);

        _outbox.Enqueue(IntegrationMessageEnvelope<ProvisionSchoolV1>.Create(
            now,
            Producer,
            IntegrationMessageScope.ForSchool(school.TenantId, school.Id),
            integrationCommand,
            new IntegrationAggregateReference(
                "school-registration",
                (Guid)school.Id,
                school.Version),
            command.CorrelationId,
            command.CausationId,
            command.TraceParent));

        var lifecycleEvent = new SchoolLifecycleChangedV1(
            lifecycleDomainEvent.TenantId,
            lifecycleDomainEvent.SchoolId,
            lifecycleDomainEvent.PreviousStatus.ToContract(),
            lifecycleDomainEvent.CurrentStatus.ToContract(),
            lifecycleDomainEvent.ProvisioningOperationId,
            lifecycleDomainEvent.Reason,
            lifecycleDomainEvent.OccurredAtUtc);

        _outbox.Enqueue(new IntegrationMessageEnvelope<SchoolLifecycleChangedV1>(
            lifecycleDomainEvent.EventId,
            SchoolLifecycleChangedV1.MessageType,
            SchoolLifecycleChangedV1.SchemaVersion,
            lifecycleDomainEvent.OccurredAtUtc,
            Producer,
            IntegrationMessageScope.ForSchool(school.TenantId, school.Id),
            new IntegrationAggregateReference(
                "school-registration",
                (Guid)school.Id,
                school.Version),
            command.CorrelationId,
            command.CausationId,
            command.TraceParent,
            lifecycleEvent));

        _audit.Stage(new RegistryAuditRecord(
            lifecycleDomainEvent.EventId,
            command.RequestedByAccountId,
            school.TenantId,
            "platform.school.begin_provisioning",
            "school-registration",
            school.Id.ToString(),
            now,
            command.CorrelationId,
            JsonSerializer.Serialize(new
            {
                operationId = command.OperationId,
                previousStatus = lifecycleDomainEvent.PreviousStatus.ToString(),
                currentStatus = lifecycleDomainEvent.CurrentStatus.ToString()
            })));

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        school.DequeueDomainEvents();
    }
}
