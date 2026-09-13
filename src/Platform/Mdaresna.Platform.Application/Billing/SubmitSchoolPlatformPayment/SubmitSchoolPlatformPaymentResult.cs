using Mdaresna.Platform.Domain.Billing;

namespace Mdaresna.Platform.Application.Billing.SubmitSchoolPlatformPayment;

public sealed record SubmitSchoolPlatformPaymentResult(
    Guid RequestId,
    PlatformPaymentStatus Status,
    bool WasCreated);
