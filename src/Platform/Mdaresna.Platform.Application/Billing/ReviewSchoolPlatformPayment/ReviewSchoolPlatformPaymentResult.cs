using Mdaresna.Platform.Domain.Billing;

namespace Mdaresna.Platform.Application.Billing.ReviewSchoolPlatformPayment;

public sealed record ReviewSchoolPlatformPaymentResult(
    Guid RequestId,
    PlatformPaymentStatus Status,
    Guid? LedgerEntryId,
    Guid? UnitGrantId);
