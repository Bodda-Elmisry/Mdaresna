using Mdaresna.Platform.Application.Billing;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;

internal sealed class PlatformPaymentRequestRepository(PlatformDbContext dbContext)
    : IPlatformPaymentRequestRepository
{
    public Task<PlatformPaymentRequest?> FindByIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default) =>
        dbContext.PlatformPaymentRequests.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == requestId, cancellationToken);

    public Task<PlatformPaymentRequest?> FindForReviewAsync(
        Guid requestId,
        CancellationToken cancellationToken = default) =>
        dbContext.PlatformPaymentRequests
            .SingleOrDefaultAsync(x => x.Id == requestId, cancellationToken);

    public Task<bool> TransferReferenceExistsAsync(
        SchoolId schoolId,
        string transferReference,
        CancellationToken cancellationToken = default) =>
        dbContext.PlatformPaymentRequests.AnyAsync(
            x => x.SchoolId == schoolId && x.TransferReference == transferReference,
            cancellationToken);

    public async Task AddAsync(
        PlatformPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        await dbContext.PlatformPaymentRequests.AddAsync(request, cancellationToken);
    }

    public void AddLedgerEntry(PlatformPaymentLedgerEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        dbContext.PlatformPaymentLedgerEntries.Add(entry);
    }
}
