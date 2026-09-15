using Mdaresna.Platform.Infrastructure.Messaging;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.UnitTests.Messaging;

public sealed class PlatformNotificationServiceTests
{
    [Fact]
    public async Task DeviceInboxReadAndLogoutRemovalFollowOneAccountLifecycle()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"platform-notifications-{Guid.NewGuid():N}")
            .Options;
        await using var db = new PlatformDbContext(options);
        var service = new PlatformNotificationService(db);
        var accountId = Guid.NewGuid();

        await service.RegisterDeviceAsync(accountId, new PlatformDeviceRegistration(
            "installation-1", "fcm-token-1", "android", "ar", "Pixel"));

        Assert.Single(db.UserDevices);
        await service.QueueAsync([accountId], new PlatformNotificationInput(
            "platform.permissions.changed",
            "تم تحديث الصلاحيات", "Permissions updated",
            "تم تحديث صلاحيات حسابك.", "Your account permissions changed.",
            "/roles", new Dictionary<string, string> { ["refreshPermissions"] = "true" }));
        await db.SaveChangesAsync();

        var arabicInbox = await service.ListAsync(accountId, "ar", 1, 20);
        Assert.Single(arabicInbox.Items);
        Assert.Equal("تم تحديث الصلاحيات", arabicInbox.Items[0].Title);
        Assert.Equal(1, arabicInbox.UnreadCount);
        Assert.Single(db.NotificationDeliveries);

        await service.MarkReadAsync(accountId, arabicInbox.Items[0].Id);
        Assert.Equal(0, await service.GetUnreadCountAsync(accountId));

        await service.RemoveDeviceAsync(accountId, "installation-1");
        Assert.Empty(db.UserDevices);
    }

    [Fact]
    public async Task ReusedFcmTokenMovesToTheNewestAccount()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"platform-notification-token-{Guid.NewGuid():N}")
            .Options;
        await using var db = new PlatformDbContext(options);
        var service = new PlatformNotificationService(db);
        var firstAccount = Guid.NewGuid();
        var secondAccount = Guid.NewGuid();

        await service.RegisterDeviceAsync(firstAccount,
            new PlatformDeviceRegistration("first", "shared-token", "android", "ar", null));
        await service.RegisterDeviceAsync(secondAccount,
            new PlatformDeviceRegistration("second", "shared-token", "ios", "en", null));

        var device = Assert.Single(db.UserDevices);
        Assert.Equal("second", device.InstallationId);
        Assert.Equal("ios", device.Platform);
    }
}
