using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Contracts.Registry;
using Mdaresna.Platform.Domain.Registry.Events;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using System.Text.Json;
using Mdaresna.SharedKernel.Time;
using ProvisionSchoolCommand = Mdaresna.Schools.Contracts.Provisioning.ProvisionSchoolV1;
using Mdaresna.Platform.Application.Billing.Units;

namespace Mdaresna.Platform.Application.Registry.BeginSchoolProvisioning;

public sealed class BeginSchoolProvisioningCommandHandler
{
    private const string Producer = "mdaresna-platform";
    private readonly ISchoolRegistrationRepository _schools;
    private readonly IPlatformOutboxWriter _outbox;
    private readonly IUnitTypeRepository _unitTypes;
    private readonly IPlatformRegistryAuditWriter _audit;
    private readonly IPlatformUnitOfWork _unitOfWork;
    private readonly IClock _clock;
    private readonly Mdaresna.Platform.Application.Billing.IPlatformBillingUnitOfWork? _billing;
    private readonly Mdaresna.Platform.Application.Billing.Subscriptions.ISubscriptionService? _subscriptions;

    public BeginSchoolProvisioningCommandHandler(
        ISchoolRegistrationRepository schools,
        IUnitTypeRepository unitTypes,
        IPlatformOutboxWriter outbox,
        IPlatformRegistryAuditWriter audit,
        IPlatformUnitOfWork unitOfWork,
        IClock clock,
        Mdaresna.Platform.Application.Billing.IPlatformBillingUnitOfWork? billing = null,
        Mdaresna.Platform.Application.Billing.Subscriptions.ISubscriptionService? subscriptions = null)
    {
        _schools = schools;
        _unitTypes = unitTypes;
        _outbox = outbox;
        _audit = audit;
        _unitOfWork = unitOfWork;
        _clock = clock;
        _billing = billing;
        _subscriptions = subscriptions;
    }

    public Task HandleAsync(BeginSchoolProvisioningCommand command, CancellationToken cancellationToken = default) => _billing == null
        ? HandleCoreAsync(command, cancellationToken)
        : _billing.ExecuteInTransactionAsync(async ct => { await HandleCoreAsync(command, ct); return true; }, cancellationToken);

    private async Task HandleCoreAsync(
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
        if (!school.UnitTypeId.HasValue && _subscriptions != null)
            await _subscriptions.PrepareInitialActivationAsync(school.Id.Value, new(command.RequestedByAccountId, command.CorrelationId), cancellationToken);
        var unitType = school.UnitTypeId is { } unitTypeId
            ? await _unitTypes.FindByIdAsync(unitTypeId, cancellationToken)
            : null;
        if (school.UnitTypeId.HasValue && unitType is null)
            throw new PlatformConflictException("unit_type.not_found", "School unit type was not found.");
        var existingDomainEventCount = school.DomainEvents.Count;
        school.BeginProvisioning(
            command.OperationId,
            (Guid)command.RequestedByAccountId,
            now);
        var lifecycleDomainEvent = school.DomainEvents
            .Skip(existingDomainEventCount)
            .OfType<SchoolLifecycleChangedDomainEvent>()
            .Single();

        if (unitType is null)
        {
            var provision = new Mdaresna.Schools.Contracts.Provisioning.ProvisionSchoolV2(command.OperationId, school.TenantId, school.Id,
                school.RegistrationRequestId, school.Code.Value, school.DisplayName, school.SchoolType.ToString(), school.DeploymentMode.ToString(),
                school.Address, school.PrimaryPhone, school.RequestedByAccountId, school.CreatedAtUtc, now);
            _outbox.Enqueue(IntegrationMessageEnvelope<Mdaresna.Schools.Contracts.Provisioning.ProvisionSchoolV2>.Create(now, Producer,
                IntegrationMessageScope.ForSchool(school.TenantId, school.Id), provision, new IntegrationAggregateReference("school-registration", (Guid)school.Id, school.Version),
                command.CorrelationId, command.CausationId, command.TraceParent));
        }
        else
        {
        var integrationCommand = new ProvisionSchoolCommand(
            command.OperationId,
            school.TenantId,
            school.Id,
            school.RegistrationRequestId,
            school.Code.Value,
            school.DisplayName,
            school.SchoolType.ToString(),
            school.DeploymentMode.ToString(),
            school.Address,
            school.PrimaryPhone,
            unitType.Id,
            unitType.Code,
            unitType.DisplayName,
            unitType.UnitPrice,
            unitType.Currency,
            school.RequestedByAccountId,
            school.CreatedAtUtc,
            now);

        _outbox.Enqueue(IntegrationMessageEnvelope<ProvisionSchoolCommand>.Create(
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

        }

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
