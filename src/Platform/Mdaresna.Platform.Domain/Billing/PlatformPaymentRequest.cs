using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Common;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Domain.Billing;

/// <summary>A school's reported payment to the Mdaresna platform, not an academic fee or coin purchase.</summary>
public sealed class PlatformPaymentRequest : AggregateRoot
{
    private PlatformPaymentRequest(
        Guid id,
        TenantId tenantId,
        SchoolId schoolId,
        decimal amount,
        string currency,
        string transferReference,
        PlatformPaymentStatus status,
        IdentityAccountId requestedByAccountId,
        DateTimeOffset requestedAtUtc,
        IdentityAccountId? reviewedByAccountId,
        DateTimeOffset? reviewedAtUtc,
        string? reviewNote,
        long version)
    {
        Id = id;
        TenantId = tenantId;
        SchoolId = schoolId;
        Amount = amount;
        Currency = currency;
        TransferReference = transferReference;
        Status = status;
        RequestedByAccountId = requestedByAccountId;
        RequestedAtUtc = requestedAtUtc;
        ReviewedByAccountId = reviewedByAccountId;
        ReviewedAtUtc = reviewedAtUtc;
        ReviewNote = reviewNote;
        RestoreVersion(version);
    }

    public Guid Id { get; }
    public TenantId TenantId { get; }
    public SchoolId SchoolId { get; }
    public decimal Amount { get; }
    public string Currency { get; }
    public string TransferReference { get; }
    public PlatformPaymentStatus Status { get; private set; }
    public IdentityAccountId RequestedByAccountId { get; }
    public DateTimeOffset RequestedAtUtc { get; }
    public IdentityAccountId? ReviewedByAccountId { get; private set; }
    public DateTimeOffset? ReviewedAtUtc { get; private set; }
    public string? ReviewNote { get; private set; }

    public static PlatformPaymentRequest Create(
        Guid id,
        TenantId tenantId,
        SchoolId schoolId,
        decimal amount,
        string currency,
        string transferReference,
        IdentityAccountId requestedByAccountId,
        DateTimeOffset requestedAtUtc)
    {
        ValidateIdentity(id, tenantId, schoolId, requestedByAccountId);

        return new PlatformPaymentRequest(
            id,
            tenantId,
            schoolId,
            ValidateAmount(amount),
            NormalizeCurrency(currency),
            NormalizeTransferReference(transferReference),
            PlatformPaymentStatus.Pending,
            requestedByAccountId,
            DomainGuard.UtcTimestamp(requestedAtUtc, nameof(requestedAtUtc)),
            null,
            null,
            null,
            version: 0);
    }

    public void Review(
        PlatformPaymentReviewDecision decision,
        IdentityAccountId reviewedByAccountId,
        DateTimeOffset reviewedAtUtc,
        string? reviewNote)
    {
        if (Status != PlatformPaymentStatus.Pending)
        {
            throw new PlatformDomainException(
                "platform_payment.already_reviewed",
                "A platform payment request can be reviewed only once.");
        }

        if (!Enum.IsDefined(decision))
        {
            throw new ArgumentOutOfRangeException(nameof(decision));
        }

        if (reviewedByAccountId.IsEmpty)
        {
            throw new ArgumentException("Reviewer account is required.", nameof(reviewedByAccountId));
        }

        if (reviewedByAccountId == RequestedByAccountId)
        {
            throw new PlatformDomainException(
                "platform_payment.self_review_forbidden",
                "A payment request cannot be reviewed by its requester.");
        }

        var reviewedAt = DomainGuard.UtcTimestamp(reviewedAtUtc, nameof(reviewedAtUtc));
        if (reviewedAt < RequestedAtUtc)
        {
            throw new ArgumentException("Review time cannot precede request time.", nameof(reviewedAtUtc));
        }

        var note = DomainGuard.OptionalText(reviewNote, 1000, nameof(reviewNote));
        if (decision == PlatformPaymentReviewDecision.Reject && note is null)
        {
            throw new PlatformDomainException(
                "platform_payment.rejection_reason_required",
                "A rejection reason is required.");
        }

        Status = decision == PlatformPaymentReviewDecision.Approve
            ? PlatformPaymentStatus.Approved
            : PlatformPaymentStatus.Rejected;
        ReviewedByAccountId = reviewedByAccountId;
        ReviewedAtUtc = reviewedAt;
        ReviewNote = note;
        Raise(new PlatformPaymentReviewedDomainEvent(
            Guid.NewGuid(),
            reviewedAt,
            Id,
            Status,
            reviewedByAccountId));
    }

