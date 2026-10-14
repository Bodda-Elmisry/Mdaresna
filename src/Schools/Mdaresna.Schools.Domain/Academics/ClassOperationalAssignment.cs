using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.Facilities;

namespace Mdaresna.Schools.Domain.Academics;

public sealed class ClassOperationalAssignment
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? BuildingFloorId { get; set; }
    public string Responsibility { get; set; } = string.Empty;
    public DateOnly StartsOn { get; set; }
    public DateOnly EndsOn { get; set; }
    public bool IsActive { get; set; } = true;
    public int Revision { get; set; } = 1;
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public LocalUserAccount User { get; set; } = null!;
    public BuildingFloor? BuildingFloor { get; set; }
    public ICollection<ClassOperationalGrant> Grants { get; set; } = [];
}

public sealed class ClassOperationalGrant
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid ClassSectionId { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid? GradeSubjectOfferingId { get; set; }
    public ClassOperationalAssignment Assignment { get; set; } = null!;
    public ClassSection ClassSection { get; set; } = null!;
    public GradeSubjectOffering? GradeSubjectOffering { get; set; }
}

public sealed class ClassOperationalAssignmentAudit
{
    public Guid Id { get; set; }
    public Guid AssignmentId { get; set; }
    public Guid ActorUserId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string SnapshotJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public ClassOperationalAssignment Assignment { get; set; } = null!;
}

public static class ClassOperationalActions
{
    public static readonly IReadOnlyDictionary<string, string> Permissions = new Dictionary<string, string>
    {
        ["class.view"] = "school.attendance.view",
        ["attendance.daily"] = "school.attendance.record",
        ["attendance.session"] = "school.attendance.record",
        ["attendance.finalize"] = "school.attendance.record",
        ["attendance.reopen"] = "school.attendance.reopen",
        ["exams.view"] = "school.exams.view",
        ["exams.results.enter"] = "school.exams.results.enter",
        ["exams.results.submit"] = "school.exams.results.enter"
    };
    public static bool IsCurrent(bool active, DateOnly starts, DateOnly ends, DateOnly date) => active && starts <= date && ends >= date;
}
