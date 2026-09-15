using System.Text.Json;
using Mdaresna.Platform.Application.Access.Staff;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Mdaresna.Platform.Infrastructure.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.IdentityAuth.Staff;

public sealed class PlatformRoleManager(
    PlatformDbContext db,
    IPlatformRoleRepository repository,
    IPlatformUnitOfWork unitOfWork,
    IPlatformPermissionEvaluator permissionEvaluator,
    IPlatformNotificationService? notifications = null) : IPlatformRoleManager
{
    public async Task<IReadOnlyList<string>> ListPermissionsAsync(CancellationToken cancellationToken = default)
    {
        var codes = await db.Permissions.AsNoTracking()
            .Select(permission => permission.Code)
            .ToArrayAsync(cancellationToken);
        return codes.Select(code => code.Value).OrderBy(code => code, StringComparer.Ordinal).ToArray();
    }

    public async Task<PlatformRoleCatalogItem> CreateAsync(
        Guid actorAccountId, PlatformRoleSaveRequest request,
        CancellationToken cancellationToken = default, string? correlationId = null)
    {
        var actor = await EnsureManagerAsync(actorAccountId, cancellationToken);
        var codes = ValidatePermissions(request);
        await EnsureKnownPermissionsAsync(codes, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var role = PlatformRole.Create(PlatformRoleId.New(), request.Key, request.DisplayName,
            false, codes, actor, now);
        if (await repository.FindByKeyAsync(role.Key, cancellationToken) is not null)
            throw new PlatformConflictException("role.key_exists", "A role with this key already exists.");

        await repository.AddAsync(role, cancellationToken);
        Audit(actor, "platform.role.created", role, now, correlationId);
        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (DatabaseErrorClassifier.IsUniqueViolation(ex.InnerException))
        {
            throw new PlatformConflictException("role.key_exists", "A role with this key already exists.");
        }
        role.DequeueDomainEvents();
        return ToItem(role);
    }

    public async Task<PlatformRoleCatalogItem> UpdateAsync(
        Guid actorAccountId, Guid roleId, PlatformRoleSaveRequest request,
        CancellationToken cancellationToken = default, string? correlationId = null)
    {
        var actor = await EnsureManagerAsync(actorAccountId, cancellationToken);
        var role = await FindRoleAsync(roleId, cancellationToken);
        EnsureEditable(role);
        if (!role.IsActive)
            throw new PlatformConflictException("role.inactive", "Activate this role before editing it.");
        if (!string.Equals(request.Key?.Trim(), role.Key, StringComparison.OrdinalIgnoreCase))
            throw new PlatformConflictException("role.key_immutable", "A role key cannot be changed.");

        var codes = ValidatePermissions(request);
        await EnsureKnownPermissionsAsync(codes, cancellationToken);
        var permissionsChanged = !role.Permissions.ToHashSet().SetEquals(codes);

        var now = DateTimeOffset.UtcNow;
        var displayNameChanged = !string.Equals(
            role.DisplayName, request.DisplayName.Trim(), StringComparison.Ordinal);
        if (displayNameChanged)
            role.Rename(request.DisplayName, actor, now);
        foreach (var code in codes.Except(role.Permissions).ToArray()) role.Grant(code, actor, now);
        foreach (var code in role.Permissions.Except(codes).ToArray()) role.Revoke(code, actor, now);
        if (displayNameChanged || permissionsChanged)
        {
            Audit(actor, "platform.role.updated", role, now, correlationId);
            if (permissionsChanged)
                await QueuePermissionsChangedAsync(role, cancellationToken);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            role.DequeueDomainEvents();
        }
        return ToItem(role);
    }

    public async Task<PlatformRoleCatalogItem> SetActiveAsync(
        Guid actorAccountId, Guid roleId, bool isActive,
        CancellationToken cancellationToken = default, string? correlationId = null)
    {
        var actor = await EnsureManagerAsync(actorAccountId, cancellationToken);
        var role = await FindRoleAsync(roleId, cancellationToken);
        EnsureEditable(role);
        if (role.IsActive == isActive) return ToItem(role);
        if (!isActive && role.Permissions.Contains(PlatformPermissionCodes.AccessManage) &&
            await HasActiveAssignmentsAsync(role.Id, cancellationToken))
            throw new PlatformConflictException("role.privileged_assigned",
                "An assigned access-management role cannot be deactivated.");

        var now = DateTimeOffset.UtcNow;
        if (isActive) role.Activate(actor, now); else role.Deactivate(actor, now);
        Audit(actor, isActive ? "platform.role.activated" : "platform.role.deactivated", role, now, correlationId);
        await QueuePermissionsChangedAsync(role, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        role.DequeueDomainEvents();
        return ToItem(role);
    }

    public async Task DeleteAsync(
        Guid actorAccountId, Guid roleId,
        CancellationToken cancellationToken = default, string? correlationId = null)
    {
        var actor = await EnsureManagerAsync(actorAccountId, cancellationToken);
        var role = await FindRoleAsync(roleId, cancellationToken);
        EnsureEditable(role);
        // Even revoked assignments are retained for history, and their FK restricts deletion.
        if (await db.RoleAssignments.AnyAsync(x => x.RoleId == role.Id, cancellationToken))
            throw new PlatformConflictException("role.has_users",
                "This role has user assignments and cannot be deleted.");
        var now = DateTimeOffset.UtcNow;
        db.Roles.Remove(role);
        Audit(actor, "platform.role.deleted", role, now, correlationId);
        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new PlatformConflictException("role.has_users_or_changed",
                "This role was assigned or changed; reload it and retry.");
        }
    }

    private async Task<IdentityAccountId> EnsureManagerAsync(Guid accountId, CancellationToken cancellationToken)
    {
        if (accountId == Guid.Empty) throw new ArgumentException("Actor account is required.", nameof(accountId));
        var actor = IdentityAccountId.From(accountId);
        if (!await permissionEvaluator.HasPermissionAsync(actor, PlatformPermissionCodes.AccessManage, cancellationToken))
            throw new PlatformStaffAccessDeniedException();
        return actor;
    }

    private async Task<PlatformRole> FindRoleAsync(Guid roleId, CancellationToken cancellationToken) =>
        roleId == Guid.Empty
            ? throw new ArgumentException("Role ID is required.", nameof(roleId))
            : await repository.FindByIdAsync(PlatformRoleId.From(roleId), cancellationToken)
              ?? throw new PlatformResourceNotFoundException("role.not_found", "The Platform role was not found.");

    private static void EnsureEditable(PlatformRole role)
    {
        if (role.IsSystem)
            throw new PlatformConflictException("role.system_role", "System roles cannot be edited or deleted.");
    }

    private static HashSet<PermissionCode> ValidatePermissions(PlatformRoleSaveRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.Key);
        ArgumentException.ThrowIfNullOrWhiteSpace(request.DisplayName);
        ArgumentNullException.ThrowIfNull(request.PermissionCodes);
        if (request.PermissionCodes.Count > PlatformPermissionCodes.All.Count)
            throw new ArgumentException("Too many permission codes.", nameof(request));
        return request.PermissionCodes.Select(PermissionCode.Create).ToHashSet();
    }

    private async Task EnsureKnownPermissionsAsync(HashSet<PermissionCode> codes, CancellationToken cancellationToken)
    {
        if (!codes.IsSubsetOf(PlatformPermissionCodes.All))
            throw new ArgumentException("One or more permission codes are unknown.");
        var known = await db.Permissions.AsNoTracking().Select(x => x.Code).ToArrayAsync(cancellationToken);
        if (!codes.IsSubsetOf(known.ToHashSet()))
            throw new ArgumentException("One or more permission codes are not configured.");
    }

    private Task<bool> HasActiveAssignmentsAsync(PlatformRoleId roleId, CancellationToken cancellationToken) =>
        db.RoleAssignments.AsNoTracking().AnyAsync(x => x.RoleId == roleId && x.RevokedAtUtc == null, cancellationToken);

    private async Task QueuePermissionsChangedAsync(PlatformRole role, CancellationToken cancellationToken)
    {
        if (notifications is null) return;
        var accounts = await db.RoleAssignments.AsNoTracking()
            .Where(x => x.RoleId == role.Id && x.RevokedAtUtc == null)
            .Select(x => x.AccountId.Value)
            .Distinct()
            .ToArrayAsync(cancellationToken);
        await notifications.QueueAsync(accounts, new PlatformNotificationInput(
            "platform.permissions.changed",
            "تم تغيير صلاحيتك",
            "Your permissions changed",
            "تم تغيير صلاحيات دورك داخل إدارة المنصة، وسيتم تحديث الأجزاء المتاحة لك الآن.",
            "Your Platform role permissions changed. The available sections will refresh now.",
            "/dashboard",
            new Dictionary<string, string> { ["refreshPermissions"] = "true" }), cancellationToken);
    }

    private static PlatformRoleCatalogItem ToItem(PlatformRole role) => new(
        role.Id.Value, role.Key, role.DisplayName, role.IsActive, role.IsSystem,
        role.Permissions.Select(x => x.Value).OrderBy(x => x, StringComparer.Ordinal).ToArray());

    private void Audit(IdentityAccountId actor, string action, PlatformRole role,
        DateTimeOffset now, string? correlationId) => db.AuditEntries.Add(new PlatformAuditEntry
        {
            Id = Guid.NewGuid(), AccountId = actor, Action = action,
            ResourceType = "platform-role", ResourceId = role.Id.Value.ToString("D"),
            OccurredAtUtc = now, CorrelationId = correlationId is { Length: <= 100 } ? correlationId : null,
            MetadataJson = JsonSerializer.Serialize(new { role.Key, role.DisplayName, role.IsActive,
                Permissions = role.Permissions.Select(x => x.Value).OrderBy(x => x).ToArray() })
        });
}
