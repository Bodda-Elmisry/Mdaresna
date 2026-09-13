using System.Text.Json;
using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Application.Billing.Units;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Contracts.Billing;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Billing.Units;

namespace Mdaresna.Platform.Application.Billing.ReviewSchoolPlatformPayment;

public sealed class ReviewSchoolPlatformPaymentCommandHandler(
    IPlatformPaymentRequestRepository payments,
    IPlatformBillingAuditWriter audit,
    IPlatformOutboxWriter outbox,
    IPlatformBillingUnitOfWork unitOfWork,
    Mdaresna.SharedKernel.Time.IClock clock,
    IUnitPurchaseRepository unitPurchases,
    IUnitCommerceAuditWriter unitAudit)
{
    private const string Producer = "mdaresna-platform";

    public Task<ReviewSchoolPlatformPaymentResult> HandleAsync(
        ReviewSchoolPlatformPaymentCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.RequestId == Guid.Empty || command.CorrelationId == Guid.Empty ||
            command.CausationId == Guid.Empty)
        {
            throw new ArgumentException("RequestId, CorrelationId, and an optional CausationId must be non-empty.");
        }

        return unitOfWork.ExecuteInTransactionAsync(
            ct => ReviewInTransactionAsync(command, ct), cancellationToken);
    }

    private async Task<ReviewSchoolPlatformPaymentResult> ReviewInTransactionAsync(
        ReviewSchoolPlatformPaymentCommand command,
        CancellationToken cancellationToken)
    {
        var request = await payments.FindForReviewAsync(command.RequestId, cancellationToken)
            ?? throw new PlatformResourceNotFoundException(
                "platform_payment.not_found", "Platform payment request was not found.");

        if (request.Status != PlatformPaymentStatus.Pending)
        {
            throw new PlatformConflictException(
                "platform_payment.already_reviewed", "Platform payment request was already reviewed.");
        }

        var intent = await unitPurchases.FindIntentAsync(request.Id, cancellationToken);
        var reviewedAtUtc = clock.UtcNow;
        request.Review(command.Decision, command.ReviewedByAccountId, reviewedAtUtc, command.ReviewNote);

        // The SQL ledger/grant insert guards must observe an Approved payment.
        // This is the first flush inside the same transaction, not a commit.
        await unitOfWork.SaveChangesAsync(cancellationToken);

        PlatformPaymentLedgerEntry? entry = null;
        UnitGrant? grant = null;
        if (request.Status == PlatformPaymentStatus.Approved)
        {
            entry = PlatformPaymentLedgerEntry.FromApprovedRequest(request);
            payments.AddLedgerEntry(entry);

            if (intent is not null)
            {
                grant = UnitGrant.FromApprovedPayment(request, intent, reviewedAtUtc);
                unitPurchases.AddGrant(grant);
                unitAudit.Stage(new UnitCommerceAuditRecord(
                    Guid.NewGuid(), command.ReviewedByAccountId, request.TenantId,
                    "platform.unit_grant.issued", "unit-grant", grant.Id.ToString("D"),
                    reviewedAtUtc, command.CorrelationId,
                    JsonSerializer.Serialize(new
                    {
                        grant.PaymentRequestId,
                        grant.SchoolId,
                        grant.UnitTypeId,
                        grant.UnitTypeCode,
                        grant.Quantity,
                        grant.UnitPrice,
                        grant.Amount,
                        grant.Currency
                    })));

                outbox.Enqueue(new IntegrationMessageEnvelope<SchoolUnitsGrantedV1>(
                    grant.Id,
                    SchoolUnitsGrantedV1.MessageType,
                    SchoolUnitsGrantedV1.SchemaVersion,
                    reviewedAtUtc,
                    Producer,
                    IntegrationMessageScope.ForSchool(grant.TenantId, grant.SchoolId),
                    new IntegrationAggregateReference("school-unit-grant", grant.Id, null),
                    command.CorrelationId,
                    command.CausationId,
                    command.TraceParent,
                    new SchoolUnitsGrantedV1(
                        grant.Id, grant.PaymentRequestId, grant.TenantId, grant.SchoolId,
                        grant.UnitTypeId, grant.UnitTypeCode, grant.UnitTypeName,
                        grant.Quantity, grant.UnitPrice, grant.Amount, grant.Currency,
                        grant.PaymentMethodCode, grant.TransferReference,
                        grant.TransferOccurredAtUtc, grant.GrantedAtUtc)));
            }
        }

        audit.RecordReviewed(request, command.CorrelationId.ToString("D"));
        var domainEvent = request.DomainEvents
            .OfType<PlatformPaymentReviewedDomainEvent>()
            .Single();
        var contract = new SchoolPlatformPaymentReviewedV1(
            request.Id,
            request.TenantId,
            request.SchoolId,
            request.Amount,
            request.Currency,
            request.TransferReference,
            request.Status == PlatformPaymentStatus.Approved
                ? SchoolPlatformPaymentStatusV1.Approved
                : SchoolPlatformPaymentStatusV1.Rejected,
            domainEvent.OccurredAtUtc);

        outbox.Enqueue(new IntegrationMessageEnvelope<SchoolPlatformPaymentReviewedV1>(
            domainEvent.EventId,
            SchoolPlatformPaymentReviewedV1.MessageType,
            SchoolPlatformPaymentReviewedV1.SchemaVersion,
            domainEvent.OccurredAtUtc,
            Producer,
            IntegrationMessageScope.ForSchool(request.TenantId, request.SchoolId),
            new IntegrationAggregateReference("school-platform-payment", request.Id, request.Version),
            command.CorrelationId,
            command.CausationId,
            command.TraceParent,
            contract));

        await unitOfWork.SaveChangesAsync(cancellationToken);
        request.DequeueDomainEvents();
        return new ReviewSchoolPlatformPaymentResult(request.Id, request.Status, entry?.Id, grant?.Id);
    }
}
