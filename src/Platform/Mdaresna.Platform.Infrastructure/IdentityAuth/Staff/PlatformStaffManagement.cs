using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Mdaresna.Platform.Application.Access.Staff;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Mdaresna.Platform.Infrastructure.Messaging;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Mdaresna.Platform.Infrastructure.IdentityAuth.Staff;

/// <summary>Invitation state and passwords are Platform-owned; shared person facts stay in Identity.</summary>
public sealed class PlatformStaffManagement(
    PlatformDbContext platformDb,
    IdentityDbContext identityDb,
    IPlatformPermissionEvaluator permissionEvaluator,
    IPlatformSmsSender smsSender,
    PlatformPasswordCredentialFactory credentialFactory,
    IPasswordHasher<Account> passwordHasher,
    PlatformActivationOptions activationOptions,
    ILogger<PlatformStaffManagement> logger,
    IPlatformNotificationService? notifications = null) : IPlatformStaffManagement
{
    private const string ProvisionedEvent = "platform.staff.identity.provisioned";
    private const int MaximumFailedAttempts = 5;
    private const int MaximumDailySends = 10;
    private static readonly TimeSpan CodeLifetime = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan SendCooldown = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan SendWindow = TimeSpan.FromDays(1);
    private static readonly Regex UserNamePattern = new("^[a-z][a-z0-9._-]{2,49}$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public async Task<PlatformStaffLookupResult> LookupAsync(Guid actorAccountId, string phone,
        CancellationToken cancellationToken = default)
    {
        await EnsureManagerAsync(actorAccountId, cancellationToken);
        var normalized = NormalizePhone(phone);
        var identifier = await identityDb.LoginIdentifiers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Type == LoginIdentifierType.Phone &&
                x.SchoolId == null && x.NormalizedValue == normalized, cancellationToken);
        if (identifier is null) return new(false, null, null, false, null);
        if (!identifier.IsPrimary)
            throw new PlatformConflictException("staff.primary_phone_required", "Use the account's primary phone.");
        var account = await identityDb.Accounts.AsNoTracking()
            .SingleAsync(x => x.Id == identifier.AccountId, cancellationToken);
        var email = await identityDb.LoginIdentifiers.AsNoTracking()
            .Where(x => x.AccountId == account.Id && x.Type == LoginIdentifierType.Email && x.IsPrimary)
            .Select(x => x.DisplayValue).SingleOrDefaultAsync(cancellationToken);
        var hasImage = await identityDb.AccountProfileImages.AsNoTracking()
            .AnyAsync(x => x.AccountId == account.Id, cancellationToken);
        var local = await platformDb.LocalUsers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.PersonId == account.Id, cancellationToken);
        return new(true, account.DisplayName, email, hasImage,
            local?.Status == "PendingActivation" ? "Requested" : local?.Status);
    }

    public async Task<Guid> InviteAsync(Guid actorAccountId, InvitePlatformStaffRequest request,
        CancellationToken cancellationToken = default, string? correlationId = null)
    {
        var actor = await EnsureManagerAsync(actorAccountId, cancellationToken);
        ArgumentNullException.ThrowIfNull(request);
        var phone = NormalizePhone(request.Phone);
        var userName = NormalizeUserName(request.UserName);
        var displayName = request.DisplayName?.Trim();
        if (displayName?.Length > 200) throw new ArgumentException("Display name is too long.", nameof(request));
        var selectedRoleIds = request.RoleIds?.Distinct().ToArray()
            ?? throw new ArgumentException("At least one role is required.", nameof(request));
        if (selectedRoleIds.Length is < 1 or > 10 || selectedRoleIds.Any(id => id == Guid.Empty))
            throw new ArgumentException("Select between one and ten roles.", nameof(request));
        var roleIds = selectedRoleIds.Select(PlatformRoleId.From).ToArray();
        var roles = await platformDb.Roles.AsNoTracking()
            .Where(role => roleIds.Contains(role.Id) && role.IsActive)
            .Select(role => role.Id).ToArrayAsync(cancellationToken);
        if (roles.Length != roleIds.Length)
            throw new PlatformConflictException("staff.roles_invalid", "Select active roles only.");
        if (await platformDb.LocalUsers.AsNoTracking().AnyAsync(x => x.NormalizedUserName == userName.ToUpperInvariant(), cancellationToken))
            throw new PlatformConflictException("staff.username_taken", "This Platform username is already in use.");

        var identifier = await identityDb.LoginIdentifiers.Include(x => x.Account)
            .SingleOrDefaultAsync(x => x.Type == LoginIdentifierType.Phone && x.SchoolId == null &&
                x.NormalizedValue == phone, cancellationToken);
        Account account;
        var now = DateTimeOffset.UtcNow;
        if (identifier is null)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("A name is required for a new account.", nameof(request));
            account = new Account
            {
                Id = Guid.NewGuid(), Status = AccountStatus.PendingVerification,
                DisplayName = displayName, CreatedAtUtc = now, UpdatedAtUtc = now
            };
            account.LoginIdentifiers.Add(new LoginIdentifier
            {
                Id = Guid.NewGuid(), AccountId = account.Id, Type = LoginIdentifierType.Phone,
                NormalizedValue = phone, DisplayValue = phone, SchoolId = null,
                IsVerified = false, IsPrimary = true, CreatedAtUtc = now
            });
            identityDb.Accounts.Add(account);
            identityDb.SecurityEvents.Add(new IdentitySecurityEvent
            {
                Id = Guid.NewGuid(), AccountId = account.Id, EventType = ProvisionedEvent,
                Succeeded = true, OccurredAtUtc = now
            });
            try { await identityDb.SaveChangesAsync(cancellationToken); }
            catch (DbUpdateException ex) when (DatabaseErrorClassifier.IsUniqueViolation(ex.InnerException))
            {
                throw new PlatformConflictException("staff.phone_taken", "This phone was just registered; retry lookup.");
            }
        }
        else
        {
            if (!identifier.IsPrimary)
                throw new PlatformConflictException("staff.primary_phone_required", "Use the account's primary phone.");
            account = identifier.Account;
            var ownedPending = account.Status == AccountStatus.PendingVerification &&
                await identityDb.SecurityEvents.AsNoTracking().AnyAsync(x => x.AccountId == account.Id &&
                    x.EventType == ProvisionedEvent && x.Succeeded, cancellationToken);
            if (!(account.Status == AccountStatus.Active && identifier.IsVerified) && !ownedPending)
                throw new PlatformConflictException("staff.account_unavailable",
                    "The central account must be active with a verified primary phone.");
        }
        if (account.Id == actor.Value)
            throw new PlatformConflictException("staff.self_invitation_forbidden", "An operator cannot invite themselves.");
        if (await platformDb.LocalUsers.AsNoTracking().AnyAsync(x => x.PersonId == account.Id, cancellationToken))
            throw new PlatformConflictException("staff.already_exists", "This person already belongs to Platform staff.");

        var local = new PlatformLocalUser
        {
            Id = Guid.NewGuid(), PersonId = account.Id, UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            DisplayName = displayName ?? account.DisplayName, Status = "PendingActivation",
            CreatedAtUtc = now, UpdatedAtUtc = now
        };
        platformDb.LocalUsers.Add(local);
        foreach (var roleId in roles)
        {
            platformDb.RoleAssignments.Add(PlatformRoleAssignment.Assign(
                PlatformRoleAssignmentId.New(), IdentityAccountId.From(account.Id), roleId, actor, now));
        }
        Audit(actor, "platform.staff.invited", account.Id, now, correlationId,
            new { UserName = userName, RoleIds = selectedRoleIds });
        try { await platformDb.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) when (DatabaseErrorClassifier.IsUniqueViolation(ex.InnerException))
        {
            throw new PlatformConflictException("staff.already_exists", "Staff member or username already exists.");
        }
        await SendInvitationAsync(local.Id, phone, userName, cancellationToken);
        return account.Id;
    }

    public async Task StartActivationAsync(string phone, CancellationToken cancellationToken = default)
    {
        if (!TryNormalizePhone(phone, out var normalized)) return;
        var identifier = await identityDb.LoginIdentifiers.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Type == LoginIdentifierType.Phone && x.SchoolId == null &&
                x.IsPrimary && x.NormalizedValue == normalized, cancellationToken);
        if (identifier is null) return;
        var user = await platformDb.LocalUsers.Include(x => x.StaffInvitationChallenge)
            .SingleOrDefaultAsync(x => x.PersonId == identifier.AccountId && x.Status == "PendingActivation",
                cancellationToken);
        if (user is null) return;
        var challenge = user.StaffInvitationChallenge;
        var now = DateTimeOffset.UtcNow;
        if (challenge is not null &&
            (challenge.ConsumedAtUtc is null && now - challenge.LastSentAtUtc < SendCooldown ||
             now - challenge.SendWindowStartUtc < SendWindow && challenge.SendCount >= MaximumDailySends))
            return;
        var code = NewCode();
        if (challenge is null)
        {
            challenge = new PlatformStaffInvitationChallenge
            {
                UserId = user.Id, CodeHash = HashCode(user.Id, code), CreatedAtUtc = now,
                ExpiresAtUtc = now.Add(CodeLifetime), LastSentAtUtc = now,
                SendWindowStartUtc = now, SendCount = 1
            };
            user.StaffInvitationChallenge = challenge;
        }
        else
        {
            if (now - challenge.SendWindowStartUtc >= SendWindow)
            {
                challenge.SendWindowStartUtc = now;
                challenge.SendCount = 0;
            }
            challenge.CodeHash = HashCode(user.Id, code);
            challenge.CreatedAtUtc = now;
            challenge.ExpiresAtUtc = now.Add(CodeLifetime);
            challenge.ConsumedAtUtc = null;
            challenge.LastSentAtUtc = now;
            challenge.SendCount++;
            challenge.FailedAttempts = 0;
        }
        Audit(IdentityAccountId.From(user.PersonId), "platform.staff.activation_code_requested", user.PersonId, now, null);
        try { await platformDb.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateConcurrencyException) { return; }
        catch (DbUpdateException ex) when (DatabaseErrorClassifier.IsUniqueViolation(ex.InnerException)) { return; }
        await SendCodeAsync(user.Id, normalized, user.UserName, code, cancellationToken);
    }

    public async Task<bool> CompleteActivationAsync(string phone, string code, string password,
        CancellationToken cancellationToken = default)
    {
        if (!TryNormalizePhone(phone, out var normalized) || code is not { Length: 8 } ||
            !code.All(char.IsAsciiDigit) || password is not { Length: >= 12 and <= 1024 } ||
            password.Any(char.IsControl)) return false;
        var identifier = await identityDb.LoginIdentifiers.Include(x => x.Account)
            .SingleOrDefaultAsync(x => x.Type == LoginIdentifierType.Phone && x.SchoolId == null &&
                x.IsPrimary && x.NormalizedValue == normalized, cancellationToken);
        if (identifier is null || identifier.Account.Status is not (AccountStatus.Active or AccountStatus.PendingVerification))
            return false;
        var account = identifier.Account;
        var local = await platformDb.LocalUsers.Include(x => x.Credential).Include(x => x.StaffInvitationChallenge)
            .SingleOrDefaultAsync(x => x.PersonId == account.Id, cancellationToken);
        var challenge = local?.StaffInvitationChallenge;
        if (local is null || challenge is null ||
            !CryptographicOperations.FixedTimeEquals(challenge.CodeHash, HashCode(local.Id, code)))
        {
            if (local?.Status == "PendingActivation" && challenge is { ConsumedAtUtc: null } &&
                challenge.ExpiresAtUtc > DateTimeOffset.UtcNow && challenge.FailedAttempts < MaximumFailedAttempts)
            {
                challenge.FailedAttempts++;
                if (challenge.FailedAttempts >= MaximumFailedAttempts) challenge.ConsumedAtUtc = DateTimeOffset.UtcNow;
                await platformDb.SaveChangesAsync(cancellationToken);
            }
            return false;
        }
        var now = DateTimeOffset.UtcNow;
        if (local.Status == "PendingActivation" && challenge.ConsumedAtUtc is null &&
            challenge.ExpiresAtUtc > now && challenge.FailedAttempts < MaximumFailedAttempts)
        {
            if (!await platformDb.RoleAssignments.AsNoTracking().AnyAsync(x =>
                    x.AccountId == IdentityAccountId.From(account.Id) && x.RevokedAtUtc == null,
                    cancellationToken)) return false;
            local.Credential = credentialFactory.CreateLocal(account, local, password, now);
            local.Status = "Active";
            local.UpdatedAtUtc = now;
            challenge.ConsumedAtUtc = now;
            Audit(IdentityAccountId.From(account.Id), "platform.staff.invitation_completed", account.Id, now, null);
            try { await platformDb.SaveChangesAsync(cancellationToken); }
            catch (DbUpdateConcurrencyException) { return false; }
        }
        else if (!(local.Status == "Active" && account.Status == AccountStatus.PendingVerification &&
                   challenge.ConsumedAtUtc is not null && local.Credential is { } credential &&
                   passwordHasher.VerifyHashedPassword(account, credential.PasswordHash, password) !=
                   PasswordVerificationResult.Failed)) return false;

        if (account.Status == AccountStatus.PendingVerification)
        {
            var owned = await identityDb.SecurityEvents.AsNoTracking().AnyAsync(x => x.AccountId == account.Id &&
                x.EventType == ProvisionedEvent && x.Succeeded, cancellationToken);
            if (!owned) return false;
            identifier.IsVerified = true;
            identifier.VerifiedAtUtc = now;
            account.Status = AccountStatus.Active;
            account.UpdatedAtUtc = now;
            identityDb.SecurityEvents.Add(new IdentitySecurityEvent
            {
                Id = Guid.NewGuid(), AccountId = account.Id,
                EventType = "platform.staff.phone_verified", Succeeded = true, OccurredAtUtc = now
            });
            try { await identityDb.SaveChangesAsync(cancellationToken); }
            catch (DbUpdateConcurrencyException) { return false; }
        }
        await QueueStaffActivatedNotificationAsync(account.Id,
            local.DisplayName ?? account.DisplayName ?? local.UserName, cancellationToken);
        return true;
    }

    public async Task UpdateAsync(Guid actorAccountId, Guid accountId, UpdatePlatformStaffRequest request,
        CancellationToken cancellationToken = default, string? correlationId = null)
    {
        var actor = await EnsureManagerAsync(actorAccountId, cancellationToken);
        ArgumentNullException.ThrowIfNull(request);
        var user = await FindLocalAsync(accountId, cancellationToken);
        var userName = NormalizeUserName(request.UserName);
        var displayName = request.DisplayName?.Trim();
        if (displayName?.Length > 200) throw new ArgumentException("Display name is too long.", nameof(request));
        if (user.Status == "PendingActivation" && user.NormalizedUserName != userName.ToUpperInvariant())
            throw new PlatformConflictException("staff.pending_username", "Complete activation before changing the username.");
        if (user.NormalizedUserName != userName.ToUpperInvariant() &&
            await platformDb.LocalUsers.AsNoTracking().AnyAsync(x => x.NormalizedUserName == userName.ToUpperInvariant(), cancellationToken))
            throw new PlatformConflictException("staff.username_taken", "This Platform username is already in use.");
        user.UserName = userName;
        user.NormalizedUserName = userName.ToUpperInvariant();
        user.DisplayName = string.IsNullOrWhiteSpace(displayName) ? null : displayName;
        user.UpdatedAtUtc = DateTimeOffset.UtcNow;
        Audit(actor, "platform.staff.updated", accountId, user.UpdatedAtUtc, correlationId,
            new { user.UserName, user.DisplayName });
        try { await platformDb.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException ex) when (DatabaseErrorClassifier.IsUniqueViolation(ex.InnerException))
        {
            throw new PlatformConflictException("staff.username_taken", "This Platform username is already in use.");
        }
    }

    public async Task SetActiveAsync(Guid actorAccountId, Guid accountId, bool isActive,
        CancellationToken cancellationToken = default, string? correlationId = null)
    {
        var actor = await EnsureManagerAsync(actorAccountId, cancellationToken);
        if (actorAccountId == accountId)
            throw new PlatformConflictException("staff.self_status_change", "An operator cannot change their own status.");
        var user = await FindLocalAsync(accountId, cancellationToken);
        if (isActive)
        {
            if (user.Status == "Active") return;
            if (user.Status != "Disabled" || user.Credential is null)
                throw new PlatformConflictException("staff.activation_required",
                    "The invited employee must set a password before activation.");
            user.Status = "Active";
        }
        else
        {
            if (user.Status == "Disabled") return;
            var privileged = await (
                from assignment in platformDb.RoleAssignments.AsNoTracking()
                join permission in platformDb.RolePermissions.AsNoTracking() on assignment.RoleId equals permission.RoleId
                where assignment.AccountId == IdentityAccountId.From(accountId) &&
                      assignment.RevokedAtUtc == null && permission.PermissionCode == PlatformPermissionCodes.AccessManage
                select assignment.Id).AnyAsync(cancellationToken);
            if (privileged)
                throw new PlatformConflictException("staff.privileged_status_change",
                    "An access-management employee cannot be disabled through this workflow.");
            user.Status = "Disabled";
            if (user.StaffInvitationChallenge is { ConsumedAtUtc: null } challenge)
                challenge.ConsumedAtUtc = DateTimeOffset.UtcNow;
        }
        user.UpdatedAtUtc = DateTimeOffset.UtcNow;
        Audit(actor, isActive ? "platform.staff.enabled" : "platform.staff.disabled",
            accountId, user.UpdatedAtUtc, correlationId);
        await platformDb.SaveChangesAsync(cancellationToken);
    }

    private async Task<PlatformLocalUser> FindLocalAsync(Guid accountId, CancellationToken ct) =>
        accountId == Guid.Empty
            ? throw new ArgumentException("Account ID is required.", nameof(accountId))
            : await platformDb.LocalUsers.Include(x => x.Credential).Include(x => x.StaffInvitationChallenge)
                  .SingleOrDefaultAsync(x => x.PersonId == accountId, ct)
              ?? throw new PlatformResourceNotFoundException("staff.not_found", "Platform staff member was not found.");

    private async Task<IdentityAccountId> EnsureManagerAsync(Guid accountId, CancellationToken ct)
    {
        if (accountId == Guid.Empty) throw new ArgumentException("Actor account is required.", nameof(accountId));
        var actor = IdentityAccountId.From(accountId);
        if (!await permissionEvaluator.HasPermissionAsync(actor, PlatformPermissionCodes.AccessManage, ct))
            throw new PlatformStaffAccessDeniedException();
        return actor;
    }

    private async Task SendCodeAsync(Guid userId, string phone, string userName, string code, CancellationToken ct)
    {
        try
        {
            await smsSender.SendAsync(phone,
                PlatformStaffSmsMessages.ActivationCode(userName, code),
                PlatformStaffSmsMessages.ActivationType, null, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning("Platform staff activation code SMS delivery failed for user {UserId}", userId);
            platformDb.ChangeTracker.Clear();
            var challenge = await platformDb.StaffInvitationChallenges.SingleOrDefaultAsync(x => x.UserId == userId, ct);
            if (challenge is { ConsumedAtUtc: null } &&
                CryptographicOperations.FixedTimeEquals(challenge.CodeHash, HashCode(userId, code)))
            {
                challenge.ConsumedAtUtc = DateTimeOffset.UtcNow;
                await platformDb.SaveChangesAsync(ct);
            }
            throw new PlatformConflictException("staff.sms_delivery_failed",
                "The activation code SMS could not be delivered. Retry the code request.");
        }
    }

    private async Task SendInvitationAsync(Guid userId, string phone, string userName, CancellationToken ct)
    {
        try
        {
            await smsSender.SendAsync(phone,
                PlatformStaffSmsMessages.Invitation(userName),
                PlatformStaffSmsMessages.InvitationType, null, ct);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Platform staff invitation SMS delivery failed for user {UserId}", userId);
            throw new PlatformConflictException("staff.invitation_sms_failed",
                "The employee was added, but the invitation SMS could not be delivered.");
        }
    }

    private async Task QueueStaffActivatedNotificationAsync(
        Guid activatedAccountId,
        string displayName,
        CancellationToken cancellationToken)
    {
        if (notifications is null) return;

        var assignedManagers = await (
            from assignment in platformDb.RoleAssignments.AsNoTracking()
            join role in platformDb.Roles.AsNoTracking() on assignment.RoleId equals role.Id
            where assignment.RevokedAtUtc == null && role.IsActive && role.Key == "app-manager"
            select assignment.AccountId).Distinct().ToArrayAsync(cancellationToken);
        var assignedManagerIds = assignedManagers.Select(accountId => accountId.Value)
            .Where(accountId => accountId != activatedAccountId)
            .ToArray();
        if (assignedManagerIds.Length == 0) return;

        var activeManagerIds = await platformDb.LocalUsers.AsNoTracking()
            .Where(user => user.Status == "Active" && assignedManagerIds.Contains(user.PersonId))
            .Select(user => user.PersonId)
            .ToArrayAsync(cancellationToken);
        if (activeManagerIds.Length == 0) return;

        await notifications.QueueAsync(activeManagerIds, new PlatformNotificationInput(
            "platform.staff.activated",
            "تم تفعيل موظف جديد",
            "A new employee was activated",
            $"قام الموظف {displayName} بتفعيل حسابه في إدارة منصة مدارسنا لأول مرة.",
            $"{displayName} activated their Mdaresna Platform administration account for the first time.",
            "/staff",
            new Dictionary<string, string>
            {
                ["staffAccountId"] = activatedAccountId.ToString("D")
            }), cancellationToken);
        await platformDb.SaveChangesAsync(cancellationToken);
    }

    private byte[] HashCode(Guid userId, string code) => HMACSHA256.HashData(
        activationOptions.CodeHashKey, Encoding.UTF8.GetBytes($"mdaresna-platform-staff-invitation:{userId:N}:{code}"));

    private static string NewCode() => RandomNumberGenerator.GetInt32(0, 100_000_000).ToString("D8");

    private static string NormalizePhone(string value) =>
        TryNormalizePhone(value, out var phone) ? phone :
            throw new ArgumentException("Use an 8-16 digit primary phone number.", nameof(value));

    private static bool TryNormalizePhone(string? value, out string phone)
    {
        phone = value?.Trim() ?? string.Empty;
        return phone.Length is >= 8 and <= 16 && phone.All(char.IsAsciiDigit);
    }

    private static string NormalizeUserName(string? value)
    {
        var normalized = value?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!UserNamePattern.IsMatch(normalized))
            throw new ArgumentException("Username must be 3-50 lowercase letters, digits, dots, hyphens or underscores.", nameof(value));
        return normalized;
    }

    private void Audit(IdentityAccountId actor, string action, Guid accountId, DateTimeOffset now,
        string? correlationId, object? metadata = null) => platformDb.AuditEntries.Add(new PlatformAuditEntry
        {
            Id = Guid.NewGuid(), AccountId = actor, Action = action,
            ResourceType = "platform-staff", ResourceId = accountId.ToString("D"),
            OccurredAtUtc = now, CorrelationId = correlationId is { Length: <= 100 } ? correlationId : null,
            MetadataJson = metadata is null ? null : JsonSerializer.Serialize(metadata)
        });
}
