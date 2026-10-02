using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Facilities;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.Students;

namespace Mdaresna.Schools.Domain.Exams;

public enum ExamKind { Weekly, Monthly, MidTerm, TermFinal, YearFinal, Other }
public enum ExamScopeLevel { ClassSection, Grade, Stage }
public enum ExamSeriesPurpose { Regular, MakeUp, Retake, Replacement }
public enum ExamAuthorityType { EducationalAuthority, School, Department, Teacher }
public enum ExamSeriesStatus { Draft, PendingApproval, Approved, Published, Closed, Cancelled }
public enum ExamAdministrationMode { InClass, Committee }
public enum ExamFormat { Written, Oral, Practical, Online, Mixed }
public enum ExamSittingPurpose { Regular, MakeUp, Retake, Replacement, Special }
public enum ExamScheduleWindowStatus { Draft, Scheduled, Postponed, Cancelled }
public enum ExamSittingExecutionStatus { NotStarted, InProgress, Completed, Cancelled }
public enum ExamResultsStatus { NotStarted, EntryOpen, PendingApproval, Approved, Published }
public enum ExamCandidateStatus { Eligible, Exempt, Withdrawn }
public enum ExamAttendanceStatus { NotRecorded, Present, Late, AbsentExcused, AbsentUnexcused }
public enum ExamResultDisposition { Pending, Scored, Exempt, AbsentExcused, AbsentUnexcused, Disqualified, Withheld }
public enum ExamApprovalStage { Schedule, Publish, Results }
public enum ExamApprovalStatus { Pending, Approved, Rejected, Skipped }
public enum ExamInvigilatorRole { Chief, Invigilator, Observer, Reserve }
public enum ExamInvigilatorStatus { Assigned, Confirmed, Absent, Replaced, Cancelled }
public enum ExamAppealStatus { Submitted, UnderReview, Accepted, Rejected, Cancelled }

public static class SchoolExamRules
{
    public static bool HasValidScoreDefinition(decimal totalScore, decimal? passScore) =>
        totalScore > 0 && totalScore <= 10000 && (!passScore.HasValue || passScore.Value >= 0 && passScore.Value <= totalScore);

    public static bool HasValidWindow(DateOnly date, TimeOnly startsAt, TimeOnly endsAt) =>
        date != default && endsAt > startsAt;

    public static bool HasValidResult(ExamResultDisposition disposition, decimal? score, decimal totalScore) => disposition switch
    {
        ExamResultDisposition.Scored => score.HasValue && score.Value >= 0 && score.Value <= totalScore,
        ExamResultDisposition.Pending => !score.HasValue,
        _ => !score.HasValue
    };

    public static bool Overlaps(DateTimeOffset leftStart, DateTimeOffset leftEnd,
        DateTimeOffset rightStart, DateTimeOffset rightEnd) => leftStart < rightEnd && rightStart < leftEnd;

    public static bool CanPublish(ExamSeriesStatus status) => status is ExamSeriesStatus.Draft or ExamSeriesStatus.Approved;
    public static bool CanEnterResults(ExamResultsStatus status) => status is ExamResultsStatus.EntryOpen;

    public static bool MonthIntersectsTerm(DateOnly assessmentMonth, DateOnly termStart, DateOnly termEnd)
    {
        if (assessmentMonth.Day != 1 || termEnd < termStart) return false;
        var monthEnd = assessmentMonth.AddMonths(1).AddDays(-1);
        return assessmentMonth <= termEnd && termStart <= monthEnd;
    }

    public static ExamKind ResolveFinalKind(int selectedTermSortOrder, int lastTermSortOrder) =>
        selectedTermSortOrder == lastTermSortOrder ? ExamKind.YearFinal : ExamKind.TermFinal;
}

