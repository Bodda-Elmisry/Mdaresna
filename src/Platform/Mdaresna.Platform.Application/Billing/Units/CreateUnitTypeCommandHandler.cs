using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Billing.Units;
using Mdaresna.SharedKernel.Time;

namespace Mdaresna.Platform.Application.Billing.Units;

public sealed class CreateUnitTypeCommandHandler(
    IUnitTypeRepository types,
    IUnitCommerceAuditWriter audit,
    IPlatformBillingUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<UnitTypeMutationResult> HandleAsync(
        CreateUnitTypeCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        UnitCommerceGuard.ValidateActorAndCorrelation(
            command.RequestedByAccountId, command.CorrelationId);
        var proposed = UnitType.Create(command.UnitTypeId, command.Code,
            command.DisplayName, command.UnitPrice, command.Currency, clock.UtcNow);

        var existing = await types.FindByIdAsync(command.UnitTypeId, cancellationToken);
        if (existing is not null)
        {
            if (existing.Code != proposed.Code ||
                existing.DisplayName != proposed.DisplayName ||
                existing.UnitPrice != proposed.UnitPrice ||
                existing.Currency != proposed.Currency)
            {
                throw new PlatformConflictException(
                    "unit_type.id_reused",
                    "Unit type ID was already used with different catalog data.");
            }

            return new UnitTypeMutationResult(existing.Id, existing.Version, existing.IsActive, false);
        }

        if (await types.FindByCodeAsync(proposed.Code, cancellationToken) is not null)
        {
            throw new PlatformConflictException(
                "unit_type.code_in_use", "Unit type code is already in use.");
        }

        await types.AddAsync(proposed, cancellationToken);
        audit.Stage(new UnitCommerceAuditRecord(
            Guid.NewGuid(), command.RequestedByAccountId, null,
            "platform.unit_type.created", "unit-type", proposed.Id.ToString("D"),
            clock.UtcNow, command.CorrelationId,
            System.Text.Json.JsonSerializer.Serialize(new
            {
                proposed.Code,
                proposed.DisplayName,
                proposed.UnitPrice,
                proposed.Currency
            })));
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new UnitTypeMutationResult(proposed.Id, proposed.Version, proposed.IsActive, true);
    }
}
