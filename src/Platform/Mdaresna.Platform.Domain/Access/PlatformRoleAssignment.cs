using Mdaresna.Platform.Domain.Access.Events;
using Mdaresna.Platform.Domain.Common;

namespace Mdaresna.Platform.Domain.Access;

public sealed class PlatformRoleAssignment : AggregateRoot
{
    private PlatformRoleAssignment(
        PlatformRoleAssignmentId id,
        IdentityAccountId accountId,
        PlatformRoleId roleId,
        IdentityAccountId assignedByAccountId,
        DateTimeOffset assignedAtUtc,
        IdentityAccountId? revokedByAccountId,
        DateTimeOffset? revokedAtUtc,
        long version)
    {
        Id = id;
        AccountId = accountId;
        RoleId = roleId;
        AssignedByAccountId = assignedByAccountId;
        AssignedAtUtc = assignedAtUtc;
        RevokedByAccountId = revokedByAccountId;
        RevokedAtUtc = revokedAtUtc;
        RestoreVersion(version);
    }

    public PlatformRoleAssignmentId Id { get; }

    public IdentityAccountId AccountId { get; }

    public PlatformRoleId RoleId { get; }

    public IdentityAccountId AssignedByAccountId { get; }

    public DateTimeOffset AssignedAtUtc { get; }

    public IdentityAccountId? RevokedByAccountId { get; private set; }

    public DateTimeOffset? RevokedAtUtc { get; private set; }

    public bool IsActive => !RevokedAtUtc.HasValue;

    public static PlatformRoleAssignment Assign(
        PlatformRoleAssignmentId id,
        IdentityAccountId accountId,
        PlatformRoleId roleId,
        IdentityAccountId assignedByAccountId,
        DateTimeOffset occurredAtUtc)
    {
        EnsureIds(id, accountId, roleId, assignedByAccountId);
        var timestamp = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        var assignment = new PlatformRoleAssignment(
            id,
            accountId,
            roleId,
            assignedByAccountId,
            timestamp,
            revokedByAccountId: null,
            revokedAtUtc: null,
            version: 0);

        assignment.Raise(new PlatformAccessChangedDomainEvent(
            Guid.NewGuid(),
            timestamp,
            "platform-role-assignment.created",
            (Guid)id,
            (Guid)assignedByAccountId));

        return assignment;
    }

    public void Revoke(IdentityAccountId revokedByAccountId, DateTimeOffset occurredAtUtc)
    {
        if (!IsActive)
        {
            return;
        }

        if (revokedByAccountId.IsEmpty)
        {
            throw new ArgumentException("IdentityAccountId cannot be empty.", nameof(revokedByAccountId));
        }

        var timestamp = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        RevokedByAccountId = revokedByAccountId;
        RevokedAtUtc = timestamp;
        Raise(new PlatformAccessChangedDomainEvent(
            Guid.NewGuid(),
            timestamp,
            "platform-role-assignment.revoked",
            (Guid)Id,
            (Guid)revokedByAccountId));
    }

    internal static PlatformRoleAssignment Rehydrate(
        PlatformRoleAssignmentId id,
        IdentityAccountId accountId,
        PlatformRoleId roleId,
        IdentityAccountId assignedByAccountId,
        DateTimeOffset assignedAtUtc,
        IdentityAccountId? revokedByAccountId,
        DateTimeOffset? revokedAtUtc,
        long version)
    {
        EnsureIds(id, accountId, roleId, assignedByAccountId);

        if (revokedByAccountId is { IsEmpty: true })
        {
            throw new ArgumentException("RevokedByAccountId cannot be empty.", nameof(revokedByAccountId));
        }

        if (revokedAtUtc.HasValue != revokedByAccountId.HasValue)
        {
            throw new ArgumentException("Revocation actor and timestamp must be supplied together.");
        }

        return new PlatformRoleAssignment(
            id,
            accountId,
            roleId,
            assignedByAccountId,
            DomainGuard.UtcTimestamp(assignedAtUtc, nameof(assignedAtUtc)),
            revokedByAccountId,
            revokedAtUtc is null
                ? null
                : DomainGuard.UtcTimestamp(revokedAtUtc.Value, nameof(revokedAtUtc)),
            version);
    }

    private static void EnsureIds(
        PlatformRoleAssignmentId id,
        IdentityAccountId accountId,
        PlatformRoleId roleId,
        IdentityAccountId assignedByAccountId)
    {
        if (id.IsEmpty || accountId.IsEmpty || roleId.IsEmpty || assignedByAccountId.IsEmpty)
        {
            throw new ArgumentException("Role assignment identifiers cannot be empty.");
        }
    }
}
