using Mdaresna.Schools.Domain.Facilities;

namespace Mdaresna.Schools.Domain.Academics;

public enum EducationProgramType { General = 1, InternationalAmerican = 2, InternationalBritish = 3, InternationalOther = 4 }
public enum SchoolCalendarEventType { Holiday = 1, Closure = 2, Activity = 3, Exam = 4, Other = 5 }

public sealed class EducationProgram : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public EducationProgramType ProgramType { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<ProgramAcademicYear> AcademicYears { get; set; } = [];
    public ICollection<SchoolDaySchedule> Schedules { get; set; } = [];
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
    public ICollection<AcademicTerm> Terms { get; set; } = [];
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
