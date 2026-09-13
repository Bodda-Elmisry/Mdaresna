using Mdaresna.Platform.Application.Billing;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;

internal sealed class PlatformBillingAuditWriter(PlatformDbContext dbContext) : IPlatformBillingAuditWriter
{
    public void RecordSubmitted(PlatformPaymentRequest request, string? correlationId)
    {
        ArgumentNullException.ThrowIfNull(request);
        dbContext.AuditEntries.Add(new PlatformAuditEntry
        {
            Id = Guid.NewGuid(),
            AccountId = request.RequestedByAccountId,
            TenantId = request.TenantId,
            Action = "school-platform-payment.submitted",
            ResourceType = "school-platform-payment",
            ResourceId = request.Id.ToString("D"),
            OccurredAtUtc = request.RequestedAtUtc,
            CorrelationId = correlationId
        });
    }

    public void RecordReviewed(PlatformPaymentRequest request, string? correlationId)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!request.ReviewedByAccountId.HasValue || !request.ReviewedAtUtc.HasValue)
        {
            throw new InvalidOperationException("A payment review is required before writing its audit entry.");
        }

        dbContext.AuditEntries.Add(new PlatformAuditEntry
        {
            Id = Guid.NewGuid(),
            AccountId = request.ReviewedByAccountId,
            TenantId = request.TenantId,
            Action = request.Status == PlatformPaymentStatus.Approved
                ? "school-platform-payment.approved"
                : "school-platform-payment.rejected",
            ResourceType = "school-platform-payment",
            ResourceId = request.Id.ToString("D"),
            OccurredAtUtc = request.ReviewedAtUtc.Value,
            CorrelationId = correlationId
        });
    }
}
