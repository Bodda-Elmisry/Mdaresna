using Mdaresna.Platform.Application.Access.Staff;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.IdentityAuth.Staff;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.UnitTests.Auth;

public sealed class PlatformStaffServicesTests
{
    [Fact]
    public void Invitation_and_first_login_code_are_separate_messages()
    {
        const string userName = "school.operator";
        const string code = "12345678";

        var invitation = PlatformStaffSmsMessages.Invitation(userName);
        var activation = PlatformStaffSmsMessages.ActivationCode(userName, code);

        Assert.Contains(userName, invitation);
        Assert.Contains("موظف في إدارة منصة مدارسنا", invitation);
        Assert.DoesNotContain(code, invitation);
        Assert.DoesNotContain("رمز التفعيل:", invitation);
        Assert.Equal("staff-invitation", PlatformStaffSmsMessages.InvitationType);
        Assert.Contains(code, activation);
        Assert.Contains("10 دقائق", activation);
        Assert.Equal("otp", PlatformStaffSmsMessages.ActivationType);
    }

    [Fact]
    public async Task Staff_directory_rejects_invalid_page_before_querying()
    {
        var directory = new PlatformStaffDirectory(PlatformDb(), IdentityDb());

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            directory.ListAsync(0, 20));
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            directory.ListAsync(1, 101));
    }

    [Fact]
    public async Task Staff_role_management_denies_actor_without_permission()
    {
        var manager = new PlatformStaffRoleManager(
            PlatformDb(), IdentityDb(), new DenyAllPermissions());

        await Assert.ThrowsAsync<PlatformStaffAccessDeniedException>(() =>
            manager.AssignAsync(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
        await Assert.ThrowsAsync<PlatformStaffAccessDeniedException>(() =>
            manager.RevokeAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task Staff_role_management_prohibits_self_assignment()
    {
        var manager = new PlatformStaffRoleManager(
            PlatformDb(), IdentityDb(), new DenyAllPermissions());
        var actor = Guid.NewGuid();

        var error = await Assert.ThrowsAsync<PlatformConflictException>(() =>
            manager.AssignAsync(actor, actor, Guid.NewGuid()));

        Assert.Equal("staff.self_assignment_forbidden", error.Code);
    }

    [Fact]
    public void Staff_directory_queries_translate_to_sql_server()
    {
        using var platformDb = new PlatformDbContext(
            new DbContextOptionsBuilder<PlatformDbContext>()
                .UseSqlServer(
                    "Server=(localdb)\\MSSQLLocalDB;Database=TranslationOnly;Trusted_Connection=True")
                .Options);
        var pageIds = new[] { IdentityAccountId.New() };

        var staffPageSql = platformDb.RoleAssignments
            .Select(x => x.AccountId)
            .Distinct()
            .OrderBy(x => x)
            .Skip(0)
            .Take(20)
            .ToQueryString();
        var roleRowsSql = (
            from assignment in platformDb.RoleAssignments
            join role in platformDb.Roles on assignment.RoleId equals role.Id
            where pageIds.Contains(assignment.AccountId) &&
                  assignment.RevokedAtUtc == null &&
                  role.IsActive
            select new { assignment.AccountId, role.Key }).ToQueryString();

        Assert.Contains("ORDER BY", staffPageSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("JOIN", roleRowsSql, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Role_catalog_queries_translate_to_sql_server()
    {
        using var platformDb = new PlatformDbContext(
            new DbContextOptionsBuilder<PlatformDbContext>()
                .UseSqlServer(
                    "Server=(localdb)\\MSSQLLocalDB;Database=TranslationOnly;Trusted_Connection=True")
                .Options);
        var roleIds = new[] { PlatformRoleId.New() };

        var rolesSql = platformDb.Roles
            .AsNoTracking()
            .Where(role => role.IsActive)
            .OrderBy(role => role.Key)
            .Select(role => new { role.Id, role.Key })
            .ToQueryString();
        var permissionsSql = platformDb.RolePermissions
            .AsNoTracking()
            .Where(permission => roleIds.Contains(permission.RoleId))
            .Select(permission => new
            {
                permission.RoleId,
                permission.PermissionCode
            })
            .ToQueryString();

        Assert.Contains("ORDER BY", rolesSql, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("role_permissions", permissionsSql, StringComparison.OrdinalIgnoreCase);
    }

    private static PlatformDbContext PlatformDb() => new(
        new DbContextOptionsBuilder<PlatformDbContext>().Options);

    private static IdentityDbContext IdentityDb() => new(
        new DbContextOptionsBuilder<IdentityDbContext>().Options);

    private sealed class DenyAllPermissions : IPlatformPermissionEvaluator
    {
        public Task<bool> HasPermissionAsync(
            IdentityAccountId accountId,
            PermissionCode permission,
            CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
}