    internal static PlatformPaymentRequest Rehydrate(
        Guid id,
        TenantId tenantId,
        SchoolId schoolId,
        decimal amount,
        string currency,
        string transferReference,
        PlatformPaymentStatus status,
        IdentityAccountId requestedByAccountId,
        DateTimeOffset requestedAtUtc,
        IdentityAccountId? reviewedByAccountId,
        DateTimeOffset? reviewedAtUtc,
        string? reviewNote,
        long version)
    {
        ValidateIdentity(id, tenantId, schoolId, requestedByAccountId);
        if (!Enum.IsDefined(status) ||
            (status == PlatformPaymentStatus.Pending && (reviewedByAccountId.HasValue || reviewedAtUtc.HasValue || reviewNote is not null)) ||
            (status != PlatformPaymentStatus.Pending && (!reviewedByAccountId.HasValue || !reviewedAtUtc.HasValue)) ||
            (status == PlatformPaymentStatus.Rejected && string.IsNullOrWhiteSpace(reviewNote)))
        {
            throw new ArgumentException("Invalid persisted payment review state.", nameof(status));
        }

        var requestedAt = DomainGuard.UtcTimestamp(requestedAtUtc, nameof(requestedAtUtc));
        DateTimeOffset? reviewedAt = reviewedAtUtc.HasValue
            ? DomainGuard.UtcTimestamp(reviewedAtUtc.Value, nameof(reviewedAtUtc))
            : null;
        if (reviewedAt < requestedAt)
        {
            throw new ArgumentException("Review time cannot precede request time.", nameof(reviewedAtUtc));
        }

        return new PlatformPaymentRequest(
            id,
            tenantId,
            schoolId,
            ValidateAmount(amount),
            NormalizeCurrency(currency),
            NormalizeTransferReference(transferReference),
            status,
            requestedByAccountId,
            requestedAt,
            reviewedByAccountId,
            reviewedAt,
            DomainGuard.OptionalText(reviewNote, 1000, nameof(reviewNote)),
            version);
    }

    private static void ValidateIdentity(Guid id, TenantId tenantId, SchoolId schoolId, IdentityAccountId requester)
    {
        DomainGuard.NonEmptyGuid(id, nameof(id));
        if (tenantId.IsEmpty || schoolId.IsEmpty || requester.IsEmpty)
        {
            throw new ArgumentException("Tenant, school, and requester identifiers are required.");
        }
    }

    private static decimal ValidateAmount(decimal amount)
    {
        if (amount <= 0 || amount > 9999999999999999.99m || decimal.Round(amount, 2) != amount)
        {
            throw new ArgumentOutOfRangeException(nameof(amount), "Amount must be positive with at most two decimal places and fit decimal(18,2).");
        }

        return amount;
    }

    private static string NormalizeCurrency(string currency)
    {
        var normalized = DomainGuard.RequiredText(currency, 3, nameof(currency)).ToUpperInvariant();
        if (normalized.Length != 3 || normalized.Any(ch => ch is < 'A' or > 'Z'))
        {
            throw new ArgumentException("Currency must be a three-letter ISO code.", nameof(currency));
        }

        return normalized;
    }

    private static string NormalizeTransferReference(string reference) =>
        DomainGuard.RequiredText(reference, 100, nameof(reference)).ToUpperInvariant();
}
