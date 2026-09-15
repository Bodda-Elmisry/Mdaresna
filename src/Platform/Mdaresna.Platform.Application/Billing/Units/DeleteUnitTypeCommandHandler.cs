using Mdaresna.Platform.Application.Errors;
using Mdaresna.SharedKernel.Time;

namespace Mdaresna.Platform.Application.Billing.Units;

public sealed class DeleteUnitTypeCommandHandler(
    IUnitTypeRepository types,
    IUnitCommerceAuditWriter audit,
    IPlatformBillingUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task HandleAsync(
        DeleteUnitTypeCommand command,
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
        if (await types.HasUsageAsync(unitType.Id, cancellationToken))
        {
            throw new PlatformConflictException(
                "unit_type.in_use",
                "A unit type linked to a school or previous transaction cannot be deleted.");
        }

        var now = clock.UtcNow;
        types.Remove(unitType);
        audit.Stage(new UnitCommerceAuditRecord(
            Guid.NewGuid(), command.RequestedByAccountId, null,
            "platform.unit_type.deleted", "unit-type", unitType.Id.ToString("D"),
            now, command.CorrelationId,
            System.Text.Json.JsonSerializer.Serialize(new
            {
                unitType.Code,
                unitType.DisplayName,
                unitType.UnitPrice,
                unitType.Currency
            })));
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
