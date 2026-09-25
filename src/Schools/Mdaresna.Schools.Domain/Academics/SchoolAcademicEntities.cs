using Mdaresna.Schools.Domain.Facilities;
using Mdaresna.Schools.Domain.Students;

namespace Mdaresna.Schools.Domain.Academics;

public enum EducationProgramType { General = 1, InternationalAmerican = 2, InternationalBritish = 3, InternationalOther = 4 }
public enum SchoolCalendarEventType { Holiday = 1, Closure = 2, Activity = 3, Exam = 4, Other = 5 }
public enum GradeOfferingStatus { Draft = 1, Active = 2, Closed = 3 }
public enum SchoolShift { FullDay = 1, Morning = 2, Evening = 3 }
public enum CurriculumPlanStatus { Draft = 1, Active = 2, Archived = 3 }
public enum GradeSubjectOfferingStatus { Draft = 1, Active = 2, Closed = 3 }

public sealed class EducationProgram : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public EducationProgramType ProgramType { get; set; }
    public StudentAttendanceMode? StudentAttendanceModeOverride { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<ProgramAcademicYear> AcademicYears { get; set; } = [];
    public ICollection<SchoolDaySchedule> Schedules { get; set; } = [];
    public ICollection<EducationStage> Stages { get; set; } = [];
    public ICollection<CurriculumPlan> CurriculumPlans { get; set; } = [];
}

public sealed class AcademicYearDefinition : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<ProgramAcademicYear> ProgramYears { get; set; } = [];
}

public sealed class ProgramAcademicYear : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid EducationProgramId { get; set; }
    public Guid AcademicYearDefinitionId { get; set; }
    public Guid? CurriculumPlanId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public EducationProgram EducationProgram { get; set; } = null!;
    public AcademicYearDefinition AcademicYearDefinition { get; set; } = null!;
    public CurriculumPlan? CurriculumPlan { get; set; }
    public ICollection<AcademicTerm> Terms { get; set; } = [];
    public ICollection<GradeOffering> GradeOfferings { get; set; } = [];
}

public sealed class AcademicTerm : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid ProgramAcademicYearId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ProgramAcademicYear ProgramAcademicYear { get; set; } = null!;
    public ICollection<AcademicPeriod> Periods { get; set; } = [];
}

public sealed class AcademicPeriod : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid AcademicTermId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public AcademicTerm AcademicTerm { get; set; } = null!;
}

public sealed class SchoolDaySchedule : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid EducationProgramId { get; set; }
    public Guid? BranchId { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly StartsAt { get; set; }
    public TimeOnly EndsAt { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public EducationProgram EducationProgram { get; set; } = null!;
    public SchoolBranch? Branch { get; set; }
}

public sealed class SchoolCalendarEvent : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid? EducationProgramId { get; set; }
    public Guid? ProgramAcademicYearId { get; set; }
    public Guid? BranchId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public SchoolCalendarEventType EventType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public bool IsSchoolClosed { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public EducationProgram? EducationProgram { get; set; }
    public ProgramAcademicYear? ProgramAcademicYear { get; set; }
    public SchoolBranch? Branch { get; set; }
}

public sealed class EducationStage : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid EducationProgramId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public int DailyLessonCount { get; set; }
    public int DailyBreakCount { get; set; }
    public StudentAttendanceMode? StudentAttendanceModeOverride { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public EducationProgram EducationProgram { get; set; } = null!;
    public ICollection<EducationTrack> Tracks { get; set; } = [];
    public ICollection<GradeLevel> GradeLevels { get; set; } = [];
}

public sealed class EducationTrack : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid EducationStageId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public EducationStage EducationStage { get; set; } = null!;
    public ICollection<GradeLevel> GradeLevels { get; set; } = [];
}

public sealed class GradeLevel : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid EducationStageId { get; set; }
    public Guid? EducationTrackId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public EducationStage EducationStage { get; set; } = null!;
    public EducationTrack? EducationTrack { get; set; }
    public ICollection<GradeOffering> Offerings { get; set; } = [];
    public ICollection<CurriculumGradeSubject> CurriculumSubjects { get; set; } = [];
}

public sealed class GradeOffering : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid ProgramAcademicYearId { get; set; }
    public Guid GradeLevelId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int? Capacity { get; set; }
    public GradeOfferingStatus Status { get; set; } = GradeOfferingStatus.Draft;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ProgramAcademicYear ProgramAcademicYear { get; set; } = null!;
    public GradeLevel GradeLevel { get; set; } = null!;
    public ICollection<ClassSection> ClassSections { get; set; } = [];
    public ICollection<GradeSubjectOffering> SubjectOfferings { get; set; } = [];
}

public sealed class ClassSection : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid GradeOfferingId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public SchoolShift Shift { get; set; } = SchoolShift.FullDay;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public GradeOffering GradeOffering { get; set; } = null!;
    public ICollection<ClassRoomAssignment> RoomAssignments { get; set; } = [];
    public ICollection<WeeklyTimetableSlot> TimetableSlots { get; set; } = [];
}

public sealed class ClassRoomAssignment : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid ClassSectionId { get; set; }
    public Guid RoomId { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly EffectiveTo { get; set; }
    public TimeOnly? StartsAt { get; set; }
    public TimeOnly? EndsAt { get; set; }
    public bool IsPrimary { get; set; } = true;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ClassSection ClassSection { get; set; } = null!;
    public SchoolRoom Room { get; set; } = null!;
}
