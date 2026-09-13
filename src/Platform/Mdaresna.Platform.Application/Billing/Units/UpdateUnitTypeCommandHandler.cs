using Mdaresna.Platform.Application.Errors;
using Mdaresna.SharedKernel.Time;

namespace Mdaresna.Platform.Application.Billing.Units;

public sealed class UpdateUnitTypeCommandHandler(
    IUnitTypeRepository types,
    IUnitCommerceAuditWriter audit,
    IPlatformBillingUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<UnitTypeMutationResult> HandleAsync(
        UpdateUnitTypeCommand command,
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

        var oldName = unitType.DisplayName;
        var oldPrice = unitType.UnitPrice;
        var oldCurrency = unitType.Currency;
        var now = clock.UtcNow;
        if (!unitType.ChangeOffer(command.DisplayName, command.UnitPrice, command.Currency, now))
        {
            return new UnitTypeMutationResult(unitType.Id, unitType.Version, unitType.IsActive, false);
        }

        audit.Stage(new UnitCommerceAuditRecord(
            Guid.NewGuid(), command.RequestedByAccountId, null,
            "platform.unit_type.updated", "unit-type", unitType.Id.ToString("D"),
            now, command.CorrelationId,
            System.Text.Json.JsonSerializer.Serialize(new
            {
                previous = new { displayName = oldName, unitPrice = oldPrice, currency = oldCurrency },
                current = new
                {
                    displayName = unitType.DisplayName,
                    unitPrice = unitType.UnitPrice,
                    currency = unitType.Currency
                }
            })));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new UnitTypeMutationResult(unitType.Id, unitType.Version, unitType.IsActive, true);
    }
}
