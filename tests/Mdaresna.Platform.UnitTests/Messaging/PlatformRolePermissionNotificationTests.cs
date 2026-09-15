using System.Text.Json;
using Mdaresna.Platform.Application.Access.Staff;
using Mdaresna.Platform.Application.Abstractions.Persistence;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.IdentityAuth.Staff;
using Mdaresna.Platform.Infrastructure.Messaging;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.UnitTests.Messaging;

public sealed class PlatformRolePermissionNotificationTests
{
    [Fact]
    public async Task PermissionChangeNotifiesEveryActiveRoleMemberOnEveryRegisteredDevice()
    {
        var options = new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"platform-role-notifications-{Guid.NewGuid():N}",
                database => database.EnableNullChecks(false))
            .Options;
        await using var db = new PlatformDbContext(options);
        var actor = IdentityAccountId.New();
        var firstAccount = IdentityAccountId.New();
        var secondAccount = IdentityAccountId.New();
        var revokedAccount = IdentityAccountId.New();
        var now = DateTimeOffset.UtcNow;
        var role = PlatformRole.Create(PlatformRoleId.New(), "school-reviewer", "School reviewer",
            false, [PlatformPermissionCodes.AccessManage, PlatformPermissionCodes.SchoolsRead], actor, now);
        db.Roles.Add(role);
        db.Permissions.AddRange(
            new PlatformPermissionRecord { Code = PlatformPermissionCodes.AccessManage },
            new PlatformPermissionRecord { Code = PlatformPermissionCodes.SchoolsRead },
            new PlatformPermissionRecord { Code = PlatformPermissionCodes.SchoolsManage });
        db.RoleAssignments.AddRange(
            PlatformRoleAssignment.Assign(PlatformRoleAssignmentId.New(), firstAccount, role.Id, actor, now),
            PlatformRoleAssignment.Assign(PlatformRoleAssignmentId.New(), secondAccount, role.Id, actor, now));
        var revoked = PlatformRoleAssignment.Assign(
            PlatformRoleAssignmentId.New(), revokedAccount, role.Id, actor, now);
        revoked.Revoke(actor, now.AddSeconds(1));
        db.RoleAssignments.Add(revoked);
        await db.SaveChangesAsync();
        role.DequeueDomainEvents();

        var notifications = new PlatformNotificationService(db);
        await notifications.RegisterDeviceAsync(firstAccount.Value,
            new PlatformDeviceRegistration("first-web", "first-web-token", "web", "ar", null));
        await notifications.RegisterDeviceAsync(firstAccount.Value,
            new PlatformDeviceRegistration("first-phone", "first-phone-token", "android", "ar", null));
        await notifications.RegisterDeviceAsync(secondAccount.Value,
            new PlatformDeviceRegistration("second-web", "second-web-token", "web", "en", null));
        await notifications.RegisterDeviceAsync(revokedAccount.Value,
            new PlatformDeviceRegistration("revoked-web", "revoked-web-token", "web", "ar", null));

        var manager = new PlatformRoleManager(
            db,
            new RoleRepository(role),
            new DbUnitOfWork(db),
            new AllowAllPermissions(),
            notifications);

        await manager.UpdateAsync(actor.Value, role.Id.Value, new PlatformRoleSaveRequest(
            role.Key, role.DisplayName,
            [PlatformPermissionCodes.AccessManage.Value, PlatformPermissionCodes.SchoolsManage.Value]));

        var notification = Assert.Single(db.Notifications);
        Assert.Equal("platform.permissions.changed", notification.Type);
        Assert.Equal("تم تغيير صلاحيتك", notification.TitleAr);
        Assert.Equal("Your permissions changed", notification.TitleEn);
        Assert.Equal("true", JsonSerializer.Deserialize<Dictionary<string, string>>(
            notification.DataJson)!["refreshPermissions"]);
        Assert.Equal(2, await db.NotificationRecipients.CountAsync());
        Assert.Equal(3, await db.NotificationDeliveries.CountAsync());
        Assert.DoesNotContain(db.NotificationRecipients,
            item => item.AccountId == revokedAccount);
    }

    private sealed class RoleRepository(PlatformRole role) : IPlatformRoleRepository
    {
        public Task<PlatformRole?> FindByIdAsync(PlatformRoleId roleId,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PlatformRole?>(role.Id == roleId ? role : null);

        public Task<PlatformRole?> FindByKeyAsync(string roleKey,
            CancellationToken cancellationToken = default) =>
            Task.FromResult<PlatformRole?>(role.Key == roleKey ? role : null);

        public Task AddAsync(PlatformRole value,
            CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    private sealed class DbUnitOfWork(PlatformDbContext db) : IPlatformUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            db.SaveChangesAsync(cancellationToken);
    }

    private sealed class AllowAllPermissions : IPlatformPermissionEvaluator
    {
        public Task<bool> HasPermissionAsync(IdentityAccountId accountId, PermissionCode permission,
            CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}
