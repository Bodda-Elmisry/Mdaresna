using Mdaresna.Platform.Domain.Access;

namespace Mdaresna.Platform.Application.Billing.Units;

public sealed record CreateUnitTypeCommand(
    Guid UnitTypeId,
    string Code,
    string DisplayName,
    decimal UnitPrice,
    string Currency,
    IdentityAccountId RequestedByAccountId,
    Guid CorrelationId);

public sealed record UpdateUnitTypeCommand(
    Guid UnitTypeId,
    long ExpectedVersion,
    string DisplayName,
    decimal UnitPrice,
    string Currency,
    IdentityAccountId RequestedByAccountId,
    Guid CorrelationId);

public sealed record DeactivateUnitTypeCommand(
    Guid UnitTypeId,
    long ExpectedVersion,
    IdentityAccountId RequestedByAccountId,
    Guid CorrelationId);

public sealed record ActivateUnitTypeCommand(
    Guid UnitTypeId,
    long ExpectedVersion,
    IdentityAccountId RequestedByAccountId,
    Guid CorrelationId);

public sealed record DeleteUnitTypeCommand(
    Guid UnitTypeId,
    long ExpectedVersion,
    IdentityAccountId RequestedByAccountId,
    Guid CorrelationId);

public sealed record UnitTypeMutationResult(
    Guid UnitTypeId,
    long Version,
    bool IsActive,
    bool Changed);
