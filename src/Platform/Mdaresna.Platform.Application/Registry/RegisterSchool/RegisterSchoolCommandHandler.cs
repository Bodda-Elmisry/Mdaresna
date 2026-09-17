using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Contracts.Registry;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Domain.Registry.Events;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using Mdaresna.SharedKernel.Time;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.RegisterSchool;

public sealed class RegisterSchoolCommandHandler
{
    private const string Producer = "mdaresna-platform";
    private readonly ITenantRepository _tenants;
    private readonly ISchoolRegistrationRepository _schools;
    private readonly IPlatformOutboxWriter _outbox;
    private readonly IPlatformRegistryAuditWriter _audit;
    private readonly IPlatformUnitOfWork _unitOfWork;
    private readonly IClock _clock;

    public RegisterSchoolCommandHandler(
        ITenantRepository tenants,
        ISchoolRegistrationRepository schools,
        IPlatformOutboxWriter outbox,
        IPlatformRegistryAuditWriter audit,
        IPlatformUnitOfWork unitOfWork,
        IClock clock)
    {
        _tenants = tenants;
        _schools = schools;
        _outbox = outbox;
        _audit = audit;
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<RegisterSchoolResult> HandleAsync(
        RegisterSchoolCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        EnsureIdentifiers(command);
        var schoolCode = SchoolCode.Create(command.SchoolCode);
        var normalizedName = NormalizeName(command.DisplayName);
        var existing = await _schools.FindByRegistrationRequestIdAsync(
            command.RegistrationRequestId,
            cancellationToken);

        if (existing is not null)
        {
            EnsureIdempotentReplayMatches(existing, command, schoolCode, normalizedName);
            return ToResult(existing, wasCreated: false);
        }

        var tenant = await _tenants.FindByIdAsync(command.TenantId, cancellationToken)
            ?? throw new PlatformResourceNotFoundException(
                "tenant.not_found",
                $"Tenant '{command.TenantId}' was not found.");

        if (!tenant.CanRegisterSchools)
        {
            throw new PlatformConflictException(
                "tenant.cannot_register_schools",
                $"Tenant in state '{tenant.Status}' cannot register schools.");
        }

        var schoolId = SchoolId.From(command.TenantId.Value);
        if (await _schools.FindByIdAsync(schoolId, cancellationToken) is not null)
        {
            throw new PlatformConflictException(
                "tenant.school_already_registered",
                "This tenant already has a school registration.");
        }

        if (await _schools.IsSchoolCodeInUseAsync(schoolCode, cancellationToken))
        {
            throw new PlatformConflictException(
                "school.code_in_use",
                $"School code '{schoolCode}' is already in use.");
        }

        var now = _clock.UtcNow;
        var registration = SchoolRegistration.Create(
            schoolId,
            command.TenantId,
            command.RegistrationRequestId,
            schoolCode,
            normalizedName,
            command.SchoolType,
            command.DeploymentMode,
            (Guid)command.RequestedByAccountId,
            now,
            command.Address,
            command.UnitTypeId,
            command.PrimaryPhone);

        await _schools.AddAsync(registration, cancellationToken);

        var integrationEvent = new SchoolRegisteredV1(
            registration.RegistrationRequestId,
            registration.TenantId,
            registration.Id,
            registration.Code.Value,
            registration.DisplayName,
            registration.SchoolType.ToContract(),
            registration.DeploymentMode.ToContract(),
            now);

        _outbox.Enqueue(IntegrationMessageEnvelope<SchoolRegisteredV1>.Create(
            now,
            Producer,
            IntegrationMessageScope.ForSchool(registration.TenantId, registration.Id),
            integrationEvent,
            new IntegrationAggregateReference(
                "school-registration",
                (Guid)registration.Id,
                registration.Version),
            command.CorrelationId,
            command.CausationId,
            command.TraceParent));

        var createdEvent = registration.DomainEvents
            .OfType<SchoolRegistrationCreatedDomainEvent>()
            .Single();
        _audit.Stage(new RegistryAuditRecord(
            createdEvent.EventId,
            command.RequestedByAccountId,
            registration.TenantId,
            "platform.school.registered",
            "school-registration",
            registration.Id.ToString(),
            now,
            command.CorrelationId,
            null));

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        registration.DequeueDomainEvents();
        return ToResult(registration, wasCreated: true);
    }

    private static RegisterSchoolResult ToResult(
        SchoolRegistration registration,
        bool wasCreated) => new(
            registration.Id,
            registration.TenantId,
            registration.Code,
            registration.Status,
            wasCreated);

    private static void EnsureIdentifiers(RegisterSchoolCommand command)
    {
        if (command.RegistrationRequestId == Guid.Empty)
        {
            throw new ArgumentException(
                "RegistrationRequestId cannot be empty.",
                nameof(command));
        }

        if (command.TenantId.IsEmpty)
        {
            throw new ArgumentException("TenantId cannot be empty.", nameof(command));
        }

        if (command.RequestedByAccountId.IsEmpty)
        {
            throw new ArgumentException("RequestedByAccountId cannot be empty.", nameof(command));
        }

        if (command.CorrelationId == Guid.Empty || command.CausationId == Guid.Empty)
        {
            throw new ArgumentException(
                "CorrelationId and an optional CausationId must be non-empty.",
                nameof(command));
        }
    }

    private static string NormalizeName(string value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value);
        var normalized = value.Trim();
        return normalized.Length <= 200
            ? normalized
            : throw new ArgumentOutOfRangeException(nameof(value));
    }

    private static void EnsureIdempotentReplayMatches(
        SchoolRegistration existing,
        RegisterSchoolCommand command,
        SchoolCode schoolCode,
        string displayName)
    {
        if (existing.TenantId != command.TenantId ||
            existing.Code != schoolCode ||
            !string.Equals(existing.DisplayName, displayName, StringComparison.Ordinal) ||
            existing.SchoolType != command.SchoolType ||
            existing.DeploymentMode != command.DeploymentMode ||
            existing.RequestedByAccountId != (Guid)command.RequestedByAccountId ||
            !string.Equals(existing.Address, NormalizeOptional(command.Address, 500),
                StringComparison.Ordinal) ||
            existing.UnitTypeId != command.UnitTypeId ||
            !string.Equals(existing.PrimaryPhone, NormalizeOptional(command.PrimaryPhone, 16), StringComparison.Ordinal))
        {
            throw new PlatformConflictException(
                "request.idempotency_key_reused",
                "RegistrationRequestId was already used with different registration data.");
        }
    }

    private static string? NormalizeOptional(string? value, int maximumLength) =>
        string.IsNullOrWhiteSpace(value) ? null : Normalize(value, maximumLength, nameof(value));

    private static string Normalize(string value, int maximumLength, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(value, parameterName);
        var normalized = value.Trim();
        return normalized.Length <= maximumLength
            ? normalized
            : throw new ArgumentOutOfRangeException(parameterName);
    }
}
