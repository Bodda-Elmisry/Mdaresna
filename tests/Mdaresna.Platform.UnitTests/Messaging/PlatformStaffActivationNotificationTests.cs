using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Mdaresna.Platform.Application.Abstractions.Messaging;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.IdentityAuth;
using Mdaresna.Platform.Infrastructure.IdentityAuth.Staff;
using Mdaresna.Platform.Infrastructure.Messaging;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Mdaresna.Platform.UnitTests.Messaging;

public sealed class PlatformStaffActivationNotificationTests
{
    [Fact]
    public async Task FirstActivationNotifiesActiveAppManagersWithoutRequestingAnAccessRefresh()
    {
        var platformOptions = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"platform-staff-activation-notification-{Guid.NewGuid():N}",
                database => database.EnableNullChecks(false))
            .Options;
        var identityOptions = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase($"identity-staff-activation-notification-{Guid.NewGuid():N}",
                database => database.EnableNullChecks(false))
            .Options;
        await using var platformDb = new PlatformDbContext(platformOptions);
        await using var identityDb = new IdentityDbContext(identityOptions);
        var managerId = Guid.NewGuid();
        var disabledManagerId = Guid.NewGuid();
        var employeeId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        const string phone = "00201000000000";
        const string code = "12345678";
        var hashKey = Enumerable.Range(1, 32).Select(value => (byte)value).ToArray();

        var account = new Account
        {
            Id = employeeId,
            Status = AccountStatus.Active,
            DisplayName = "New employee",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        account.LoginIdentifiers.Add(new LoginIdentifier
        {
            Id = Guid.NewGuid(),
            AccountId = employeeId,
            Type = LoginIdentifierType.Phone,
            NormalizedValue = phone,
            DisplayValue = phone,
            SchoolId = null,
            IsVerified = true,
            IsPrimary = true,
            VerifiedAtUtc = now,
            CreatedAtUtc = now
        });
        identityDb.Accounts.Add(account);
        await identityDb.SaveChangesAsync();

        var actor = IdentityAccountId.From(managerId);
        var managerRole = PlatformRole.Create(PlatformRoleId.New(), "app-manager", "App Manager",
            isSystem: true, [PlatformPermissionCodes.AccessManage], actor, now);
        var employeeRole = PlatformRole.Create(PlatformRoleId.New(), "employee", "Employee",
            isSystem: false, [PlatformPermissionCodes.SchoolsRead], actor, now);
        var localEmployeeId = Guid.NewGuid();
        platformDb.Roles.AddRange(managerRole, employeeRole);
        platformDb.LocalUsers.AddRange(
            new PlatformLocalUser
            {
                Id = Guid.NewGuid(), PersonId = managerId, UserName = "manager",
                NormalizedUserName = "MANAGER", DisplayName = "Manager", Status = "Active",
                CreatedAtUtc = now, UpdatedAtUtc = now
            },
            new PlatformLocalUser
            {
                Id = Guid.NewGuid(), PersonId = disabledManagerId, UserName = "disabled-manager",
                NormalizedUserName = "DISABLED-MANAGER", DisplayName = "Disabled manager", Status = "Disabled",
                CreatedAtUtc = now, UpdatedAtUtc = now
            },
            new PlatformLocalUser
            {
                Id = localEmployeeId, PersonId = employeeId, UserName = "new-employee",
                NormalizedUserName = "NEW-EMPLOYEE", DisplayName = "New employee", Status = "PendingActivation",
                CreatedAtUtc = now, UpdatedAtUtc = now,
                StaffInvitationChallenge = new PlatformStaffInvitationChallenge
                {
                    UserId = localEmployeeId,
                    CodeHash = HMACSHA256.HashData(hashKey, Encoding.UTF8.GetBytes(
                        $"mdaresna-platform-staff-invitation:{localEmployeeId:N}:{code}")),
                    CreatedAtUtc = now,
                    ExpiresAtUtc = now.AddMinutes(10),
                    LastSentAtUtc = now,
                    SendWindowStartUtc = now,
                    SendCount = 1
                }
            });
        platformDb.RoleAssignments.AddRange(
            PlatformRoleAssignment.Assign(PlatformRoleAssignmentId.New(),
                IdentityAccountId.From(managerId), managerRole.Id, actor, now),
            PlatformRoleAssignment.Assign(PlatformRoleAssignmentId.New(),
                IdentityAccountId.From(disabledManagerId), managerRole.Id, actor, now),
            PlatformRoleAssignment.Assign(PlatformRoleAssignmentId.New(),
                IdentityAccountId.From(employeeId), employeeRole.Id, actor, now));
        await platformDb.SaveChangesAsync();
        managerRole.DequeueDomainEvents();
        employeeRole.DequeueDomainEvents();

        var notifications = new PlatformNotificationService(platformDb);
        await notifications.RegisterDeviceAsync(managerId, new PlatformDeviceRegistration(
            "manager-web", "manager-web-token", "web", "ar", null));
        await notifications.RegisterDeviceAsync(managerId, new PlatformDeviceRegistration(
            "manager-phone", "manager-phone-token", "android", "en", null));
        await notifications.RegisterDeviceAsync(disabledManagerId, new PlatformDeviceRegistration(
            "disabled-web", "disabled-web-token", "web", "ar", null));
        var passwordHasher = new PasswordHasher<Account>();
        var management = new PlatformStaffManagement(
            platformDb,
            identityDb,
            new AllowAllPermissions(),
            new NoOpSmsSender(),
            new PlatformPasswordCredentialFactory(passwordHasher),
            passwordHasher,
            new PlatformActivationOptions(hashKey),
            NullLogger<PlatformStaffManagement>.Instance,
            notifications);

        var completed = await management.CompleteActivationAsync(
            phone, code, "Strong-password-123");

        Assert.True(completed);
        var notification = Assert.Single(platformDb.Notifications);
        Assert.Equal("platform.staff.activated", notification.Type);
        Assert.Equal("/staff", notification.ActionUrl);
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(notification.DataJson)!;
        Assert.Equal(employeeId.ToString("D"), data["staffAccountId"]);
        Assert.DoesNotContain("refreshPermissions", data.Keys);
        Assert.Equal(managerId, Assert.Single(platformDb.NotificationRecipients).AccountId.Value);
        Assert.Equal(2, await platformDb.NotificationDeliveries.CountAsync());
        Assert.DoesNotContain(platformDb.NotificationRecipients,
            recipient => recipient.AccountId.Value == disabledManagerId);
    }

    private sealed class AllowAllPermissions : IPlatformPermissionEvaluator
    {
        public Task<bool> HasPermissionAsync(
            IdentityAccountId accountId,
            PermissionCode permission,
            CancellationToken cancellationToken = default) => Task.FromResult(true);
    }

    private sealed class NoOpSmsSender : IPlatformSmsSender
    {
        public Task SendAsync(
            string phoneNumber,
            string message,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }
}
