using System.Security.Claims;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Auth;

internal static class SchoolDepartmentScope
{
    public static async Task<IQueryable<LocalUserAccount>> ApplyAsync(ClaimsPrincipal principal,
        SchoolsDbContext db, IQueryable<LocalUserAccount> query, CancellationToken ct)
    {
        var currentUserId = CurrentUserId(principal);
        if (currentUserId == Guid.Empty) return query.Where(_ => false);
        if (await db.LocalUserRoles.AsNoTracking().AnyAsync(x => x.UserId == currentUserId &&
                x.RoleId == SchoolIdentitySeed.SchoolAdminRoleId && x.Role.IsActive, ct))
            return query;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var rootIds = await db.DepartmentLeaderships.AsNoTracking()
            .Where(x => x.UserId == currentUserId && x.IsActive && x.StartsOn <= today &&
                (!x.EndsOn.HasValue || x.EndsOn >= today) && x.Department.IsActive)
            .Select(x => x.DepartmentId).Distinct().ToListAsync(ct);
        if (rootIds.Count == 0) return query.Where(x => x.Id == currentUserId);

        var departments = await db.SchoolDepartments.AsNoTracking().Where(x => x.IsActive)
            .Select(x => new { x.Id, x.ParentDepartmentId }).ToListAsync(ct);
        var allowed = rootIds.ToHashSet();
        var changed = true;
        while (changed)
        {
            changed = false;
            foreach (var department in departments.Where(x => x.ParentDepartmentId.HasValue && allowed.Contains(x.ParentDepartmentId.Value)))
                changed |= allowed.Add(department.Id);
        }
        var ids = allowed.ToArray();
        return query.Where(x => x.Id == currentUserId || x.DepartmentMemberships.Any(m => ids.Contains(m.DepartmentId) &&
            m.IsActive && m.StartsOn <= today && (!m.EndsOn.HasValue || m.EndsOn >= today)));
    }

    private static Guid CurrentUserId(ClaimsPrincipal principal) =>
        Guid.TryParse(principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value, out var id)
            ? id : Guid.Empty;
}
