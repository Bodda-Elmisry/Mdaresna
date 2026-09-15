using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Application.Billing.Units;
using Mdaresna.Platform.Application.Registry.CreateTenant;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using Mdaresna.Platform.Application.Registry.RegisterSchool;
using Mdaresna.Platform.Contracts.Registry;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Registry.ConsumeSchoolRegistrationRequest;

/// <summary>
/// Applies the school-registration fact received from the Schools application
/// to the Platform control plane. Broker acknowledgement and inbox persistence
/// remain infrastructure concerns.
/// </summary>
public sealed class ConsumeSchoolRegistrationRequestHandler(
    ISharedIdentityAccountLookup identityAccounts,
    IUnitTypeRepository unitTypes,
    ISchoolRegistrationRepository schools,
    CreateTenantCommandHandler createTenant,
    RegisterSchoolCommandHandler registerSchool,
    TransitionSchoolCommandHandler transitionSchool)
{
    public async Task HandleAsync(
        IntegrationMessageEnvelope<SchoolRegistrationRequestedV1> envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ValidateEnvelope(envelope);

        var data = envelope.Data;
        var requester = IdentityAccountId.From(data.RequestedByAccountId);
        if (!await identityAccounts.ExistsAsync(requester, cancellationToken))
        {
            throw new ArgumentException(
                "The requesting shared identity account does not exist.",
                nameof(envelope));
        }

        if (data.UnitTypeId is { } unitTypeId)
        {
            var unitType = await unitTypes.FindByIdAsync(unitTypeId, cancellationToken);
            if (unitType is null || !unitType.IsActive)
            {
                throw new ArgumentException(
                    "The requested unit type does not exist or is inactive.",
                    nameof(envelope));
            }
        }

        await createTenant.HandleAsync(new CreateTenantCommand(
            data.TenantId,
            data.DisplayName,
            data.LegalName,
            requester,
            envelope.CorrelationId,
            envelope.MessageId,
            envelope.TraceParent), cancellationToken);

        var registration = await registerSchool.HandleAsync(new RegisterSchoolCommand(
            data.RegistrationRequestId,
            data.TenantId,
            data.SchoolCode,
            data.DisplayName,
            data.SchoolType.ToDomain(),
            data.DeploymentMode.ToDomain(),
            requester,
            envelope.CorrelationId,
            envelope.MessageId,
            envelope.TraceParent,
            data.Address,
            data.UnitTypeId), cancellationToken);

        // A cross-application registration event represents a submitted request,
        // not a Platform-side draft. Leave already-advanced requests untouched on
        // safe redelivery/replay.
        if (registration.Status != SchoolLifecycleStatus.Draft)
        {
            return;
        }

        var school = await schools.FindByIdAsync(registration.SchoolId, cancellationToken)
            ?? throw new InvalidOperationException(
                "The school registration disappeared after it was created.");
        await transitionSchool.HandleAsync(new TransitionSchoolCommand(
            data.TenantId,
            registration.SchoolId,
            SchoolLifecycleAction.SubmitForVerification,
            requester,
            school.Version,
            envelope.CorrelationId,
            CausationId: envelope.MessageId,
            TraceParent: envelope.TraceParent), cancellationToken);
    }

    private static void ValidateEnvelope(
        IntegrationMessageEnvelope<SchoolRegistrationRequestedV1> envelope)
    {
        if (!string.Equals(envelope.Producer, "schools", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "School registration requests must be produced by Schools.",
                nameof(envelope));
        }

        if (envelope.Scope.TenantId != envelope.Data.TenantId ||
            envelope.Scope.SchoolId is not null ||
            envelope.Scope.BranchId is not null)
        {
            throw new ArgumentException(
                "School registration request scope is invalid.",
                nameof(envelope));
        }

        if (envelope.Data.RequestedAtUtc > envelope.OccurredAtUtc)
        {
            throw new ArgumentException(
                "RequestedAtUtc cannot be later than the event occurrence time.",
                nameof(envelope));
        }
    }
}
