using Mdaresna.Schools.Domain.Facilities;
using Mdaresna.Schools.Domain.Identity;

namespace Mdaresna.Schools.Domain.Academics;

public enum ClassSubjectTeacherRole { Primary = 1, Substitute = 2, Assistant = 3 }

public sealed class TeacherGradeSubjectScope : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid TeacherUserId { get; set; }
    public Guid GradeSubjectOfferingId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public LocalUserAccount TeacherUser { get; set; } = null!;
    public GradeSubjectOffering GradeSubjectOffering { get; set; } = null!;
}

public sealed class ClassSectionTeacherScope : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid ClassSectionId { get; set; }
    public Guid TeacherGradeSubjectScopeId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ClassSection ClassSection { get; set; } = null!;
    public TeacherGradeSubjectScope TeacherGradeSubjectScope { get; set; } = null!;
}

public sealed class ClassSectionSubject : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid ClassSectionId { get; set; }
    public Guid GradeSubjectOfferingId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ClassSection ClassSection { get; set; } = null!;
    public GradeSubjectOffering GradeSubjectOffering { get; set; } = null!;
    public ICollection<ClassSubjectTeacherAssignment> TeacherAssignments { get; set; } = [];
    public ICollection<WeeklyTimetableSlot> TimetableSlots { get; set; } = [];
}

public sealed class ClassSubjectTeacherAssignment : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid ClassSectionSubjectId { get; set; }
    public Guid TeacherGradeSubjectScopeId { get; set; }
    public ClassSubjectTeacherRole Role { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ClassSectionSubject ClassSectionSubject { get; set; } = null!;
    public TeacherGradeSubjectScope TeacherGradeSubjectScope { get; set; } = null!;
}

public sealed class WeeklyTimetableSlot : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid ClassSectionId { get; set; }
    public Guid? ClassSectionSubjectId { get; set; }
    public Guid? PrimaryTeacherScopeId { get; set; }
    public Guid? RoomId { get; set; }
    public bool AllowRoomSharing { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public int SlotNumber { get; set; }
    public bool IsBreak { get; set; }
    public TimeOnly StartsAt { get; set; }
    public TimeOnly EndsAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ClassSection ClassSection { get; set; } = null!;
    public ClassSectionSubject? ClassSectionSubject { get; set; }
    public TeacherGradeSubjectScope? PrimaryTeacherScope { get; set; }
    public SchoolRoom? Room { get; set; }
    public ICollection<WeeklyTimetableSlotSubstituteTeacher> SubstituteTeachers { get; set; } = [];
}

public sealed class TemporaryClassMerge : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid RoomId { get; set; }
    public DateOnly MergeDate { get; set; }
    public TimeOnly StartsAt { get; set; }
    public TimeOnly EndsAt { get; set; }
    public int ExpectedStudentCount { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public SchoolRoom Room { get; set; } = null!;
    public ICollection<TemporaryClassMergeSection> Sections { get; set; } = [];
}

public sealed class TemporaryClassMergeSection : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid TemporaryClassMergeId { get; set; }
    public Guid ClassSectionId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public TemporaryClassMerge TemporaryClassMerge { get; set; } = null!;
    public ClassSection ClassSection { get; set; } = null!;
}

public sealed class WeeklyTimetableSlotSubstituteTeacher : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid WeeklyTimetableSlotId { get; set; }
    public Guid TeacherGradeSubjectScopeId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public WeeklyTimetableSlot WeeklyTimetableSlot { get; set; } = null!;
    public TeacherGradeSubjectScope TeacherGradeSubjectScope { get; set; } = null!;
}

public sealed class TeacherSubstitution : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid WeeklyTimetableSlotId { get; set; }
    public Guid SubstituteTeacherAssignmentId { get; set; }
    public DateOnly LessonDate { get; set; }
    public string? Reason { get; set; }
    public string SourceType { get; set; } = "Manual";
    public string? SourceReferenceId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public WeeklyTimetableSlot WeeklyTimetableSlot { get; set; } = null!;
    public ClassSubjectTeacherAssignment SubstituteTeacherAssignment { get; set; } = null!;
}
