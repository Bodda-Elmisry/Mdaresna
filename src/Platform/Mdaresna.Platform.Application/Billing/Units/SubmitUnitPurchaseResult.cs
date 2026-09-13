using Mdaresna.Platform.Domain.Billing;

namespace Mdaresna.Platform.Application.Billing.Units;

public sealed record SubmitUnitPurchaseResult(
    Guid RequestId,
    Guid UnitTypeId,
    int Quantity,
    decimal UnitPrice,
    decimal Amount,
    string Currency,
    PlatformPaymentStatus Status,
    bool WasCreated);
