using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Identity;

namespace Mdaresna.Schools.Domain.Students;

public enum StudentGender { Male, Female }
public enum StudentEnrollmentStatus { Active, Suspended, Withdrawn, Transferred, Graduated }
public enum AdmissionApplicationStatus { Submitted, UnderReview, InterviewScheduled, Waitlisted, Accepted, Rejected, Withdrawn }
public enum AdmissionApplicationSource { SchoolDesk, FamilyApp }
public enum GuardianRelationship { Father, Mother, LegalGuardian, Relative, Other }

public sealed class Student
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public Guid GlobalStudentId { get; set; }
    public string StudentCode { get; set; } = string.Empty;
    public string FullNameAr { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public StudentGender Gender { get; set; }
    public string? NationalId { get; set; }
    public string? BirthCertificateNumber { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Person Person { get; set; } = null!;
    public ICollection<StudentGuardian> Guardians { get; set; } = [];
    public ICollection<StudentEnrollment> Enrollments { get; set; } = [];
}

public sealed class Guardian
{
    public Guid Id { get; set; }
    public Guid? PlatformAccountId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? NationalId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ICollection<StudentGuardian> Students { get; set; } = [];
    public ICollection<AdmissionApplicationGuardian> Applications { get; set; } = [];
}

public sealed class StudentGuardian
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid GuardianId { get; set; }
    public GuardianRelationship Relationship { get; set; }
    public bool IsPrimary { get; set; }
    public bool CanPickup { get; set; }
    public bool IsFinancialResponsible { get; set; }
    public bool IsEmergencyContact { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Student Student { get; set; } = null!;
    public Guardian Guardian { get; set; } = null!;
}

public sealed class StudentEnrollment
{
    public Guid Id { get; set; }
    public Guid StudentId { get; set; }
    public Guid GradeOfferingId { get; set; }
    public Guid ClassSectionId { get; set; }
    public DateOnly EnrollmentDate { get; set; }
    public StudentEnrollmentStatus Status { get; set; } = StudentEnrollmentStatus.Active;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Student Student { get; set; } = null!;
    public GradeOffering GradeOffering { get; set; } = null!;
    public ClassSection ClassSection { get; set; } = null!;
}

public sealed class AdmissionApplication
{
    public Guid Id { get; set; }
    public string ApplicationNumber { get; set; } = string.Empty;
    public Guid? GlobalStudentId { get; set; }
    public string? StudentCode { get; set; }
    public string FullNameAr { get; set; } = string.Empty;
    public string FullNameEn { get; set; } = string.Empty;
    public string NormalizedName { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public StudentGender Gender { get; set; }
    public string? NationalId { get; set; }
    public string? BirthCertificateNumber { get; set; }
    public Guid ProgramAcademicYearId { get; set; }
    public Guid GradeLevelId { get; set; }
    public AdmissionApplicationSource Source { get; set; } = AdmissionApplicationSource.SchoolDesk;
    public AdmissionApplicationStatus Status { get; set; } = AdmissionApplicationStatus.Submitted;
    public DateTimeOffset SubmittedAtUtc { get; set; }
    public string? Notes { get; set; }
    public Guid? AcceptedStudentId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ProgramAcademicYear ProgramAcademicYear { get; set; } = null!;
    public GradeLevel GradeLevel { get; set; } = null!;
    public Student? AcceptedStudent { get; set; }
    public ICollection<AdmissionApplicationGuardian> Guardians { get; set; } = [];
}

public sealed class AdmissionApplicationGuardian
{
    public Guid Id { get; set; }
    public Guid AdmissionApplicationId { get; set; }
    public Guid GuardianId { get; set; }
    public GuardianRelationship Relationship { get; set; }
    public bool IsPrimary { get; set; }
    public bool CanPickup { get; set; }
    public bool IsFinancialResponsible { get; set; }
    public bool IsEmergencyContact { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public AdmissionApplication AdmissionApplication { get; set; } = null!;
    public Guardian Guardian { get; set; } = null!;
}
