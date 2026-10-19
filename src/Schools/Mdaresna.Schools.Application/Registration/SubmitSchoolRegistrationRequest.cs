using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Schools.Contracts.Registration;
using Mdaresna.SharedKernel.Time;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Schools.Application.Registration;

public sealed record SubmitSchoolRegistrationRequest(
    string SchoolName,
    string Address,
    RequestedSchoolTypeV2 SchoolType,
    string SchoolPrimaryPhone,
    string OwnerName,
    string OwnerPhone,
    RegistrationPlanSelection[]? Plans = null,
    Guid? RequestId = null);

public sealed record SubmitSchoolRegistrationResult(Guid RegistrationRequestId);

public interface ISchoolRegistrationRequestPublisher
{
    Task PublishAsync(IntegrationMessageEnvelope<SchoolRegistrationRequestedV3> envelope, CancellationToken cancellationToken = default) => throw new NotSupportedException("Registration subscriptions require the V3 publisher.");
    Task PublishAsync(IntegrationMessageEnvelope<SchoolRegistrationRequestedV2> envelope,
        CancellationToken cancellationToken = default);
}
public interface ISchoolRegistrationCatalog
{
    Task<RegistrationPlanOffer[]> PlansAsync(CancellationToken ct);
    Task<Guid> PrepareAsync(PrepareRegistrationOffer input, CancellationToken ct);
}

public sealed class SubmitSchoolRegistrationRequestHandler(
    ISchoolRegistrationRequestPublisher publisher,
    IClock clock,
    ISchoolRegistrationCatalog? catalog = null)
{
    public async Task<SubmitSchoolRegistrationResult> HandleAsync(
        SubmitSchoolRegistrationRequest request,
        string? traceParent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var requestId = request.RequestId ?? Guid.NewGuid();
        if (requestId == Guid.Empty) throw new ArgumentException("Request identity is required.");
        var tenantId = TenantId.From(requestId);
        var now = clock.UtcNow;
        var data = new SchoolRegistrationRequestedV2(
            requestId, tenantId, $"SCH-{requestId:N}"[..16], request.SchoolName,
            request.SchoolType, request.Address, request.SchoolPrimaryPhone,
            request.OwnerName, request.OwnerPhone, now);
        if (catalog != null)
        {
            if (request.Plans == null || request.Plans.Length == 0) throw new ArgumentException("Select at least one system plan.");
            var offerId = await catalog.PrepareAsync(new(data, request.Plans), cancellationToken);
            var selectedEnvelope = IntegrationMessageEnvelope<SchoolRegistrationRequestedV3>.Create(now, "schools", IntegrationMessageScope.ForTenant(tenantId),
                new(data, offerId), correlationId: requestId, traceParent: traceParent);
            await publisher.PublishAsync(selectedEnvelope, cancellationToken);
            return new SubmitSchoolRegistrationResult(requestId);
        }
        var envelope = IntegrationMessageEnvelope<SchoolRegistrationRequestedV2>.Create(
            now, "schools", IntegrationMessageScope.ForTenant(tenantId), data,
            correlationId: requestId, traceParent: traceParent);
        await publisher.PublishAsync(envelope, cancellationToken);
        return new SubmitSchoolRegistrationResult(requestId);
    }
}