public sealed class ExamPolicy
{
    public string? WorkflowDefaultsJson { get; set; }
    public Guid Id { get; set; }
    public Guid? EducationProgramId { get; set; }
    public Guid? EducationStageId { get; set; }
    public ExamKind? ExamKind { get; set; }
    public ExamAdministrationMode DefaultAdministrationMode { get; set; } = ExamAdministrationMode.InClass;
    public bool TeacherCanCreate { get; set; } = true;
    public bool TeacherCanPublishWithoutApproval { get; set; }
    public bool DepartmentApprovalRequired { get; set; }
    public bool SchoolApprovalRequired { get; set; }
    public bool ResultApprovalRequired { get; set; }
    public bool AllowScheduleWarningOverride { get; set; }
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public int Version { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public EducationProgram? EducationProgram { get; set; }
    public EducationStage? EducationStage { get; set; }
}

public sealed class ExamSeries
{
    public Guid Id { get; set; }
    public Guid ProgramAcademicYearId { get; set; }
    public Guid? AcademicTermId { get; set; }
    public Guid? EducationStageId { get; set; }
    public Guid? ScopeGradeOfferingId { get; set; }
    public Guid? ScopeClassSectionId { get; set; }
    public ExamScopeLevel? ScopeLevel { get; set; }
    public DateOnly? AssessmentMonth { get; set; }
    public ExamSeriesPurpose Purpose { get; set; } = ExamSeriesPurpose.Regular;
    public Guid? ParentExamSeriesId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public ExamKind Kind { get; set; }
    public ExamAuthorityType IssuingAuthority { get; set; } = ExamAuthorityType.School;
    public ExamAuthorityType SchedulingAuthority { get; set; } = ExamAuthorityType.School;
    public ExamAdministrationMode DefaultAdministrationMode { get; set; } = ExamAdministrationMode.InClass;
    public ExamSeriesStatus Status { get; set; } = ExamSeriesStatus.Draft;
    public string TimeZoneIdSnapshot { get; set; } = string.Empty;
    public string? PolicySnapshotJson { get; set; }
    public string? ExternalSourceCode { get; set; }
    public string? ExternalAuthorityName { get; set; }
    public string? ExternalReferenceId { get; set; }
    public string? ExternalRevision { get; set; }
    public string? ExternalPayloadHash { get; set; }
    public bool IsExternalScheduleLocked { get; set; }
    public Guid CreatedByUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public Guid? PublishedByUserId { get; set; }
    public Guid? ClosedByUserId { get; set; }
    public Guid? CancelledByUserId { get; set; }
    public string? CancellationReason { get; set; }
    public DateTimeOffset? ApprovedAtUtc { get; set; }
    public DateTimeOffset? PublishedAtUtc { get; set; }
    public DateTimeOffset? ClosedAtUtc { get; set; }
    public DateTimeOffset? CancelledAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public bool IsDeleted { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ProgramAcademicYear ProgramAcademicYear { get; set; } = null!;
    public AcademicTerm? AcademicTerm { get; set; }
    public EducationStage? EducationStage { get; set; }
    public GradeOffering? ScopeGradeOffering { get; set; }
    public ClassSection? ScopeClassSection { get; set; }
    public ExamSeries? ParentExamSeries { get; set; }
    public LocalUserAccount CreatedByUser { get; set; } = null!;
    public ICollection<ExamSeriesTarget> Targets { get; set; } = [];
    public ICollection<ExamPaper> Papers { get; set; } = [];
    public ICollection<ExamScheduleWindow> ScheduleWindows { get; set; } = [];
    public ICollection<ExamCandidate> Candidates { get; set; } = [];
    public ICollection<ExamApproval> Approvals { get; set; } = [];
    public ICollection<ExamAudit> AuditTrail { get; set; } = [];
    public ICollection<ExamCommittee> Committees { get; set; } = [];
}

public sealed class ExamSeriesTarget
{
    public Guid Id { get; set; }
    public Guid ExamSeriesId { get; set; }
    public Guid GradeOfferingId { get; set; }
    public Guid? ClassSectionId { get; set; }
    public ExamSeries ExamSeries { get; set; } = null!;
    public GradeOffering GradeOffering { get; set; } = null!;
    public ClassSection? ClassSection { get; set; }
}

public sealed class ExamPaper
{
    public Guid Id { get; set; }
    public Guid ExamSeriesId { get; set; }
    public Guid GradeSubjectOfferingId { get; set; }
    public string PaperCode { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public ExamFormat Format { get; set; } = ExamFormat.Written;
    public string? Instructions { get; set; }
    public decimal TotalScore { get; set; }
    public decimal? PassScore { get; set; }
    public int DurationMinutes { get; set; }
    public int SortOrder { get; set; }
    public Guid? ContentOwnerUserId { get; set; }
    public ExamResultsStatus ResultsStatus { get; set; } = ExamResultsStatus.NotStarted;
    public int ResultsRevision { get; set; }
    public string? SubjectCodeSnapshot { get; set; }
    public string? SubjectNameArSnapshot { get; set; }
    public string? SubjectNameEnSnapshot { get; set; }
    public string? GradeNameArSnapshot { get; set; }
    public string? GradeNameEnSnapshot { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ExamSeries ExamSeries { get; set; } = null!;
    public GradeSubjectOffering GradeSubjectOffering { get; set; } = null!;
    public LocalUserAccount? ContentOwnerUser { get; set; }
    public ICollection<ExamPaperTarget> Targets { get; set; } = [];
    public ICollection<ExamSitting> Sittings { get; set; } = [];
    public ICollection<ExamPaperCandidate> Candidates { get; set; } = [];
}

public sealed class ExamPaperTarget
{
    public Guid Id { get; set; }
    public Guid ExamPaperId { get; set; }
    public Guid GradeOfferingId { get; set; }
    public Guid? ClassSectionId { get; set; }
    public Guid? ClassSectionSubjectId { get; set; }
    public ExamPaper ExamPaper { get; set; } = null!;
    public GradeOffering GradeOffering { get; set; } = null!;
    public ClassSection? ClassSection { get; set; }
    public ClassSectionSubject? ClassSectionSubject { get; set; }
}

public sealed class ExamScheduleWindow
{
    public Guid Id { get; set; }
    public Guid ExamSeriesId { get; set; }
    public DateOnly LocalDate { get; set; }
    public TimeOnly StartsAtLocal { get; set; }
    public TimeOnly EndsAtLocal { get; set; }
    public DateTimeOffset StartsAtUtc { get; set; }
    public DateTimeOffset EndsAtUtc { get; set; }
    public string TimeZoneIdSnapshot { get; set; } = string.Empty;
    public ExamScheduleWindowStatus Status { get; set; } = ExamScheduleWindowStatus.Draft;
    public bool IsScheduleLocked { get; set; }
    public Guid? RescheduledFromWindowId { get; set; }
    public string? PostponementReason { get; set; }
    public string? CancellationReason { get; set; }
    public Guid ScheduledByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ExamSeries ExamSeries { get; set; } = null!;
    public ExamScheduleWindow? RescheduledFromWindow { get; set; }
    public ICollection<ExamSitting> Sittings { get; set; } = [];
    public ICollection<ExamWindowVenue> Venues { get; set; } = [];
    public ICollection<ExamCommittee> Committees { get; set; } = [];
    public ExamCalendarProjection? CalendarProjection { get; set; }
}

public sealed class ExamSitting
{
    public Guid Id { get; set; }
    public Guid ExamPaperId { get; set; }
    public Guid ExamScheduleWindowId { get; set; }
    public Guid? ParentSittingId { get; set; }
    public ExamSittingPurpose Purpose { get; set; } = ExamSittingPurpose.Regular;
    public ExamAdministrationMode AdministrationMode { get; set; } = ExamAdministrationMode.InClass;
    public int StartsAtOffsetMinutes { get; set; }
    public int DurationMinutesSnapshot { get; set; }
    public ExamSittingExecutionStatus ExecutionStatus { get; set; } = ExamSittingExecutionStatus.NotStarted;
    public Guid? StartedByUserId { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public DateTimeOffset? StartedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ExamPaper ExamPaper { get; set; } = null!;
    public ExamScheduleWindow ExamScheduleWindow { get; set; } = null!;
    public ExamSitting? ParentSitting { get; set; }
    public ICollection<ExamSittingVenue> Venues { get; set; } = [];
    public ICollection<ExamCandidateSittingAssignment> CandidateAssignments { get; set; } = [];
}

public sealed class ExamWindowVenue
{
    public Guid Id { get; set; }
    public Guid ExamScheduleWindowId { get; set; }
    public Guid? RoomId { get; set; }
    public Guid? ClassSectionId { get; set; }
    public Guid? ExamCommitteeId { get; set; }
    public int CapacitySnapshot { get; set; }
    public string? VenueLabel { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ExamScheduleWindow ExamScheduleWindow { get; set; } = null!;
    public SchoolRoom? Room { get; set; }
    public ClassSection? ClassSection { get; set; }
    public ExamCommittee? ExamCommittee { get; set; }
    public ICollection<ExamSittingVenue> Sittings { get; set; } = [];
}

public sealed class ExamSittingVenue
{
    public Guid Id { get; set; }
    public Guid ExamSittingId { get; set; }
    public Guid ExamWindowVenueId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ExamSitting ExamSitting { get; set; } = null!;
    public ExamWindowVenue ExamWindowVenue { get; set; } = null!;
    public ICollection<ExamCandidateSittingAssignment> CandidateAssignments { get; set; } = [];
}

public sealed class ExamCandidate
{
    public Guid Id { get; set; }
    public Guid ExamSeriesId { get; set; }
    public Guid StudentId { get; set; }
    public Guid StudentEnrollmentIdSnapshot { get; set; }
    public Guid GlobalStudentIdSnapshot { get; set; }
    public Guid GradeOfferingIdSnapshot { get; set; }
    public Guid ClassSectionIdSnapshot { get; set; }
    public string StudentCodeSnapshot { get; set; } = string.Empty;
    public string NameArSnapshot { get; set; } = string.Empty;
    public string NameEnSnapshot { get; set; } = string.Empty;
    public string? ExamNumber { get; set; }
    public ExamCandidateStatus Status { get; set; } = ExamCandidateStatus.Eligible;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ExamSeries ExamSeries { get; set; } = null!;
    public Student Student { get; set; } = null!;
    public StudentEnrollment StudentEnrollmentSnapshot { get; set; } = null!;
    public ICollection<ExamPaperCandidate> Papers { get; set; } = [];
}

public sealed class ExamPaperCandidate
{
    public Guid Id { get; set; }
    public Guid ExamPaperId { get; set; }
    public Guid ExamCandidateId { get; set; }
    public ExamCandidateStatus Eligibility { get; set; } = ExamCandidateStatus.Eligible;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ExamPaper ExamPaper { get; set; } = null!;
    public ExamCandidate ExamCandidate { get; set; } = null!;
    public ICollection<ExamCandidateSittingAssignment> SittingAssignments { get; set; } = [];
    public ICollection<ExamResultAttempt> ResultAttempts { get; set; } = [];
    public ICollection<ExamResultAppeal> Appeals { get; set; } = [];
}

public sealed class ExamCandidateSittingAssignment
{
    public Guid Id { get; set; }
    public Guid ExamPaperId { get; set; }
    public Guid ExamPaperCandidateId { get; set; }
    public Guid ExamSittingId { get; set; }
    public Guid ExamSittingVenueId { get; set; }
    public string? DeskOrSeatLabel { get; set; }
    public int? SeatNumber { get; set; }
    public Guid AssignedByUserId { get; set; }
    public DateTimeOffset AssignedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ExamPaperCandidate ExamPaperCandidate { get; set; } = null!;
    public ExamSitting ExamSitting { get; set; } = null!;
    public ExamSittingVenue ExamSittingVenue { get; set; } = null!;
    public ExamAttendance? Attendance { get; set; }
    public ICollection<ExamResultAttempt> ResultAttempts { get; set; } = [];
}

public sealed class ExamAttendance
{
    public Guid Id { get; set; }
    public Guid ExamCandidateSittingAssignmentId { get; set; }
    public ExamAttendanceStatus Status { get; set; } = ExamAttendanceStatus.NotRecorded;
    public TimeOnly? ArrivedAt { get; set; }
    public string? Notes { get; set; }
    public Guid? RecordedByUserId { get; set; }
    public DateTimeOffset? RecordedAtUtc { get; set; }
    public Guid? FinalizedByUserId { get; set; }
    public DateTimeOffset? FinalizedAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ExamCandidateSittingAssignment ExamCandidateSittingAssignment { get; set; } = null!;
}

public sealed class ExamResultAttempt
{
    public Guid Id { get; set; }
    public Guid ExamPaperCandidateId { get; set; }
    public Guid? ExamCandidateSittingAssignmentId { get; set; }
    public int AttemptNumber { get; set; } = 1;
    public ExamResultDisposition Disposition { get; set; } = ExamResultDisposition.Pending;
    public decimal? Score { get; set; }
    public bool IsFinal { get; set; } = true;
    public Guid? MarkerUserId { get; set; }
    public Guid? ModeratorUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? MarkedAtUtc { get; set; }
    public DateTimeOffset? ModeratedAtUtc { get; set; }
    public DateTimeOffset? ApprovedAtUtc { get; set; }
    public DateTimeOffset? PublishedAtUtc { get; set; }
    public string? Notes { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ExamPaperCandidate ExamPaperCandidate { get; set; } = null!;
    public ExamCandidateSittingAssignment? ExamCandidateSittingAssignment { get; set; }
}

public sealed class ExamCommittee
{
    public Guid Id { get; set; }
    public Guid ExamSeriesId { get; set; }
    public Guid ExamScheduleWindowId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public int Capacity { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ExamSeries ExamSeries { get; set; } = null!;
    public ExamScheduleWindow ExamScheduleWindow { get; set; } = null!;
    public ICollection<ExamWindowVenue> Venues { get; set; } = [];
    public ICollection<ExamInvigilatorAssignment> Invigilators { get; set; } = [];
}

public sealed class ExamInvigilatorAssignment
{
    public Guid Id { get; set; }
    public Guid ExamCommitteeId { get; set; }
    public Guid UserId { get; set; }
    public ExamInvigilatorRole Role { get; set; } = ExamInvigilatorRole.Invigilator;
    public ExamInvigilatorStatus Status { get; set; } = ExamInvigilatorStatus.Assigned;
    public Guid? ReplacesAssignmentId { get; set; }
    public string? Notes { get; set; }
    public Guid AssignedByUserId { get; set; }
    public DateTimeOffset AssignedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ExamCommittee ExamCommittee { get; set; } = null!;
    public LocalUserAccount User { get; set; } = null!;
    public ExamInvigilatorAssignment? ReplacesAssignment { get; set; }
}

public sealed class ExamResultAppeal
{
    public Guid Id { get; set; }
    public Guid ExamPaperCandidateId { get; set; }
    public ExamAppealStatus Status { get; set; } = ExamAppealStatus.Submitted;
    public string Reason { get; set; } = string.Empty;
    public string? DecisionNotes { get; set; }
    public decimal? PreviousScore { get; set; }
    public decimal? RevisedScore { get; set; }
    public Guid RequestedByUserId { get; set; }
    public Guid? ReviewedByUserId { get; set; }
    public DateTimeOffset RequestedAtUtc { get; set; }
    public DateTimeOffset? ReviewedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ExamPaperCandidate ExamPaperCandidate { get; set; } = null!;
}

public sealed class ExamApproval
{
    public Guid Id { get; set; }
    public Guid ExamSeriesId { get; set; }
    public Guid? ExamPaperId { get; set; }
    public ExamApprovalStage Stage { get; set; }
    public int StepOrder { get; set; }
    public Guid? ApproverUserId { get; set; }
    public ExamApprovalStatus Status { get; set; } = ExamApprovalStatus.Pending;
    public string? DecisionReason { get; set; }
    public DateTimeOffset? DecidedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ExamSeries ExamSeries { get; set; } = null!;
    public ExamPaper? ExamPaper { get; set; }
}

public sealed class ExamAudit
{
    public Guid Id { get; set; }
    public Guid ExamSeriesId { get; set; }
    public Guid? ExamPaperId { get; set; }
    public Guid? ExamSittingId { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid ActorUserId { get; set; }
    public string? Reason { get; set; }
    public string? PayloadJson { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public ExamSeries ExamSeries { get; set; } = null!;
    public LocalUserAccount ActorUser { get; set; } = null!;
}

public sealed class ExamCalendarProjection
{
    public Guid Id { get; set; }
    public Guid ExamScheduleWindowId { get; set; }
    public Guid SchoolCalendarEventId { get; set; }
    public int ProjectionVersion { get; set; } = 1;
    public DateTimeOffset LastProjectedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ExamScheduleWindow ExamScheduleWindow { get; set; } = null!;
    public SchoolCalendarEvent SchoolCalendarEvent { get; set; } = null!;
}
