using Mdaresna.Platform.Domain.Common;
using Mdaresna.Platform.Domain.Registry.Events;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.Domain.Registry;

public sealed class Tenant : AggregateRoot
{
    private Tenant(
        TenantId id,
        string displayName,
        string? legalName,
        TenantStatus status,
        string? suspensionReason,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        long version)
    {
        Id = id;
        DisplayName = displayName;
        LegalName = legalName;
        Status = status;
        SuspensionReason = suspensionReason;
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        RestoreVersion(version);
    }

    public TenantId Id { get; }

    public string DisplayName { get; private set; }

    public string? LegalName { get; private set; }

    public TenantStatus Status { get; private set; }

    public string? SuspensionReason { get; private set; }

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public bool CanRegisterSchools => Status is TenantStatus.Draft or TenantStatus.Active;

    public static Tenant Create(
        TenantId id,
        string displayName,
        string? legalName,
        DateTimeOffset occurredAtUtc)
    {
        if (id.IsEmpty)
        {
            throw new ArgumentException("TenantId cannot be empty.", nameof(id));
        }

        var timestamp = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        var tenant = new Tenant(
            id,
            DomainGuard.RequiredText(displayName, 200, nameof(displayName)),
            DomainGuard.OptionalText(legalName, 250, nameof(legalName)),
            TenantStatus.Draft,
            suspensionReason: null,
            timestamp,
            timestamp,
            version: 0);

        tenant.Raise(new TenantCreatedDomainEvent(
            Guid.NewGuid(),
            timestamp,
            id,
            tenant.DisplayName));

        return tenant;
    }

    public void Rename(string displayName, string? legalName, DateTimeOffset occurredAtUtc)
    {
        EnsureNotClosed();
        DisplayName = DomainGuard.RequiredText(displayName, 200, nameof(displayName));
        LegalName = DomainGuard.OptionalText(legalName, 250, nameof(legalName));
        UpdatedAtUtc = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        MarkChanged();
    }

    public void Activate(DateTimeOffset occurredAtUtc) =>
        TransitionTo(TenantStatus.Active, occurredAtUtc, reason: null, TenantStatus.Draft);

    public void Suspend(string reason, DateTimeOffset occurredAtUtc) =>
        TransitionTo(
            TenantStatus.Suspended,
            occurredAtUtc,
            DomainGuard.RequiredText(reason, 500, nameof(reason)),
            TenantStatus.Active);

    public void Reinstate(DateTimeOffset occurredAtUtc) =>
        TransitionTo(TenantStatus.Active, occurredAtUtc, reason: null, TenantStatus.Suspended);

    public void Close(string reason, DateTimeOffset occurredAtUtc)
    {
        EnsureNotClosed();
        TransitionTo(
            TenantStatus.Closed,
            occurredAtUtc,
            DomainGuard.RequiredText(reason, 500, nameof(reason)),
            Status);
    }

    internal static Tenant Rehydrate(
        TenantId id,
        string displayName,
        string? legalName,
        TenantStatus status,
        string? suspensionReason,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        long version)
    {
        if (id.IsEmpty)
        {
            throw new ArgumentException("TenantId cannot be empty.", nameof(id));
        }

        return new Tenant(
            id,
            DomainGuard.RequiredText(displayName, 200, nameof(displayName)),
            DomainGuard.OptionalText(legalName, 250, nameof(legalName)),
            status,
            DomainGuard.OptionalText(suspensionReason, 500, nameof(suspensionReason)),
            DomainGuard.UtcTimestamp(createdAtUtc, nameof(createdAtUtc)),
            DomainGuard.UtcTimestamp(updatedAtUtc, nameof(updatedAtUtc)),
            version);
    }

    private void TransitionTo(
        TenantStatus next,
        DateTimeOffset occurredAtUtc,
        string? reason,
        params TenantStatus[] allowedCurrentStates)
    {
        if (!allowedCurrentStates.Contains(Status))
        {
            throw new PlatformDomainException(
                "tenant.invalid_status_transition",
                $"Tenant cannot transition from {Status} to {next}.");
        }

        var timestamp = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        var previous = Status;
        Status = next;
        SuspensionReason = next is TenantStatus.Suspended or TenantStatus.Closed
            ? reason
            : null;
        UpdatedAtUtc = timestamp;
        Raise(new TenantLifecycleChangedDomainEvent(
            Guid.NewGuid(),
            timestamp,
            Id,
            previous,
            next,
            reason));
    }

    private void EnsureNotClosed()
    {
        if (Status == TenantStatus.Closed)
        {
            throw new PlatformDomainException(
                "tenant.closed",
                "A closed tenant cannot be changed.");
        }
    }
}
