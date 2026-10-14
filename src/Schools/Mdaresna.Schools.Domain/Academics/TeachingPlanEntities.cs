using Mdaresna.Schools.Domain.Documents;

namespace Mdaresna.Schools.Domain.Academics;

public enum TeachingPlanType { Yearly, Term, Monthly, Weekly, Daily }
public enum TeachingPlanStatus { Draft, Submitted, Approved, Published, Archived }
public enum TeachingPlanSourceAuthority { Ministry, EducationalAdministration, School }
public enum TeachingPlanDocumentPurpose { MainPlan, Reference, Supporting }

public sealed class TeachingPlan
{
    public Guid Id { get; set; }
    public TeachingPlanType Type { get; set; }
    public TeachingPlanStatus Status { get; set; } = TeachingPlanStatus.Draft;
    public Guid ProgramAcademicYearId { get; set; }
    public Guid GradeOfferingId { get; set; }
    public Guid GradeSubjectOfferingId { get; set; }
    public Guid? AcademicTermId { get; set; }
    public Guid? ParentPlanId { get; set; }
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
    public TeachingPlanSourceAuthority SourceAuthority { get; set; }
    public string? SourceReference { get; set; }
    public DateOnly FromDate { get; set; }
    public DateOnly ToDate { get; set; }
    public int Revision { get; set; } = 1;
    public Guid CreatedByUserId { get; set; }
    public Guid? SubmittedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public Guid? PublishedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public DateTimeOffset? SubmittedAtUtc { get; set; }
    public DateTimeOffset? ApprovedAtUtc { get; set; }
    public DateTimeOffset? PublishedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ProgramAcademicYear ProgramAcademicYear { get; set; } = null!;
    public GradeOffering GradeOffering { get; set; } = null!;
    public GradeSubjectOffering GradeSubjectOffering { get; set; } = null!;
    public AcademicTerm? AcademicTerm { get; set; }
    public TeachingPlan? ParentPlan { get; set; }
    public ICollection<TeachingPlanTarget> Targets { get; set; } = [];
    public ICollection<TeachingPlanItem> Items { get; set; } = [];
    public ICollection<TeachingPlanDayDetail> DayDetails { get; set; } = [];
    public ICollection<TeachingPlanDocument> Documents { get; set; } = [];
}

public sealed class TeachingPlanTarget
{
    public Guid Id { get; set; }
    public Guid TeachingPlanId { get; set; }
    public Guid ClassSectionId { get; set; }
    public TeachingPlan TeachingPlan { get; set; } = null!;
    public ClassSection ClassSection { get; set; } = null!;
}

public sealed class TeachingPlanDayDetail
{
    public Guid Id { get; set; }
    public Guid TeachingPlanId { get; set; }
    public DateOnly Date { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public string Details { get; set; } = string.Empty;
    public TeachingPlan TeachingPlan { get; set; } = null!;
}

public static class WeeklyPlanDayRules
{
    public static DateOnly SaturdayStart(DateOnly date) => date.AddDays(-(((int)date.DayOfWeek + 1) % 7));
    public static bool IsTermWeek(DateOnly from, DateOnly to, DateOnly termStart, DateOnly termEnd)
    {
        var saturday = SaturdayStart(from);
        var expectedStart = saturday < termStart ? termStart : saturday;
        var friday = saturday.AddDays(6);
        return from >= termStart && from <= termEnd && from == expectedStart &&
            to == (friday > termEnd ? termEnd : friday);
    }
    public static bool IsValid(DateOnly from, DateOnly to, IReadOnlyList<(DateOnly Date, string Details)> days) =>
        from <= to && to.DayNumber - from.DayNumber <= 6 && days.Count is > 0 and <= 7 &&
        days.Select(x => x.Date).Distinct().Count() == days.Count &&
        days.All(x => x.Date >= from && x.Date <= to && !string.IsNullOrWhiteSpace(x.Details) && x.Details.Trim().Length <= 4000);
}

public static class TeachingPlanPeriodRules
{
    public static bool Matches(TeachingPlanType type, DateOnly from, DateOnly to, DateOnly start, DateOnly end) =>
        start <= end && (type == TeachingPlanType.Monthly ? from == start && to == end :
            type == TeachingPlanType.Weekly && WeeklyPlanDayRules.IsTermWeek(from,to,start,end));
}

public sealed class TeachingPlanItem
{
    public Guid Id { get; set; }
    public Guid TeachingPlanId { get; set; }
    public int SortOrder { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Details { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }
    public TeachingPlan TeachingPlan { get; set; } = null!;
}

public sealed class TeachingPlanDocument
{
    public Guid Id { get; set; }
    public Guid TeachingPlanId { get; set; }
    public Guid DocumentId { get; set; }
    public TeachingPlanDocumentPurpose Purpose { get; set; }
    public int SortOrder { get; set; }
    public TeachingPlan TeachingPlan { get; set; } = null!;
    public SchoolDocument Document { get; set; } = null!;
}

public sealed class TeachingPlanAudit
{
    public Guid Id { get; set; }
    public Guid TeachingPlanId { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid ActorUserId { get; set; }
    public string? Details { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public TeachingPlan TeachingPlan { get; set; } = null!;
}
