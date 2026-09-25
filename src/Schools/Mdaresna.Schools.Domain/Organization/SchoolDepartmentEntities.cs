using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Facilities;
using Mdaresna.Schools.Domain.Identity;

namespace Mdaresna.Schools.Domain.Organization;

public enum SchoolDepartmentType { Academic = 1, Administrative = 2 }
public enum DepartmentLeadershipRole { Head = 1, Deputy = 2, ActingHead = 3 }

public sealed class SchoolDepartment : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid? ParentDepartmentId { get; set; }
    public Guid? BranchId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public SchoolDepartmentType Type { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public SchoolDepartment? ParentDepartment { get; set; }
    public SchoolBranch? Branch { get; set; }
    public ICollection<SchoolDepartment> Children { get; set; } = [];
    public ICollection<DepartmentMembership> Memberships { get; set; } = [];
    public ICollection<DepartmentLeadership> Leaderships { get; set; } = [];
    public ICollection<AcademicDepartmentSubject> Subjects { get; set; } = [];
}

public sealed class DepartmentMembership : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid UserId { get; set; }
    public string? TitleAr { get; set; }
    public string? TitleEn { get; set; }
    public bool IsPrimary { get; set; }
    public DateOnly StartsOn { get; set; }
    public DateOnly? EndsOn { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public SchoolDepartment Department { get; set; } = null!;
    public LocalUserAccount User { get; set; } = null!;
}

public sealed class DepartmentLeadership : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid UserId { get; set; }
    public DepartmentLeadershipRole Role { get; set; } = DepartmentLeadershipRole.Head;
    public DateOnly StartsOn { get; set; }
    public DateOnly? EndsOn { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public SchoolDepartment Department { get; set; } = null!;
    public LocalUserAccount User { get; set; } = null!;
}

public sealed class AcademicDepartmentSubject : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid DepartmentId { get; set; }
    public Guid SubjectId { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public SchoolDepartment Department { get; set; } = null!;
    public Subject Subject { get; set; } = null!;
    public ICollection<SubjectCoordinatorAssignment> Coordinators { get; set; } = [];
}

public sealed class SubjectCoordinatorAssignment : ISoftDeletableSchoolEntity
{
    public Guid Id { get; set; }
    public Guid DepartmentSubjectId { get; set; }
    public Guid CoordinatorUserId { get; set; }
    public Guid? EducationProgramId { get; set; }
    public Guid? EducationStageId { get; set; }
    public DateOnly StartsOn { get; set; }
    public DateOnly? EndsOn { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public Guid? DeletedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public AcademicDepartmentSubject DepartmentSubject { get; set; } = null!;
    public LocalUserAccount CoordinatorUser { get; set; } = null!;
    public EducationProgram? EducationProgram { get; set; }
    public EducationStage? EducationStage { get; set; }
}
