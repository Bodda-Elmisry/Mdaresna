using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Billing.Read;

public interface IPlatformPaymentReadStore
{
    Task<PlatformPaymentPage> ListAsync(
        TenantId? tenantId,
        SchoolId? schoolId,
        PlatformPaymentStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<PlatformPaymentDetail?> GetAsync(
        Guid requestId,
        CancellationToken cancellationToken = default);
}
