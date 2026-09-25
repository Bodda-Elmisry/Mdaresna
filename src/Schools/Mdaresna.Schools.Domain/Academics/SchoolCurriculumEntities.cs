using Mdaresna.Schools.Domain.Facilities;

namespace Mdaresna.Schools.Domain.Academics;

public sealed class Subject : ISoftDeletableSchoolEntity
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
    public ICollection<CurriculumGradeSubject> CurriculumGrades { get; set; } = [];
}

public sealed class CurriculumPlan : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid EducationProgramId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string VersionLabel { get; set; } = string.Empty;
    public CurriculumPlanStatus Status { get; set; } = CurriculumPlanStatus.Draft;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public EducationProgram EducationProgram { get; set; } = null!;
    public ICollection<ProgramAcademicYear> ProgramYears { get; set; } = [];
    public ICollection<CurriculumGradeSubject> GradeSubjects { get; set; } = [];
}

public sealed class CurriculumGradeSubject : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid CurriculumPlanId { get; set; }
    public Guid GradeLevelId { get; set; }
    public Guid SubjectId { get; set; }
    /// <summary>Zero means the subject applies to all terms in the curriculum plan.</summary>
    public int TermNumber { get; set; }
    public int WeeklyPeriods { get; set; }
    public bool IsRequired { get; set; } = true;
    public string? InstructionLanguage { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public CurriculumPlan CurriculumPlan { get; set; } = null!;
    public GradeLevel GradeLevel { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public ICollection<CurriculumSubjectBook> Books { get; set; } = [];
    public ICollection<GradeSubjectOffering> Offerings { get; set; } = [];
}

public sealed class Book : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string? Publisher { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<BookVersion> Versions { get; set; } = [];
}

public sealed class BookVersion : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid BookId { get; set; }
    public string EditionCode { get; set; } = string.Empty;
    public string VersionLabel { get; set; } = string.Empty;
    public int PublicationYear { get; set; }
    public string Language { get; set; } = string.Empty;
    public string? Isbn { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Book Book { get; set; } = null!;
    public ICollection<CurriculumSubjectBook> CurriculumSubjects { get; set; } = [];
}

public sealed class BookRole : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public bool IsSystem { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<CurriculumSubjectBook> CurriculumSubjects { get; set; } = [];
}

public sealed class CurriculumSubjectBook : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid CurriculumGradeSubjectId { get; set; }
    public Guid BookVersionId { get; set; }
    public Guid BookRoleId { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public CurriculumGradeSubject CurriculumGradeSubject { get; set; } = null!;
    public BookVersion BookVersion { get; set; } = null!;
    public BookRole BookRole { get; set; } = null!;
}

public sealed class GradeSubjectOffering : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid GradeOfferingId { get; set; }
    public Guid CurriculumGradeSubjectId { get; set; }
    public GradeSubjectOfferingStatus Status { get; set; } = GradeSubjectOfferingStatus.Draft;
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public GradeOffering GradeOffering { get; set; } = null!;
    public CurriculumGradeSubject CurriculumGradeSubject { get; set; } = null!;
}
