using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Billing;

public interface IPlatformPaymentRequestRepository
{
    Task<PlatformPaymentRequest?> FindByIdAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task<PlatformPaymentRequest?> FindForReviewAsync(Guid requestId, CancellationToken cancellationToken = default);
    Task<bool> TransferReferenceExistsAsync(
        SchoolId schoolId,
        string transferReference,
        CancellationToken cancellationToken = default);
    Task AddAsync(PlatformPaymentRequest request, CancellationToken cancellationToken = default);
    void AddLedgerEntry(PlatformPaymentLedgerEntry entry);
}
