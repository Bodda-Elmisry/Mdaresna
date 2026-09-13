using System.Data;
using System.Text.Json;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mdaresna.Platform.Bootstrap;

internal sealed class FirstOwnerBootstrapper(
    IdentityDbContext identityDb,
    PlatformDbContext platformDb)
{
    private const string OwnerRoleKey = "app-manager";
    private const string IdentityProvisionedEvent = "platform.bootstrap.first_owner.provisioned";
    private const string PlatformAuditAction = "platform.bootstrap.first_owner.provisioned";
    private const string VerificationMethod = "phone-otp-on-first-activation";

    public async Task<BootstrapResult> RunAsync(
        BootstrapOptions options,
        CancellationToken cancellationToken = default)
    {
        if (!await identityDb.Database.CanConnectAsync(cancellationToken) ||
            !await platformDb.Database.CanConnectAsync(cancellationToken) ||
            (await identityDb.Database.GetPendingMigrationsAsync(cancellationToken)).Any() ||
            (await platformDb.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
        {
            throw new BootstrapRejectedException(
                "Both databases must be reachable with all reviewed migrations applied.");
        }

        // The Platform database is the one-time gate. Its transaction-scoped
        // provider-specific advisory lock serializes concurrent bootstrap processes before any
        // Identity write occurs. The Identity operation marker permits recovery
        // if Identity commits but the Platform transaction subsequently fails.
        await using var transaction = await platformDb.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);
        await AcquireFirstOwnerLockAsync(transaction, cancellationToken);

        var existingAudit = await platformDb.AuditEntries
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == options.OperationId, cancellationToken);
        if (existingAudit is not null)
        {
            var completed = await VerifyCompletedAsync(
                existingAudit, options, cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new BootstrapResult(completed.Account.Id, AlreadyCompleted: true);
        }

        if (await HasAnyActiveAccessManagerAsync(cancellationToken))
        {
            throw new BootstrapRejectedException(
                "A Platform access manager already exists. First-owner bootstrap is closed.");
        }

        if (await platformDb.Roles.AnyAsync(x => x.Key == OwnerRoleKey, cancellationToken))
        {
            throw new BootstrapRejectedException(
                "The App Manager role already exists without this operation marker. Manual review is required.");
        }

        var seededCodes = (await platformDb.Permissions.AsNoTracking()
                .Select(x => x.Code)
                .ToArrayAsync(cancellationToken))
            .ToHashSet();
        if (!PlatformPermissionCodes.All.IsSubsetOf(seededCodes))
        {
            throw new BootstrapRejectedException(
                "Platform permission rows are incomplete; bootstrap refused.");
        }

        var account = await GetOrCreateIdentityAsync(
            options, allowCreate: true, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        var accountId = IdentityAccountId.From(account.Id);
        var role = PlatformRole.Create(
            PlatformRoleId.New(),
            OwnerRoleKey,
            "App Manager",
            isSystem: true,
            PlatformPermissionCodes.All,
            accountId,
            now);
        var assignment = PlatformRoleAssignment.Assign(
            PlatformRoleAssignmentId.New(),
            accountId,
            role.Id,
            accountId,
            now);

        platformDb.Roles.Add(role);
        foreach (var permission in role.Permissions)
        {
            platformDb.RolePermissions.Add(new PlatformRolePermissionRecord
            {
                RoleId = role.Id,
                PermissionCode = permission
            });
        }

        platformDb.RoleAssignments.Add(assignment);
        platformDb.AuditEntries.Add(new PlatformAuditEntry
        {
            Id = options.OperationId,
            AccountId = accountId,
            Action = PlatformAuditAction,
            ResourceType = "platform-first-owner",
            ResourceId = account.Id.ToString("D"),
            OccurredAtUtc = now,
            CorrelationId = options.OperationId.ToString("D"),
            MetadataJson = JsonSerializer.Serialize(new PlatformBootstrapMetadata(
                options.OperationId,
                account.Id,
                role.Id.Value,
                VerificationMethod))
        });

        await platformDb.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return new BootstrapResult(account.Id, AlreadyCompleted: false);
    }

    public async Task<string> InspectAsync(
        BootstrapOptions options,
        CancellationToken cancellationToken = default)
    {
        if (!await identityDb.Database.CanConnectAsync(cancellationToken) ||
            !await platformDb.Database.CanConnectAsync(cancellationToken) ||
            (await identityDb.Database.GetPendingMigrationsAsync(cancellationToken)).Any() ||
            (await platformDb.Database.GetPendingMigrationsAsync(cancellationToken)).Any())
        {
            throw new BootstrapRejectedException(
                "Both databases must be reachable with all reviewed migrations applied.");
        }

        var auditExists = await platformDb.AuditEntries.AsNoTracking()
            .AnyAsync(x => x.Id == options.OperationId, cancellationToken);
        var identityMarkerExists = await identityDb.SecurityEvents.AsNoTracking()
            .AnyAsync(x => x.Id == options.OperationId, cancellationToken);
        var phoneExists = await identityDb.LoginIdentifiers.AsNoTracking()
            .AnyAsync(x => x.Type == LoginIdentifierType.Phone &&
                           x.SchoolId == null &&
                           x.NormalizedValue == options.Phone,
                cancellationToken);
        var roleExists = await platformDb.Roles.AsNoTracking()
            .AnyAsync(x => x.Key == OwnerRoleKey, cancellationToken);
        var managerExists = await HasAnyActiveAccessManagerAsync(cancellationToken);

        if (auditExists)
        {
            return "An operation marker exists; a real rerun will verify and reconcile it.";
        }

        if (managerExists || roleExists || phoneExists && !identityMarkerExists)
        {
            return "Existing account or Platform access state requires manual review; no changes made.";
        }

        return identityMarkerExists
            ? "A matching Identity operation marker may need completion; no changes made."
            : "First-owner bootstrap prerequisites appear ready; no changes made.";
    }

    private async Task<Account> GetOrCreateIdentityAsync(
        BootstrapOptions options,
        bool allowCreate,
        CancellationToken cancellationToken)
    {
        var marker = await identityDb.SecurityEvents.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == options.OperationId, cancellationToken);
        var existingIdentifier = await identityDb.LoginIdentifiers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Type == LoginIdentifierType.Phone &&
                                       x.SchoolId == null &&
                                       x.NormalizedValue == options.Phone,
                cancellationToken);

        if (marker is not null)
        {
            if (marker.EventType != IdentityProvisionedEvent || marker.AccountId is null ||
                marker.MetadataJson is null)
            {
                throw new BootstrapRejectedException(
                    "The operation ID is already used for another Identity event.");
            }

            IdentityBootstrapMetadata? metadata;
            try
            {
                metadata = JsonSerializer.Deserialize<IdentityBootstrapMetadata>(marker.MetadataJson);
            }
            catch (JsonException)
            {
                throw new BootstrapRejectedException("Identity bootstrap marker is invalid.");
            }

            if (metadata is null || metadata.OperationId != options.OperationId ||
                metadata.AccountId != marker.AccountId ||
                metadata.Phone != options.Phone ||
                metadata.VerificationMethod != VerificationMethod ||
                existingIdentifier?.AccountId != marker.AccountId)
            {
                throw new BootstrapRejectedException(
                    "Identity bootstrap marker does not match this phone and operation.");
            }

            var existingAccount = await identityDb.Accounts
                .SingleOrDefaultAsync(x => x.Id == marker.AccountId, cancellationToken);
            if (existingAccount is null ||
                existingAccount.DisplayName != options.DisplayName)
            {
                throw new BootstrapRejectedException(
                    "The existing bootstrap account does not match this operation.");
            }

            return existingAccount;
        }

        if (existingIdentifier is not null)
        {
            throw new BootstrapRejectedException(
                "Phone already belongs to a central account without this bootstrap operation.");
        }

        if (!allowCreate)
        {
            throw new BootstrapRejectedException(
                "Platform bootstrap audit exists but the matching Identity marker is missing.");
        }

        var now = DateTimeOffset.UtcNow;
        var account = new Account
        {
            Id = Guid.NewGuid(),
            Status = AccountStatus.PendingVerification,
            DisplayName = options.DisplayName,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        account.LoginIdentifiers.Add(new LoginIdentifier
        {
            Id = Guid.NewGuid(),
            AccountId = account.Id,
            Type = LoginIdentifierType.Phone,
            NormalizedValue = options.Phone,
            DisplayValue = options.Phone,
            SchoolId = null,
            IsVerified = false,
            VerifiedAtUtc = null,
            CreatedAtUtc = now
        });
        identityDb.Accounts.Add(account);
        identityDb.SecurityEvents.Add(new IdentitySecurityEvent
        {
            Id = options.OperationId,
            AccountId = account.Id,
            EventType = IdentityProvisionedEvent,
            Succeeded = true,
            OccurredAtUtc = now,
            MetadataJson = JsonSerializer.Serialize(new IdentityBootstrapMetadata(
                options.OperationId,
                account.Id,
                options.Phone,
                VerificationMethod))
        });
        await identityDb.SaveChangesAsync(cancellationToken);
        return account;
    }

    private async Task<CompletedBootstrap> VerifyCompletedAsync(
        PlatformAuditEntry audit,
        BootstrapOptions options,
        CancellationToken cancellationToken)
    {
        PlatformBootstrapMetadata? metadata;
        try
        {
            metadata = audit.MetadataJson is null
                ? null
                : JsonSerializer.Deserialize<PlatformBootstrapMetadata>(audit.MetadataJson);
        }
        catch (JsonException)
        {
            throw new BootstrapRejectedException("Platform bootstrap audit is invalid.");
        }

        if (audit.Action != PlatformAuditAction || metadata is null ||
            metadata.OperationId != options.OperationId ||
            audit.AccountId?.Value != metadata.AccountId ||
            audit.ResourceId != metadata.AccountId.ToString("D") ||
            metadata.VerificationMethod != VerificationMethod)
        {
            throw new BootstrapRejectedException("Operation ID conflicts with another Platform audit entry.");
        }

        var account = await GetOrCreateIdentityAsync(
            options, allowCreate: false, cancellationToken);
        if (account.Id != metadata.AccountId)
        {
            throw new BootstrapRejectedException("Identity and Platform owner records disagree.");
        }

        var roleId = PlatformRoleId.From(metadata.RoleId);
        var role = await platformDb.Roles.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == roleId, cancellationToken);
        var hasAssignment = await platformDb.RoleAssignments.AsNoTracking()
            .AnyAsync(x => x.AccountId == IdentityAccountId.From(account.Id) &&
                           x.RoleId == roleId && x.RevokedAtUtc == null,
                cancellationToken);
        var permissionCodes = (await platformDb.RolePermissions.AsNoTracking()
                .Where(x => x.RoleId == roleId)
                .Select(x => x.PermissionCode)
                .ToArrayAsync(cancellationToken))
            .ToHashSet();
        if (role is null || role.Key != OwnerRoleKey || !role.IsSystem || !role.IsActive ||
            !hasAssignment || !PlatformPermissionCodes.All.IsSubsetOf(permissionCodes))
        {
            throw new BootstrapRejectedException("The completed App Manager assignment is incomplete.");
        }

        return new CompletedBootstrap(account);
    }

    private Task<bool> HasAnyActiveAccessManagerAsync(CancellationToken cancellationToken) =>
        (from assignment in platformDb.RoleAssignments.AsNoTracking()
         join role in platformDb.Roles.AsNoTracking()
             on assignment.RoleId equals role.Id
         join permission in platformDb.RolePermissions.AsNoTracking()
             on role.Id equals permission.RoleId
         where assignment.RevokedAtUtc == null && role.IsActive &&
               permission.PermissionCode == PlatformPermissionCodes.AccessManage
         select assignment.Id).AnyAsync(cancellationToken);

    private async Task AcquireFirstOwnerLockAsync(
        IDbContextTransaction transaction,
        CancellationToken cancellationToken)
    {
        try
        {
            await DatabaseAdvisoryLock.AcquireAsync(platformDb, transaction,
                "mdaresna-platform-first-owner-bootstrap", cancellationToken);
        }
        catch (InvalidOperationException)
        {
            throw new BootstrapRejectedException(
                "Another first-owner bootstrap is running or the database lock was refused.");
        }
    }

    private sealed record IdentityBootstrapMetadata(
        Guid OperationId,
        Guid AccountId,
        string Phone,
        string VerificationMethod);

    private sealed record PlatformBootstrapMetadata(
        Guid OperationId,
        Guid AccountId,
        Guid RoleId,
        string VerificationMethod);

    private sealed record CompletedBootstrap(Account Account);
}

internal sealed record BootstrapResult(Guid AccountId, bool AlreadyCompleted);

internal sealed class BootstrapRejectedException(string message) : Exception(message);
