using Mdaresna.Platform.Application.Access.Staff;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.IdentityAuth.Staff;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
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
    public async Task Staff_summary_keeps_unique_staff_total_and_splits_each_role_by_status()
    {
        await using var platformDb = PlatformMemoryDb();
        await using var identityDb = IdentityMemoryDb();
        var actor = IdentityAccountId.New();
        var activePersonId = Guid.NewGuid();
        var disabledPersonId = Guid.NewGuid();
        var now = DateTimeOffset.UtcNow;
        var managerRole = PlatformRole.Create(PlatformRoleId.New(), "manager", "Manager", false,
            [], actor, now);
        var reviewerRole = PlatformRole.Create(PlatformRoleId.New(), "reviewer", "Reviewer", false,
            [], actor, now);
        platformDb.Roles.AddRange(managerRole, reviewerRole);
        platformDb.LocalUsers.AddRange(
            new PlatformLocalUser { Id = Guid.NewGuid(), PersonId = activePersonId, UserName = "active",
                NormalizedUserName = "ACTIVE", Status = "Active", CreatedAtUtc = now, UpdatedAtUtc = now },
            new PlatformLocalUser { Id = Guid.NewGuid(), PersonId = disabledPersonId, UserName = "disabled",
                NormalizedUserName = "DISABLED", Status = "Disabled", CreatedAtUtc = now, UpdatedAtUtc = now });
        platformDb.RoleAssignments.AddRange(
            PlatformRoleAssignment.Assign(PlatformRoleAssignmentId.New(), IdentityAccountId.From(activePersonId),
                managerRole.Id, actor, now),
            PlatformRoleAssignment.Assign(PlatformRoleAssignmentId.New(), IdentityAccountId.From(activePersonId),
                reviewerRole.Id, actor, now),
            PlatformRoleAssignment.Assign(PlatformRoleAssignmentId.New(), IdentityAccountId.From(disabledPersonId),
                managerRole.Id, actor, now));
        identityDb.Accounts.AddRange(
            new Account { Id = activePersonId, Status = AccountStatus.Active, CreatedAtUtc = now, UpdatedAtUtc = now },
            new Account { Id = disabledPersonId, Status = AccountStatus.Active, CreatedAtUtc = now, UpdatedAtUtc = now });
        await platformDb.SaveChangesAsync();
        await identityDb.SaveChangesAsync();

        var summary = await new PlatformStaffDirectory(platformDb, identityDb).SummaryAsync();

        Assert.Equal(2, summary.TotalStaff);
        Assert.Equal(1, summary.ActiveStaff);
        Assert.Equal(1, summary.InactiveStaff);
        Assert.Equal(3, summary.TotalRoleAssignments);
        var manager = Assert.Single(summary.Roles, role => role.Key == "manager");
        Assert.Equal(1, manager.ActiveCount);
        Assert.Equal(1, manager.InactiveCount);
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

    private static PlatformDbContext PlatformMemoryDb() => new(
        new DbContextOptionsBuilder<PlatformDbContext>()
            .UseInMemoryDatabase($"platform-staff-summary-{Guid.NewGuid():N}",
                database => database.EnableNullChecks(false)).Options);

    private static IdentityDbContext IdentityMemoryDb() => new(
        new DbContextOptionsBuilder<IdentityDbContext>()
            .UseInMemoryDatabase($"identity-staff-summary-{Guid.NewGuid():N}",
                database => database.EnableNullChecks(false)).Options);

    private sealed class DenyAllPermissions : IPlatformPermissionEvaluator
    {
        public Task<bool> HasPermissionAsync(
            IdentityAccountId accountId,
            PermissionCode permission,
            CancellationToken cancellationToken = default) => Task.FromResult(false);
    }
}
