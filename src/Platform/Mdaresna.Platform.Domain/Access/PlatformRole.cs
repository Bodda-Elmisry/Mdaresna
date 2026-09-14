using System.Text.RegularExpressions;
using Mdaresna.Platform.Domain.Access.Events;
using Mdaresna.Platform.Domain.Common;

namespace Mdaresna.Platform.Domain.Access;

public sealed partial class PlatformRole : AggregateRoot
{
    private readonly HashSet<PermissionCode> _permissions;

    private PlatformRole()
    {
        Id = default;
        Key = string.Empty;
        DisplayName = string.Empty;
        _permissions = [];
    }

    private PlatformRole(
        PlatformRoleId id,
        string key,
        string displayName,
        bool isSystem,
        bool isActive,
        IEnumerable<PermissionCode> permissions,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        long version)
    {
        Id = id;
        Key = key;
        DisplayName = displayName;
        IsSystem = isSystem;
        IsActive = isActive;
        _permissions = new HashSet<PermissionCode>(permissions);
        CreatedAtUtc = createdAtUtc;
        UpdatedAtUtc = updatedAtUtc;
        RestoreVersion(version);
    }

    public PlatformRoleId Id { get; }

    public string Key { get; }

    public string DisplayName { get; private set; }

    public bool IsSystem { get; }

    public bool IsActive { get; private set; }

    public IReadOnlySet<PermissionCode> Permissions => _permissions;

    public DateTimeOffset CreatedAtUtc { get; }

    public DateTimeOffset UpdatedAtUtc { get; private set; }

    public static PlatformRole Create(
        PlatformRoleId id,
        string key,
        string displayName,
        bool isSystem,
        IEnumerable<PermissionCode> permissions,
        IdentityAccountId changedByAccountId,
        DateTimeOffset occurredAtUtc)
    {
        if (id.IsEmpty)
        {
            throw new ArgumentException("PlatformRoleId cannot be empty.", nameof(id));
        }

        if (changedByAccountId.IsEmpty)
        {
            throw new ArgumentException("IdentityAccountId cannot be empty.", nameof(changedByAccountId));
        }

        ArgumentNullException.ThrowIfNull(permissions);
        var timestamp = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        var validatedPermissions = permissions.ToArray();
        EnsurePermissions(validatedPermissions);
        var role = new PlatformRole(
            id,
            NormalizeKey(key),
            DomainGuard.RequiredText(displayName, 100, nameof(displayName)),
            isSystem,
            isActive: true,
            validatedPermissions,
            timestamp,
            timestamp,
            version: 0);

        role.RaiseAccessChange("platform-role.created", changedByAccountId, timestamp);
        return role;
    }

    public void Rename(
        string displayName,
        IdentityAccountId changedByAccountId,
        DateTimeOffset occurredAtUtc)
    {
        EnsureActive();
        DisplayName = DomainGuard.RequiredText(displayName, 100, nameof(displayName));
        var timestamp = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        UpdatedAtUtc = timestamp;
        RaiseAccessChange("platform-role.renamed", changedByAccountId, timestamp);
    }

    public void Grant(
        PermissionCode permission,
        IdentityAccountId changedByAccountId,
        DateTimeOffset occurredAtUtc)
    {
        EnsureActive();
        EnsurePermissions([permission]);
        var timestamp = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));

        if (_permissions.Add(permission))
        {
            UpdatedAtUtc = timestamp;
            RaiseAccessChange("platform-role.permission-granted", changedByAccountId, timestamp);
        }
    }

    public void Revoke(
        PermissionCode permission,
        IdentityAccountId changedByAccountId,
        DateTimeOffset occurredAtUtc)
    {
        EnsureActive();
        var timestamp = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));

        if (_permissions.Remove(permission))
        {
            UpdatedAtUtc = timestamp;
            RaiseAccessChange("platform-role.permission-revoked", changedByAccountId, timestamp);
        }
    }

    public void Deactivate(IdentityAccountId changedByAccountId, DateTimeOffset occurredAtUtc)
    {
        EnsureActive();

        if (IsSystem)
        {
            throw new PlatformDomainException(
                "platform_role.system_role",
                "A system platform role cannot be deactivated.");
        }

        var timestamp = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        IsActive = false;
        UpdatedAtUtc = timestamp;
        RaiseAccessChange("platform-role.deactivated", changedByAccountId, timestamp);
    }

    public void Activate(IdentityAccountId changedByAccountId, DateTimeOffset occurredAtUtc)
    {
        if (IsActive)
        {
            return;
        }

        var timestamp = DomainGuard.UtcTimestamp(occurredAtUtc, nameof(occurredAtUtc));
        IsActive = true;
        UpdatedAtUtc = timestamp;
        RaiseAccessChange("platform-role.activated", changedByAccountId, timestamp);
    }

    internal static PlatformRole Rehydrate(
        PlatformRoleId id,
        string key,
        string displayName,
        bool isSystem,
        bool isActive,
        IEnumerable<PermissionCode> permissions,
        DateTimeOffset createdAtUtc,
        DateTimeOffset updatedAtUtc,
        long version)
    {
        ArgumentNullException.ThrowIfNull(permissions);
        var validatedPermissions = permissions.ToArray();
        EnsurePermissions(validatedPermissions);
        return new PlatformRole(
            id,
            NormalizeKey(key),
            DomainGuard.RequiredText(displayName, 100, nameof(displayName)),
            isSystem,
            isActive,
            validatedPermissions,
            DomainGuard.UtcTimestamp(createdAtUtc, nameof(createdAtUtc)),
            DomainGuard.UtcTimestamp(updatedAtUtc, nameof(updatedAtUtc)),
            version);
    }

    internal void RestorePermissions(IEnumerable<PermissionCode> permissions)
    {
        ArgumentNullException.ThrowIfNull(permissions);
        var validatedPermissions = permissions.ToArray();
        EnsurePermissions(validatedPermissions);
        _permissions.Clear();
        _permissions.UnionWith(validatedPermissions);
    }

    private void RaiseAccessChange(
        string changeType,
        IdentityAccountId changedByAccountId,
        DateTimeOffset occurredAtUtc)
    {
        if (changedByAccountId.IsEmpty)
        {
            throw new ArgumentException("IdentityAccountId cannot be empty.", nameof(changedByAccountId));
        }

        Raise(new PlatformAccessChangedDomainEvent(
            Guid.NewGuid(),
            occurredAtUtc,
            changeType,
            (Guid)Id,
            (Guid)changedByAccountId));
    }

    private void EnsureActive()
    {
        if (!IsActive)
        {
            throw new PlatformDomainException(
                "platform_role.inactive",
                "An inactive platform role cannot be changed.");
        }
    }

    private static string NormalizeKey(string key)
    {
        var normalized = DomainGuard.RequiredText(key, 64, nameof(key)).ToLowerInvariant();

        if (!ValidRoleKey().IsMatch(normalized))
        {
            throw new ArgumentException(
                "Role keys must use lowercase letters, digits, and internal hyphens.",
                nameof(key));
        }

        return normalized;
    }

    private static void EnsurePermissions(IEnumerable<PermissionCode> permissions)
    {
        if (permissions.Any(permission => string.IsNullOrWhiteSpace(permission.Value)))
        {
            throw new ArgumentException("Permission codes cannot be empty.", nameof(permissions));
        }
    }

    [GeneratedRegex("^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.CultureInvariant)]
    private static partial Regex ValidRoleKey();
}
