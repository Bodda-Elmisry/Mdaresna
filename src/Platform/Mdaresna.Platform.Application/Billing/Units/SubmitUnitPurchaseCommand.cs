using Mdaresna.Platform.Domain.Access;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Application.Billing.Units;

public sealed record SubmitUnitPurchaseCommand(
    Guid RequestId,
    TenantId TenantId,
    SchoolId SchoolId,
    Guid UnitTypeId,
    long ExpectedUnitTypeVersion,
    int Quantity,
    string TransferReference,
    string PaymentMethodCode,
    DateTimeOffset TransferOccurredAtUtc,
    IdentityAccountId RequestedByAccountId,
    Guid CorrelationId);
