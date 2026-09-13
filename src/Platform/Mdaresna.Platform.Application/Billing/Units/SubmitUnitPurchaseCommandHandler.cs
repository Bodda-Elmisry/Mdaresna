using System.Text.Json;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Billing.Units;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.SharedKernel.Time;

namespace Mdaresna.Platform.Application.Billing.Units;

public sealed class SubmitUnitPurchaseCommandHandler(
    IUnitTypeRepository types,
    IUnitPurchaseRepository purchases,
    IPlatformPaymentRequestRepository payments,
    ISchoolRegistrationRepository schools,
    IUnitCommerceAuditWriter audit,
    IPlatformBillingUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<SubmitUnitPurchaseResult> HandleAsync(
        SubmitUnitPurchaseCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        Validate(command);

        var existingPayment = await payments.FindByIdAsync(command.RequestId, cancellationToken);
        if (existingPayment is not null)
        {
            var existingIntent = await purchases.FindIntentAsync(command.RequestId, cancellationToken);
            EnsureMatchingReplay(existingPayment, existingIntent, command);
            return Result(existingPayment, existingIntent!, false);
        }

        var school = await schools.FindByIdAsync(command.SchoolId, cancellationToken)
            ?? throw new PlatformResourceNotFoundException(
                "school.not_found", "School registration was not found.");
        if (school.TenantId != command.TenantId || school.Status == SchoolLifecycleStatus.Closed)
        {
            throw new PlatformConflictException(
                "school.unavailable_for_payment",
                "School is unavailable for unit purchase.");
        }

        var unitType = await types.FindByIdAsync(command.UnitTypeId, cancellationToken)
            ?? throw new PlatformResourceNotFoundException(
                "unit_type.not_found", "Unit type was not found.");
        UnitCommerceGuard.EnsureVersion(unitType.Version, command.ExpectedUnitTypeVersion);

        var now = clock.UtcNow;
        var intent = UnitPurchaseIntent.Create(
            command.RequestId,
            command.TenantId,
            command.SchoolId,
            unitType,
            command.Quantity,
            command.PaymentMethodCode,
            command.TransferOccurredAtUtc,
            now);
        var payment = PlatformPaymentRequest.Create(
            command.RequestId,
            command.TenantId,
            command.SchoolId,
            intent.Amount,
            intent.Currency,
            command.TransferReference,
            command.RequestedByAccountId,
            now);

        if (await payments.TransferReferenceExistsAsync(
            command.SchoolId, payment.TransferReference, cancellationToken))
        {
            throw new PlatformConflictException(
                "platform_payment.transfer_reference_in_use",
                "Transfer reference is already used for this school.");
        }

        await payments.AddAsync(payment, cancellationToken);
        await purchases.AddIntentAsync(intent, cancellationToken);
        audit.Stage(new UnitCommerceAuditRecord(
            Guid.NewGuid(), command.RequestedByAccountId, command.TenantId,
            "platform.unit_purchase.submitted", "unit-purchase-intent",
            command.RequestId.ToString("D"), now, command.CorrelationId,
            JsonSerializer.Serialize(new
            {
                intent.UnitTypeId,
                intent.UnitTypeVersion,
                intent.Quantity,
                intent.UnitPrice,
                intent.Amount,
                intent.Currency,
                intent.PaymentMethodCode,
                intent.TransferOccurredAtUtc,
                payment.TransferReference
            })));

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (PlatformConflictException)
        {
            // A concurrent request may have committed the same idempotency key.
            // These repository lookups must read the committed database state.
            existingPayment = await payments.FindByIdAsync(command.RequestId, cancellationToken);
            var existingIntent = existingPayment is null
                ? null
                : await purchases.FindIntentAsync(command.RequestId, cancellationToken);
            if (existingPayment is null || existingIntent is null)
            {
                throw;
            }

            EnsureMatchingReplay(existingPayment, existingIntent, command);
            return Result(existingPayment, existingIntent, false);
        }

        return Result(payment, intent, true);
    }

    private static SubmitUnitPurchaseResult Result(
        PlatformPaymentRequest payment,
        UnitPurchaseIntent intent,
        bool wasCreated) => new(
            payment.Id,
            intent.UnitTypeId,
            intent.Quantity,
            intent.UnitPrice,
            intent.Amount,
            intent.Currency,
            payment.Status,
            wasCreated);

    private static void Validate(SubmitUnitPurchaseCommand command)
    {
        UnitCommerceGuard.ValidateActorAndCorrelation(
            command.RequestedByAccountId, command.CorrelationId);
        UnitCommerceGuard.ValidateTypeAndVersion(
            command.UnitTypeId, command.ExpectedUnitTypeVersion);
        if (command.RequestId == Guid.Empty || command.TenantId.IsEmpty ||
            command.SchoolId.IsEmpty || command.SchoolId.Value != command.TenantId.Value ||
            command.Quantity is < 1 or > 1_000_000)
        {
            throw new ArgumentException("Unit purchase identifiers or quantity are invalid.", nameof(command));
        }

        UnitPurchaseIntent.NormalizePaymentMethodCode(command.PaymentMethodCode);
        if (command.TransferOccurredAtUtc == default ||
            command.TransferOccurredAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ArgumentException("Transfer date must be UTC and non-default.", nameof(command));
        }

        if (string.IsNullOrWhiteSpace(command.TransferReference) ||
            command.TransferReference.Trim().Length > 100)
        {
            throw new ArgumentException("Transfer reference is invalid.", nameof(command));
        }
    }

    private static void EnsureMatchingReplay(
        PlatformPaymentRequest payment,
        UnitPurchaseIntent? intent,
        SubmitUnitPurchaseCommand command)
    {
        if (intent is null || payment.TenantId != command.TenantId ||
            payment.SchoolId != command.SchoolId ||
            payment.RequestedByAccountId != command.RequestedByAccountId ||
            !string.Equals(payment.TransferReference,
                command.TransferReference.Trim().ToUpperInvariant(), StringComparison.Ordinal) ||
            intent.TenantId != command.TenantId || intent.SchoolId != command.SchoolId ||
            intent.UnitTypeId != command.UnitTypeId ||
            intent.UnitTypeVersion != command.ExpectedUnitTypeVersion ||
            intent.Quantity != command.Quantity ||
            !string.Equals(intent.PaymentMethodCode,
                UnitPurchaseIntent.NormalizePaymentMethodCode(command.PaymentMethodCode),
                StringComparison.Ordinal) ||
            intent.TransferOccurredAtUtc.ToUnixTimeMilliseconds() !=
                command.TransferOccurredAtUtc.ToUnixTimeMilliseconds() ||
            payment.Amount != intent.Amount || payment.Currency != intent.Currency)
        {
            throw new PlatformConflictException(
                "request.idempotency_key_reused",
                "RequestId was already used with different payment or unit purchase data.");
        }
    }
}
