using Mdaresna.Platform.Application.Billing.Read;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;

internal sealed class PlatformPaymentReadStore(PlatformDbContext dbContext) : IPlatformPaymentReadStore
{
    public async Task<PlatformPaymentPage> ListAsync(
        TenantId? tenantId,
        SchoolId? schoolId,
        PlatformPaymentStatus? status,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        if (tenantId is { IsEmpty: true } || schoolId is { IsEmpty: true })
        {
            throw new ArgumentException("TenantId and SchoolId cannot be empty when supplied.");
        }

        if (status.HasValue && !Enum.IsDefined(status.Value))
        {
            throw new ArgumentOutOfRangeException(nameof(status));
        }

        ArgumentOutOfRangeException.ThrowIfLessThan(pageNumber, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(pageSize, 1);
        ArgumentOutOfRangeException.ThrowIfGreaterThan(pageSize, 100);

        var query = dbContext.PlatformPaymentRequests.AsNoTracking();
        if (tenantId.HasValue)
        {
            query = query.Where(x => x.TenantId == tenantId.Value);
        }

        if (schoolId.HasValue)
        {
            query = query.Where(x => x.SchoolId == schoolId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var offset = ((long)pageNumber - 1) * pageSize;
        if (offset > int.MaxValue)
        {
            return new PlatformPaymentPage([], totalCount, pageNumber, pageSize);
        }

        var pageQuery = query
            .OrderByDescending(x => x.RequestedAtUtc)
            .ThenBy(x => x.Id)
            .Skip((int)offset)
            .Take(pageSize);

        // Read the request and its posted ledger in the same SQL statement.
        var rows = await WithLedger(
                pageQuery,
                dbContext.PlatformPaymentLedgerEntries.AsNoTracking())
            .ToListAsync(cancellationToken);

        if (rows.Count == 0)
        {
            return new PlatformPaymentPage([], totalCount, pageNumber, pageSize);
        }

        var items = rows.Select(row =>
            PlatformPaymentDetail.From(row.Request, row.Ledger))
            .ToArray();

        return new PlatformPaymentPage(items, totalCount, pageNumber, pageSize);
    }

    public async Task<PlatformPaymentDetail?> GetAsync(
        Guid requestId,
        CancellationToken cancellationToken = default)
    {
        if (requestId == Guid.Empty)
        {
            throw new ArgumentException("RequestId cannot be empty.", nameof(requestId));
        }

        var row = await WithLedger(
                dbContext.PlatformPaymentRequests.AsNoTracking()
                    .Where(x => x.Id == requestId),
                dbContext.PlatformPaymentLedgerEntries.AsNoTracking())
            .SingleOrDefaultAsync(cancellationToken);
        if (row is null)
        {
            return null;
        }

        return PlatformPaymentDetail.From(row.Request, row.Ledger);
    }

    internal static IQueryable<PlatformPaymentReadRow> WithLedger(
        IQueryable<PlatformPaymentRequest> requests,
        IQueryable<PlatformPaymentLedgerEntry> ledgerEntries) =>
        from request in requests
        join posted in ledgerEntries
            on request.Id equals posted.PaymentRequestId into postedEntries
        from ledger in postedEntries.DefaultIfEmpty()
        select new PlatformPaymentReadRow(request, ledger);
}

internal sealed record PlatformPaymentReadRow(
    PlatformPaymentRequest Request,
    PlatformPaymentLedgerEntry? Ledger);
