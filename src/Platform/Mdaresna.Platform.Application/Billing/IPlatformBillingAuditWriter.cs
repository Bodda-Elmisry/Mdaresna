using Mdaresna.Platform.Domain.Billing;

namespace Mdaresna.Platform.Application.Billing;

public interface IPlatformBillingAuditWriter
{
    void RecordSubmitted(PlatformPaymentRequest request, string? correlationId);
    void RecordReviewed(PlatformPaymentRequest request, string? correlationId);
}
