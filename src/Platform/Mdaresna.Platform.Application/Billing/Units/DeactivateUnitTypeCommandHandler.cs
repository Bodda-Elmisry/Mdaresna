using Mdaresna.Platform.Application.Errors;
using Mdaresna.SharedKernel.Time;

namespace Mdaresna.Platform.Application.Billing.Units;

public sealed class DeactivateUnitTypeCommandHandler(
    IUnitTypeRepository types,
    IUnitCommerceAuditWriter audit,
    IPlatformBillingUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<UnitTypeMutationResult> HandleAsync(
        DeactivateUnitTypeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        UnitCommerceGuard.ValidateActorAndCorrelation(
            command.RequestedByAccountId, command.CorrelationId);
        UnitCommerceGuard.ValidateTypeAndVersion(command.UnitTypeId, command.ExpectedVersion);
        var unitType = await types.FindByIdAsync(command.UnitTypeId, cancellationToken)
            ?? throw new PlatformResourceNotFoundException(
                "unit_type.not_found", "Unit type was not found.");
        UnitCommerceGuard.EnsureVersion(unitType.Version, command.ExpectedVersion);

        var now = clock.UtcNow;
        if (!unitType.Deactivate(now))
        {
            return new UnitTypeMutationResult(unitType.Id, unitType.Version, false, false);
        }

        audit.Stage(new UnitCommerceAuditRecord(
            Guid.NewGuid(), command.RequestedByAccountId, null,
            "platform.unit_type.deactivated", "unit-type", unitType.Id.ToString("D"),
            now, command.CorrelationId, null));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new UnitTypeMutationResult(unitType.Id, unitType.Version, false, true);
    }
}
