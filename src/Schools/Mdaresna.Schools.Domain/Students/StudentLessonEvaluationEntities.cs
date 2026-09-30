using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Identity;

namespace Mdaresna.Schools.Domain.Students;

public enum StudentLessonEvaluationRegisterStatus { Draft, Finalized }
public enum StudentLessonEvaluationEntryStatus { Rated, NotRated, Absent }

public static class StudentLessonEvaluationRules
{
    public static bool IsValidRating(int? value) => value is >= 1 and <= 5;

    public static bool IsValidEntry(StudentLessonEvaluationEntryStatus status, int? focusRating, int? behaviorRating) =>
        status == StudentLessonEvaluationEntryStatus.Rated
            ? IsValidRating(focusRating) && IsValidRating(behaviorRating)
            : focusRating is null && behaviorRating is null;
}

public sealed class StudentLessonEvaluationRegister
{
    public Guid Id { get; set; }
    public Guid ClassSectionId { get; set; }
    public Guid WeeklyTimetableSlotId { get; set; }
    public DateOnly LessonDate { get; set; }
    public string TimeZoneIdSnapshot { get; set; } = string.Empty;
    public StudentLessonEvaluationRegisterStatus Status { get; set; } = StudentLessonEvaluationRegisterStatus.Finalized;
    public Guid EvaluatedByUserId { get; set; }
    public DateTimeOffset EvaluatedAtUtc { get; set; }
    public Guid? FinalizedByUserId { get; set; }
    public DateTimeOffset? FinalizedAtUtc { get; set; }
    public int Revision { get; set; } = 1;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ClassSection ClassSection { get; set; } = null!;
    public WeeklyTimetableSlot WeeklyTimetableSlot { get; set; } = null!;
    public LocalUserAccount EvaluatedByUser { get; set; } = null!;
    public LocalUserAccount? FinalizedByUser { get; set; }
    public ICollection<StudentLessonEvaluationEntry> Entries { get; set; } = [];
    public ICollection<StudentLessonEvaluationAudit> AuditTrail { get; set; } = [];
}

public sealed class StudentLessonEvaluationEntry
{
    public Guid Id { get; set; }
    public Guid RegisterId { get; set; }
    public Guid StudentEnrollmentId { get; set; }
    public StudentLessonEvaluationEntryStatus Status { get; set; }
    public int? FocusRating { get; set; }
    public int? BehaviorRating { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public StudentLessonEvaluationRegister Register { get; set; } = null!;
    public StudentEnrollment StudentEnrollment { get; set; } = null!;
}

public sealed class StudentLessonEvaluationAudit
{
    public Guid Id { get; set; }
    public Guid RegisterId { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid ActorUserId { get; set; }
    public string SnapshotJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public StudentLessonEvaluationRegister Register { get; set; } = null!;
    public LocalUserAccount ActorUser { get; set; } = null!;
}
