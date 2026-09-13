using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Billing;

namespace Mdaresna.Platform.Application.Billing.ReviewSchoolPlatformPayment;

public sealed record ReviewSchoolPlatformPaymentCommand(
    Guid RequestId,
    PlatformPaymentReviewDecision Decision,
    IdentityAccountId ReviewedByAccountId,
    string? ReviewNote,
    Guid CorrelationId,
    Guid? CausationId = null,
    string? TraceParent = null);
