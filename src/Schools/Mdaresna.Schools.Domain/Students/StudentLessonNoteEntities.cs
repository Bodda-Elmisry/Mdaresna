using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Identity;

namespace Mdaresna.Schools.Domain.Students;

public enum StudentLessonNoteCategory { Academic, Behavior, Participation, Commitment, Positive, Other }
public enum StudentLessonNoteVisibility { Internal, FamilyEligible }

public static class StudentLessonNoteRules
{
    public static bool IsValidRating(int? value) => value is null or >= 1 and <= 5;
}

public sealed class StudentLessonNote
{
    public Guid Id { get; set; }
    public Guid ClassSectionId { get; set; }
    public Guid StudentEnrollmentId { get; set; }
    public Guid WeeklyTimetableSlotId { get; set; }
    public DateOnly LessonDate { get; set; }
    public StudentLessonNoteCategory Category { get; set; }
    public string NoteText { get; set; } = string.Empty;
    public int? RatingLevel { get; set; }
    public bool RequiresFollowUp { get; set; }
    public StudentLessonNoteVisibility Visibility { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid UpdatedByUserId { get; set; }
    public bool IsDeleted { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public ClassSection ClassSection { get; set; } = null!;
    public StudentEnrollment StudentEnrollment { get; set; } = null!;
    public WeeklyTimetableSlot WeeklyTimetableSlot { get; set; } = null!;
    public LocalUserAccount CreatedByUser { get; set; } = null!;
    public LocalUserAccount UpdatedByUser { get; set; } = null!;
    public LocalUserAccount? DeletedByUser { get; set; }
    public ICollection<StudentLessonNoteAudit> AuditTrail { get; set; } = [];
}

public sealed class StudentLessonNoteAudit
{
    public Guid Id { get; set; }
    public Guid StudentLessonNoteId { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid ActorUserId { get; set; }
    public string SnapshotJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public StudentLessonNote StudentLessonNote { get; set; } = null!;
    public LocalUserAccount ActorUser { get; set; } = null!;
}
