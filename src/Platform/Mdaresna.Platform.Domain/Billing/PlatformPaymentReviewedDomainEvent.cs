using Mdaresna.Platform.Domain.Access;
using Mdaresna.SharedKernel.Domain;

namespace Mdaresna.Platform.Domain.Billing;

public sealed record PlatformPaymentReviewedDomainEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid PaymentRequestId,
    PlatformPaymentStatus Status,
    IdentityAccountId ReviewedByAccountId) : IDomainEvent;
