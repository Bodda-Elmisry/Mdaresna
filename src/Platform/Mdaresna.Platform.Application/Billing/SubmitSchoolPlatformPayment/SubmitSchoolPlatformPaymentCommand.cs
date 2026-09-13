using Mdaresna.Platform.Domain.Access;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Billing.SubmitSchoolPlatformPayment;

public sealed record SubmitSchoolPlatformPaymentCommand(
    Guid RequestId,
    TenantId TenantId,
    SchoolId SchoolId,
    decimal Amount,
    string Currency,
    string TransferReference,
    IdentityAccountId RequestedByAccountId,
    Guid CorrelationId,
    Guid? CausationId = null,
    string? TraceParent = null);
