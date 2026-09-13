using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.SharedKernel.Time;

namespace Mdaresna.Platform.Application.Billing.SubmitSchoolPlatformPayment;

public sealed class SubmitSchoolPlatformPaymentCommandHandler(
    IPlatformPaymentRequestRepository payments,
    ISchoolRegistrationRepository schools,
    IPlatformBillingAuditWriter audit,
    IPlatformBillingUnitOfWork unitOfWork,
    IClock clock)
{
    public async Task<SubmitSchoolPlatformPaymentResult> HandleAsync(
        SubmitSchoolPlatformPaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        ValidateMessageIdentifiers(command.RequestId, command.CorrelationId, command.CausationId);

        var proposed = PlatformPaymentRequest.Create(
            command.RequestId,
            command.TenantId,
            command.SchoolId,
            command.Amount,
            command.Currency,
            command.TransferReference,
            command.RequestedByAccountId,
            clock.UtcNow);

        var existing = await payments.FindByIdAsync(command.RequestId, cancellationToken);
        if (existing is not null)
        {
            EnsureSameRequest(existing, proposed);
            return new SubmitSchoolPlatformPaymentResult(existing.Id, existing.Status, WasCreated: false);
        }

        var school = await schools.FindByIdAsync(command.SchoolId, cancellationToken)
            ?? throw new PlatformResourceNotFoundException(
                "school.not_found", "School registration was not found.");

        if (school.TenantId != command.TenantId || school.Status == SchoolLifecycleStatus.Closed)
        {
            throw new PlatformConflictException(
                "school.unavailable_for_payment", "School is unavailable for platform payment requests.");
        }

        if (await payments.TransferReferenceExistsAsync(
            command.SchoolId,
            proposed.TransferReference,
            cancellationToken))
        {
            throw new PlatformConflictException(
                "platform_payment.transfer_reference_in_use",
                "Transfer reference is already used for this school.");
        }

        await payments.AddAsync(proposed, cancellationToken);
        audit.RecordSubmitted(proposed, command.CorrelationId.ToString("D"));

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (PlatformConflictException)
        {
            // A concurrent retry may have committed the same idempotency key first.
            // Repository reads for this path are no-tracking, so the unsaved local entity is ignored.
            existing = await payments.FindByIdAsync(command.RequestId, cancellationToken);
            if (existing is null)
            {
                throw;
            }

            EnsureSameRequest(existing, proposed);
            return new SubmitSchoolPlatformPaymentResult(existing.Id, existing.Status, WasCreated: false);
        }

        return new SubmitSchoolPlatformPaymentResult(proposed.Id, proposed.Status, WasCreated: true);
    }

    private static void EnsureSameRequest(PlatformPaymentRequest existing, PlatformPaymentRequest proposed)
    {
        if (existing.TenantId != proposed.TenantId ||
            existing.SchoolId != proposed.SchoolId ||
            existing.Amount != proposed.Amount ||
            !string.Equals(existing.Currency, proposed.Currency, StringComparison.Ordinal) ||
            !string.Equals(existing.TransferReference, proposed.TransferReference, StringComparison.Ordinal) ||
            existing.RequestedByAccountId != proposed.RequestedByAccountId)
        {
            throw new PlatformConflictException(
                "request.idempotency_key_reused",
                "RequestId was already used with different payment data.");
        }
    }

    private static void ValidateMessageIdentifiers(Guid requestId, Guid correlationId, Guid? causationId)
    {
        if (requestId == Guid.Empty || correlationId == Guid.Empty || causationId == Guid.Empty)
        {
            throw new ArgumentException("RequestId, CorrelationId, and an optional CausationId must be non-empty.");
        }
    }
}
