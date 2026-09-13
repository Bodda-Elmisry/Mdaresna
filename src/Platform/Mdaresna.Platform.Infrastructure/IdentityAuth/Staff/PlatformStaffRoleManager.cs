using System.Text.Json;
using Mdaresna.Platform.Application.Access.Staff;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.IdentityAuth.Staff;

/// <summary>
/// Mutates only the Platform database. Identity is consulted read-only, so a
/// cross-database transaction is neither assumed nor attempted.
/// </summary>
public sealed class PlatformStaffRoleManager(
    PlatformDbContext platformDb,
    IdentityDbContext identityDb,
    IPlatformPermissionEvaluator permissionEvaluator) : IPlatformStaffRoleManager
{
    public async Task<PlatformStaffRoleAssignmentResult> AssignAsync(
        Guid actorAccountId,
        Guid targetAccountId,
        Guid roleId,
        CancellationToken cancellationToken = default,
        string? correlationId = null)
    {
        EnsureDistinctValidAccounts(actorAccountId, targetAccountId);
        if (roleId == Guid.Empty)
        {
            throw new ArgumentException("Role ID is required.", nameof(roleId));
        }

        var actor = IdentityAccountId.From(actorAccountId);
        await EnsureCanManageAccessAsync(actor, cancellationToken);

        var verifiedActiveTarget = await identityDb.Accounts.AsNoTracking().AnyAsync(
            x => x.Id == targetAccountId &&
                 x.Status == AccountStatus.Active &&
                 identityDb.LoginIdentifiers.Any(login =>
                     login.AccountId == targetAccountId &&
                     login.SchoolId == null &&
                     (login.Type == LoginIdentifierType.Email ||
                      login.Type == LoginIdentifierType.Phone) &&
                     login.IsVerified),
            cancellationToken);
        if (!verifiedActiveTarget)
        {
            throw new PlatformConflictException(
                "staff.verified_active_account_required",
                "The staff member needs an active central account with a verified email or phone.");
        }

        var parsedRoleId = PlatformRoleId.From(roleId);
        var role = await platformDb.Roles.AsNoTracking().SingleOrDefaultAsync(
            x => x.Id == parsedRoleId,
            cancellationToken);
        if (role is null)
        {
            throw new PlatformResourceNotFoundException(
                "staff.role_not_found",
                "The Platform role was not found.");
        }

        if (!role.IsActive)
        {
            throw new PlatformConflictException(
                "staff.role_inactive",
                "An inactive Platform role cannot be assigned.");
        }

        var target = IdentityAccountId.From(targetAccountId);
        var existing = await platformDb.RoleAssignments.AsNoTracking().SingleOrDefaultAsync(
            x => x.AccountId == target &&
                 x.RoleId == parsedRoleId &&
                 x.RevokedAtUtc == null,
            cancellationToken);
        if (existing is not null)
        {
            return new PlatformStaffRoleAssignmentResult(
                existing.Id.Value, targetAccountId, roleId, false);
        }

        var now = DateTimeOffset.UtcNow;
        var assignment = PlatformRoleAssignment.Assign(
            PlatformRoleAssignmentId.New(),
            target,
            parsedRoleId,
            actor,
            now);
        platformDb.RoleAssignments.Add(assignment);
        AddAudit(
            actor,
            "platform.staff.role.assigned",
            assignment.Id.Value,
            targetAccountId,
            roleId,
            now,
            correlationId);
        try
        {
            await platformDb.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            throw new PlatformConflictException(
                "staff.role_already_assigned",
                "This Platform role is already assigned to the staff member.");
        }

        assignment.DequeueDomainEvents();
        return new PlatformStaffRoleAssignmentResult(
            assignment.Id.Value, targetAccountId, roleId, true);
    }

    public async Task<PlatformStaffRoleAssignmentResult> RevokeAsync(
        Guid actorAccountId,
        Guid assignmentId,
        CancellationToken cancellationToken = default,
        string? correlationId = null)
    {
        if (actorAccountId == Guid.Empty || assignmentId == Guid.Empty)
        {
            throw new ArgumentException("Actor and assignment IDs are required.");
        }

        var actor = IdentityAccountId.From(actorAccountId);
        await EnsureCanManageAccessAsync(actor, cancellationToken);

        var parsedAssignmentId = PlatformRoleAssignmentId.From(assignmentId);
        var assignment = await platformDb.RoleAssignments.SingleOrDefaultAsync(
            x => x.Id == parsedAssignmentId,
            cancellationToken) ?? throw new PlatformResourceNotFoundException(
                "staff.assignment_not_found",
                "The Platform role assignment was not found.");

        if (assignment.AccountId == actor)
        {
            throw new PlatformConflictException(
                "staff.self_revocation_forbidden",
                "An operator cannot revoke their own Platform role.");
        }

        var result = new PlatformStaffRoleAssignmentResult(
            assignment.Id.Value,
            assignment.AccountId.Value,
            assignment.RoleId.Value,
            assignment.IsActive);
        if (!assignment.IsActive)
        {
            return result with { Changed = false };
        }

        // Until a transactional recovery-owner workflow exists, privileged
        // assignments cannot be revoked here. This protects the last admin.
        var grantsAccessManage = await platformDb.RolePermissions.AsNoTracking().AnyAsync(
            x => x.RoleId == assignment.RoleId &&
                 x.PermissionCode == PlatformPermissionCodes.AccessManage,
            cancellationToken);
        if (grantsAccessManage)
        {
            throw new PlatformConflictException(
                "staff.privileged_revocation_requires_recovery_workflow",
                "An access-management role cannot be revoked through this workflow.");
        }

        var now = DateTimeOffset.UtcNow;
        assignment.Revoke(actor, now);
        AddAudit(
            actor,
            "platform.staff.role.revoked",
            assignment.Id.Value,
            assignment.AccountId.Value,
            assignment.RoleId.Value,
            now,
            correlationId);
        try
        {
            await platformDb.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new PlatformConflictException(
                "staff.assignment_changed",
                "The role assignment changed; reload it and retry.");
        }

        assignment.DequeueDomainEvents();
        return result;
    }

    private async Task EnsureCanManageAccessAsync(
        IdentityAccountId actor,
        CancellationToken cancellationToken)
    {
        if (!await permissionEvaluator.HasPermissionAsync(
                actor,
                PlatformPermissionCodes.AccessManage,
                cancellationToken))
        {
            throw new PlatformStaffAccessDeniedException();
        }
    }

    private void AddAudit(
        IdentityAccountId actor,
        string action,
        Guid assignmentId,
        Guid targetAccountId,
        Guid roleId,
        DateTimeOffset now,
        string? correlationId) =>
        platformDb.AuditEntries.Add(new PlatformAuditEntry
        {
            Id = Guid.NewGuid(),
            AccountId = actor,
            Action = action,
            ResourceType = "platform-role-assignment",
            ResourceId = assignmentId.ToString("D"),
            OccurredAtUtc = now,
            CorrelationId = correlationId is { Length: <= 100 }
                ? correlationId
                : null,
            MetadataJson = JsonSerializer.Serialize(new
            {
                TargetAccountId = targetAccountId,
                RoleId = roleId
            })
        });

    private static void EnsureDistinctValidAccounts(Guid actorAccountId, Guid targetAccountId)
    {
        if (actorAccountId == Guid.Empty || targetAccountId == Guid.Empty)
        {
            throw new ArgumentException("Actor and target account IDs are required.");
        }

        if (actorAccountId == targetAccountId)
        {
            throw new PlatformConflictException(
                "staff.self_assignment_forbidden",
                "An operator cannot assign themselves a Platform role.");
        }
    }
}
