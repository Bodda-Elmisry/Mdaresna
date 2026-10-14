using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Auth;

public static class ClassOperationalScope
{
    public static async Task<DateOnly> Today(SchoolsDbContext db, CancellationToken ct)
    {
        var zone = await db.SchoolInformation.Select(x => x.TimeZoneId).SingleOrDefaultAsync(ct);
        return DateOnly.FromDateTime(TimeZoneInfo.ConvertTime(DateTimeOffset.UtcNow,
            Time.SchoolClock.Resolve(string.IsNullOrWhiteSpace(zone) ? "UTC" : zone)).DateTime);
    }

    public static IQueryable<ClassOperationalGrant> Current(SchoolsDbContext db, Guid userId, DateOnly today) =>
        db.ClassOperationalGrants.Where(x => x.Assignment.UserId == userId && x.Assignment.IsActive &&
            x.Assignment.StartsOn <= today && x.Assignment.EndsOn >= today && x.Assignment.User.Status == LocalUserStatus.Active &&
            x.ClassSection.IsActive && x.ClassSection.GradeOffering.IsActive && x.ClassSection.GradeOffering.Status != GradeOfferingStatus.Closed &&
            x.ClassSection.GradeOffering.ProgramAcademicYear.StartDate <= today && x.ClassSection.GradeOffering.ProgramAcademicYear.EndDate >= today &&
            (x.GradeSubjectOfferingId == null || (x.GradeSubjectOffering!.IsActive && db.ClassSectionSubjects.Any(s =>
                s.ClassSectionId == x.ClassSectionId && s.IsActive && s.GradeSubjectOfferingId == x.GradeSubjectOfferingId))) &&
            (x.Assignment.BuildingFloorId == null || (x.Assignment.BuildingFloor!.IsActive && x.Assignment.BuildingFloor.Building.IsActive && x.ClassSection.RoomAssignments.Any(r => r.IsActive && !r.IsDeleted && r.IsPrimary &&
                r.EffectiveFrom <= today && r.EffectiveTo >= today && r.Room.IsActive && r.Room.FloorId == x.Assignment.BuildingFloorId))));

    public static Task<bool> HasPermission(SchoolsDbContext db, Guid userId, string permission, CancellationToken ct) =>
        db.LocalUserRoles.AnyAsync(r => r.UserId == userId && r.User.Status == LocalUserStatus.Active && r.Role.IsActive &&
            r.Role.Permissions.Any(p => p.Permission.IsActive && p.Permission.Code == permission), ct);

    public static async Task<bool> Allows(SchoolsDbContext db, Guid userId, Guid classId, string action,
        CancellationToken ct, Guid? offeringId = null)
    {
        if (!ClassOperationalActions.Permissions.TryGetValue(action, out var permission) ||
            !await HasPermission(db, userId, permission, ct)) return false;
        var today = await Today(db, ct);
        return await Current(db, userId, today).AnyAsync(x => x.ClassSectionId == classId && x.Action == action &&
            (offeringId == null ? x.GradeSubjectOfferingId == null : x.GradeSubjectOfferingId == offeringId), ct);
    }

    public static async Task<Guid[]> VisibleClasses(SchoolsDbContext db, Guid userId, CancellationToken ct)
    {
        var permissions = await db.LocalUserRoles.Where(r => r.UserId == userId && r.User.Status == LocalUserStatus.Active && r.Role.IsActive)
            .SelectMany(r => r.Role.Permissions.Where(p => p.Permission.IsActive).Select(p => p.Permission.Code)).ToArrayAsync(ct);
        var actions = ClassOperationalActions.Permissions.Where(x => permissions.Contains(x.Value)).Select(x => x.Key).ToArray();
        var today = await Today(db, ct);
        return await Current(db, userId, today).Where(x => actions.Contains(x.Action)).Select(x => x.ClassSectionId).Distinct().ToArrayAsync(ct);
    }
}
