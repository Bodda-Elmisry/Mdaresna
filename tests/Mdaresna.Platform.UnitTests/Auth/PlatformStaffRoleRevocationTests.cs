using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.IdentityAuth.Staff;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.UnitTests.Auth;

public sealed class PlatformStaffRoleRevocationTests
{
    [Fact]
    public async Task Access_manager_role_can_be_revoked_when_another_active_manager_remains()
    {
        await using var platformDb = PlatformDb();
        await using var identityDb = IdentityDb();
        var actor = Guid.NewGuid();
        var target = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var role = AddAccessManagerRole(platformDb, actor, now);
        AddActiveLocalUser(platformDb, actor, "actor", now);
        AddActiveLocalUser(platformDb, target, "target", now);
        platformDb.RoleAssignments.Add(PlatformRoleAssignment.Assign(
            PlatformRoleAssignmentId.New(), IdentityAccountId.From(actor), role.Id,
            IdentityAccountId.From(actor), now));
        var targetAssignment = PlatformRoleAssignment.Assign(
            PlatformRoleAssignmentId.New(), IdentityAccountId.From(target), role.Id,
            IdentityAccountId.From(actor), now);
        platformDb.RoleAssignments.Add(targetAssignment);
        await platformDb.SaveChangesAsync();

        var manager = new PlatformStaffRoleManager(
            platformDb, identityDb, new AllowAllPermissions());

        var result = await manager.RevokeAsync(actor, targetAssignment.Id.Value);

        Assert.True(result.Changed);
        Assert.False(targetAssignment.IsActive);
        Assert.True(await platformDb.RoleAssignments.AnyAsync(assignment =>
            assignment.AccountId == IdentityAccountId.From(actor) &&
            assignment.RevokedAtUtc == null));
    }

    [Fact]
    public async Task Last_active_access_manager_role_cannot_be_revoked()
    {
        await using var platformDb = PlatformDb();
        await using var identityDb = IdentityDb();
        var actor = Guid.NewGuid();
        var target = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var role = AddAccessManagerRole(platformDb, actor, now);
        AddActiveLocalUser(platformDb, target, "target", now);
        var targetAssignment = PlatformRoleAssignment.Assign(
            PlatformRoleAssignmentId.New(), IdentityAccountId.From(target), role.Id,
            IdentityAccountId.From(actor), now);
        platformDb.RoleAssignments.Add(targetAssignment);
        await platformDb.SaveChangesAsync();

        var manager = new PlatformStaffRoleManager(
            platformDb, identityDb, new AllowAllPermissions());

        var error = await Assert.ThrowsAsync<PlatformConflictException>(() =>
            manager.RevokeAsync(actor, targetAssignment.Id.Value));

        Assert.Equal("staff.last_access_manager", error.Code);
        Assert.True(targetAssignment.IsActive);
    }

    private static PlatformRole AddAccessManagerRole(
        PlatformDbContext db,
        Guid actor,
        DateTimeOffset now)
    {
        var role = PlatformRole.Create(
            PlatformRoleId.New(), "app-manager", "App Manager", true,
            [PlatformPermissionCodes.AccessManage], IdentityAccountId.From(actor), now);
        db.Permissions.Add(new PlatformPermissionRecord
        {
            Code = PlatformPermissionCodes.AccessManage
        });
        db.Roles.Add(role);
        db.RolePermissions.Add(new PlatformRolePermissionRecord
        {
            RoleId = role.Id,
            PermissionCode = PlatformPermissionCodes.AccessManage
        });
        return role;
    }

    private static void AddActiveLocalUser(
        PlatformDbContext db,
        Guid personId,
        string userName,
        DateTimeOffset now) => db.LocalUsers.Add(new PlatformLocalUser
        {
            Id = Guid.NewGuid(),
            PersonId = personId,
            UserName = userName,
            NormalizedUserName = userName.ToUpperInvariant(),
            DisplayName = userName,
            Status = "Active",
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        });

    private static PlatformDbContext PlatformDb() => new(
        new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"platform-staff-role-revocation-{Guid.NewGuid():N}",
                database => database.EnableNullChecks(false))
            .Options);

    private static IdentityDbContext IdentityDb() => new(
        new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase($"identity-staff-role-revocation-{Guid.NewGuid():N}",
                database => database.EnableNullChecks(false))
            .Options);

    private sealed class AllowAllPermissions : IPlatformPermissionEvaluator
    {
        public Task<bool> HasPermissionAsync(
            IdentityAccountId accountId,
            PermissionCode permission,
            CancellationToken cancellationToken = default) => Task.FromResult(true);
    }
}
