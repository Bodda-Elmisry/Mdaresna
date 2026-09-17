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
    string OwnerPhone);

public sealed record SubmitSchoolRegistrationResult(Guid RegistrationRequestId);

public interface ISchoolRegistrationRequestPublisher
{
    Task PublishAsync(IntegrationMessageEnvelope<SchoolRegistrationRequestedV2> envelope,
        CancellationToken cancellationToken = default);
}

public sealed class SubmitSchoolRegistrationRequestHandler(
    ISchoolRegistrationRequestPublisher publisher,
    IClock clock)
{
    public async Task<SubmitSchoolRegistrationResult> HandleAsync(
        SubmitSchoolRegistrationRequest request,
        string? traceParent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        var requestId = Guid.NewGuid();
        var tenantId = TenantId.From(Guid.NewGuid());
        var now = clock.UtcNow;
        var data = new SchoolRegistrationRequestedV2(
            requestId, tenantId, $"SCH-{requestId:N}"[..16], request.SchoolName,
            request.SchoolType, request.Address, request.SchoolPrimaryPhone,
            request.OwnerName, request.OwnerPhone, now);
        var envelope = IntegrationMessageEnvelope<SchoolRegistrationRequestedV2>.Create(
            now, "schools", IntegrationMessageScope.ForTenant(tenantId), data,
            correlationId: requestId, traceParent: traceParent);
        await publisher.PublishAsync(envelope, cancellationToken);
        return new SubmitSchoolRegistrationResult(requestId);
    }
}
