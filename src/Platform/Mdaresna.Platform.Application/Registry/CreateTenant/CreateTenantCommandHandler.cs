using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Contracts.Registry;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Domain.Registry.Events;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using Mdaresna.SharedKernel.Time;

namespace Mdaresna.Platform.Application.Registry.CreateTenant;

public sealed class CreateTenantCommandHandler
{
    private const string Producer = "mdaresna-platform";
    private readonly ITenantRepository _tenants;
    private readonly IPlatformOutboxWriter _outbox;
    private readonly IPlatformRegistryAuditWriter _audit;
    private readonly IPlatformUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public CreateTenantCommandHandler(
        ITenantRepository tenants,
        IPlatformOutboxWriter outbox,
        IPlatformRegistryAuditWriter audit,
        IPlatformUnitOfWork unitOfWork,
        IClock clock)
    {
        _tenants = tenants;
        _outbox = outbox;
        _audit = audit;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<CreateTenantResult> HandleAsync(
        CreateTenantCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (command.TenantId.IsEmpty || command.RequestedByAccountId.IsEmpty ||
            command.CorrelationId == Guid.Empty || command.CausationId == Guid.Empty)
        {
            throw new ArgumentException("Tenant request identifiers cannot be empty.", nameof(command));
        }

        var displayName = Normalize(command.DisplayName, 200, nameof(command.DisplayName));
        var legalName = string.IsNullOrWhiteSpace(command.LegalName)
            ? null
            : Normalize(command.LegalName, 250, nameof(command.LegalName));
        var existing = await _tenants.FindByIdAsync(command.TenantId, cancellationToken);

        if (existing is not null)
        {
            if (!string.Equals(existing.DisplayName, displayName, StringComparison.Ordinal) ||
                !string.Equals(existing.LegalName, legalName, StringComparison.Ordinal))
            {
                throw new PlatformConflictException(
                    "tenant.id_reused",
                    "TenantId was already used with different tenant data.");
            }

            return new CreateTenantResult(
                existing.Id,
                existing.DisplayName,
                existing.Status,
                WasCreated: false);
        }

        var now = _clock.UtcNow;
        var tenant = Tenant.Create(command.TenantId, displayName, legalName, now);
        await _tenants.AddAsync(tenant, cancellationToken);

        var integrationEvent = new TenantRegisteredV1(tenant.Id, tenant.DisplayName, now);
        _outbox.Enqueue(IntegrationMessageEnvelope<TenantRegisteredV1>.Create(
            now,
            Producer,
            IntegrationMessageScope.ForTenant(tenant.Id),
            integrationEvent,
            new IntegrationAggregateReference("tenant", (Guid)tenant.Id, tenant.Version),
            command.CorrelationId,
            command.CausationId,
            command.TraceParent));

        var createdEvent = tenant.DomainEvents.OfType<TenantCreatedDomainEvent>().Single();
        _audit.Stage(new RegistryAuditRecord(
            createdEvent.EventId,
            command.RequestedByAccountId,
            tenant.Id,
            "platform.tenant.created",
            "tenant",
            tenant.Id.ToString(),
            now,
            command.CorrelationId,
            null));

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        tenant.DequeueDomainEvents();
        return new CreateTenantResult(tenant.Id, tenant.DisplayName, tenant.Status, WasCreated: true);
    }

    private static string Normalize(string value, int maximumLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        return normalized.Length <= maximumLength
            ? normalized
            : throw new ArgumentOutOfRangeException(parameterName);
    }
}
