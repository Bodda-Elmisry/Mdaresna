using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Application.Registry.CreateTenant;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using Mdaresna.Platform.Application.Registry.RegisterSchool;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Schools.Contracts.Registration;

namespace Mdaresna.Platform.Application.Registry.ConsumeSchoolRegistrationRequest;

public sealed class ConsumeSchoolRegistrationRequestV2Handler(
    ISharedIdentityAccountProvisioner identityAccounts,
    ISchoolRegistrationRepository schools,
    CreateTenantCommandHandler createTenant,
    RegisterSchoolCommandHandler registerSchool,
    TransitionSchoolCommandHandler transitionSchool)
{
    public async Task HandleAsync(
        IntegrationMessageEnvelope<SchoolRegistrationRequestedV2> envelope,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(envelope);
        ValidateEnvelope(envelope);
        var data = envelope.Data;
        var owner = await identityAccounts.GetOrCreateByPhoneAsync(
            data.RegistrationRequestId, data.OwnerName, data.OwnerPhone, cancellationToken);

        await createTenant.HandleAsync(new CreateTenantCommand(
            data.TenantId, data.SchoolName, data.SchoolName, owner,
            envelope.CorrelationId, envelope.MessageId, envelope.TraceParent), cancellationToken);

        var registration = await registerSchool.HandleAsync(new RegisterSchoolCommand(
            data.RegistrationRequestId, data.TenantId, data.SchoolCode, data.SchoolName,
            data.SchoolType == RequestedSchoolTypeV2.Private ? SchoolType.Private : SchoolType.Government,
            DeploymentMode.SharedSaaS, owner, envelope.CorrelationId, envelope.MessageId,
            envelope.TraceParent, data.Address, UnitTypeId: null,
            PrimaryPhone: data.SchoolPrimaryPhone), cancellationToken);

        if (registration.Status != SchoolLifecycleStatus.Draft) return;
        var school = await schools.FindByIdAsync(registration.SchoolId, cancellationToken)
            ?? throw new InvalidOperationException("The school registration disappeared after creation.");
        await transitionSchool.HandleAsync(new TransitionSchoolCommand(
            data.TenantId, registration.SchoolId, SchoolLifecycleAction.SubmitForVerification,
            owner, school.Version, envelope.CorrelationId, Reason: null,
            CausationId: envelope.MessageId, TraceParent: envelope.TraceParent), cancellationToken);
    }

    private static void ValidateEnvelope(IntegrationMessageEnvelope<SchoolRegistrationRequestedV2> envelope)
    {
        if (!string.Equals(envelope.Producer, "schools", StringComparison.Ordinal) ||
            envelope.Scope.TenantId != envelope.Data.TenantId ||
            envelope.Scope.SchoolId is not null || envelope.Scope.BranchId is not null)
            throw new ArgumentException("School registration request envelope is invalid.", nameof(envelope));
        if (envelope.Data.RequestedAtUtc > envelope.OccurredAtUtc)
            throw new ArgumentException("RequestedAtUtc cannot be later than OccurredAtUtc.", nameof(envelope));
    }
}
