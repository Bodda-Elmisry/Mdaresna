using System.Text.Json;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.IdentityAuth.Staff;
using Mdaresna.Platform.Infrastructure.Messaging;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.UnitTests.Messaging;

public sealed class PlatformStaffRoleAssignmentNotificationTests
{
    [Fact]
    public async Task AssigningRoleNotifiesTheStaffMemberOnEveryRegisteredDevice()
    {
        var platformOptions = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"platform-staff-role-notification-{Guid.NewGuid():N}",
                database => database.EnableNullChecks(false))
            .Options;
        var identityOptions = new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase($"identity-staff-role-notification-{Guid.NewGuid():N}",
                database => database.EnableNullChecks(false))
            .Options;
        await using var platformDb = new PlatformDbContext(platformOptions);
        await using var identityDb = new IdentityDbContext(identityOptions);
        var actor = Guid.NewGuid();
        var target = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;

        identityDb.Accounts.Add(new Account
        {
            Id = target,
            Status = AccountStatus.Active,
            DisplayName = "Notification target",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });
        identityDb.LoginIdentifiers.Add(new LoginIdentifier
        {
            Id = Guid.NewGuid(),
            AccountId = target,
            Type = LoginIdentifierType.Phone,
            NormalizedValue = "+201000000000",
            DisplayValue = "+201000000000",
            SchoolId = null,
            IsVerified = true,
            VerifiedAtUtc = now,
            CreatedAtUtc = now
        });
        await identityDb.SaveChangesAsync();

        var role = PlatformRole.Create(
            PlatformRoleId.New(),
            "notification-target",
            "Notification target",
            isSystem: false,
            [PlatformPermissionCodes.SchoolsRead],
            IdentityAccountId.From(actor),
            now);
        platformDb.Roles.Add(role);
        await platformDb.SaveChangesAsync();
        role.DequeueDomainEvents();

        var notifications = new PlatformNotificationService(platformDb);
        await notifications.RegisterDeviceAsync(target, new PlatformDeviceRegistration(
            "target-web", "target-web-token", "web", "ar", null));
        await notifications.RegisterDeviceAsync(target, new PlatformDeviceRegistration(
            "target-phone", "target-phone-token", "android", "en", null));
        var manager = new PlatformStaffRoleManager(
            platformDb, identityDb, new AllowAllPermissions(), notifications);

        var result = await manager.AssignAsync(actor, target, role.Id.Value);

        Assert.True(result.Changed);
        var notification = Assert.Single(platformDb.Notifications);
        Assert.Equal("platform.permissions.changed", notification.Type);
        Assert.Equal("تم تغيير صلاحيتك", notification.TitleAr);
        Assert.Equal("Your permissions changed", notification.TitleEn);
        Assert.Equal("true", JsonSerializer.Deserialize<Dictionary<string, string>>(
            notification.DataJson)!["refreshPermissions"]);
        Assert.Equal(target, Assert.Single(platformDb.NotificationRecipients).AccountId.Value);
        Assert.Equal(2, await platformDb.NotificationDeliveries.CountAsync());

        var duplicate = await manager.AssignAsync(actor, target, role.Id.Value);
        Assert.False(duplicate.Changed);
        Assert.Single(platformDb.Notifications);
        Assert.Equal(2, await platformDb.NotificationDeliveries.CountAsync());
    }

    private sealed class AllowAllPermissions : IPlatformPermissionEvaluator
    {
        public Task<bool> HasPermissionAsync(
            IdentityAccountId accountId,
            PermissionCode permission,
            CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}
