using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Identity;

namespace Mdaresna.Schools.Domain.Students;

public enum HomeworkDeliveryMode { Online, Offline }
public enum HomeworkStatus { Draft, Published, Closed, Cancelled }
public enum HomeworkQuestionType { SingleChoice, MultipleChoice, Text, FillInBlank }
public enum StudentHomeworkStatus { NotSubmitted, Draft, Submitted, Late, PendingManualReview, AutoGraded, Graded, Returned, Excused }
public enum HomeworkAttemptStatus { Draft, Submitted, Late, PendingManualReview, AutoGraded, Graded, Returned }
public enum HomeworkSubmissionChannel { SchoolApp, FamilyApp }
public enum HomeworkAnswerGradingStatus { Pending, AutoGraded, PendingManualReview, Graded }
public enum HomeworkCorrectAnswersPolicy { Never, AfterSubmission, AfterDueDate, AfterClosing }

public sealed class HomeworkAssignment
{
    public Guid Id { get; set; }
    public Guid ClassSectionSubjectId { get; set; }
    public HomeworkDeliveryMode DeliveryMode { get; set; }
    public HomeworkStatus Status { get; set; } = HomeworkStatus.Draft;
    public string Title { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public Guid? CurriculumSubjectBookId { get; set; }
    public string? BookReference { get; set; }
    public DateTimeOffset DueAtUtc { get; set; }
    public string TimeZoneIdSnapshot { get; set; } = string.Empty;
    public decimal TotalScore { get; set; }
    public bool AllowLateSubmission { get; set; } = true;
    public int MaximumAttempts { get; set; } = 1;
    public bool AllowUnsubmitBeforeDue { get; set; }
    public HomeworkCorrectAnswersPolicy ShowCorrectAnswersAfter { get; set; } = HomeworkCorrectAnswersPolicy.AfterClosing;
    public bool ShuffleQuestions { get; set; }
    public bool ShuffleOptions { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset? PublishedAtUtc { get; set; }
    public Guid? PublishedByUserId { get; set; }
    public DateTimeOffset? ClosedAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public ClassSectionSubject ClassSectionSubject { get; set; } = null!;
    public CurriculumSubjectBook? CurriculumSubjectBook { get; set; }
    public LocalUserAccount CreatedByUser { get; set; } = null!;
    public LocalUserAccount? PublishedByUser { get; set; }
    public ICollection<HomeworkQuestion> Questions { get; set; } = [];
    public ICollection<StudentHomework> Students { get; set; } = [];
    public ICollection<HomeworkAudit> AuditTrail { get; set; } = [];
}

public sealed class HomeworkQuestion
{
    public Guid Id { get; set; }
    public Guid HomeworkAssignmentId { get; set; }
    public HomeworkQuestionType Type { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsRequired { get; set; } = true;
    public decimal MaxScore { get; set; }
    public string? ModelAnswer { get; set; }
    public string? Explanation { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public HomeworkAssignment HomeworkAssignment { get; set; } = null!;
    public ICollection<HomeworkQuestionOption> Options { get; set; } = [];
    public ICollection<HomeworkQuestionBlank> Blanks { get; set; } = [];
    public ICollection<HomeworkStudentAnswer> Answers { get; set; } = [];
}

public sealed class HomeworkQuestionOption
{
    public Guid Id { get; set; }
    public Guid HomeworkQuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public bool IsCorrect { get; set; }
    public HomeworkQuestion HomeworkQuestion { get; set; } = null!;
    public ICollection<HomeworkStudentSelectedOption> SelectedByAnswers { get; set; } = [];
}

public sealed class HomeworkQuestionBlank
{
    public Guid Id { get; set; }
    public Guid HomeworkQuestionId { get; set; }
    public string Token { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public decimal MaxScore { get; set; }
    public bool IgnoreCase { get; set; } = true;
    public bool IgnoreDiacritics { get; set; } = true;
    public bool CollapseWhitespace { get; set; } = true;
    public bool SendUnmatchedToManualReview { get; set; } = true;
    public HomeworkQuestion HomeworkQuestion { get; set; } = null!;
    public ICollection<HomeworkBlankAcceptedAnswer> AcceptedAnswers { get; set; } = [];
    public ICollection<HomeworkStudentBlankAnswer> StudentAnswers { get; set; } = [];
}

public sealed class HomeworkBlankAcceptedAnswer
{
    public Guid Id { get; set; }
    public Guid HomeworkQuestionBlankId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public string NormalizedAnswer { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public HomeworkQuestionBlank HomeworkQuestionBlank { get; set; } = null!;
}

public sealed class StudentHomework
{
    public Guid Id { get; set; }
    public Guid HomeworkAssignmentId { get; set; }
    public Guid StudentEnrollmentId { get; set; }
    public StudentHomeworkStatus Status { get; set; } = StudentHomeworkStatus.NotSubmitted;
    public Guid? CurrentAttemptId { get; set; }
    public decimal? FinalScore { get; set; }
    public string? TeacherFeedback { get; set; }
    public Guid? GradedByUserId { get; set; }
    public DateTimeOffset? GradedAtUtc { get; set; }
    public string? ExcuseReason { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public HomeworkAssignment HomeworkAssignment { get; set; } = null!;
    public StudentEnrollment StudentEnrollment { get; set; } = null!;
    public LocalUserAccount? GradedByUser { get; set; }
    public ICollection<HomeworkSubmissionAttempt> Attempts { get; set; } = [];
}

public sealed class HomeworkSubmissionAttempt
{
    public Guid Id { get; set; }
    public Guid StudentHomeworkId { get; set; }
    public int AttemptNumber { get; set; }
    public HomeworkSubmissionChannel Channel { get; set; }
    public HomeworkAttemptStatus Status { get; set; } = HomeworkAttemptStatus.Draft;
    public Guid? SubmittedByUserId { get; set; }
    public string? SubmittedByActorType { get; set; }
    public DateTimeOffset? SubmittedAtUtc { get; set; }
    public decimal AutoScore { get; set; }
    public decimal? FinalScore { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public StudentHomework StudentHomework { get; set; } = null!;
    public ICollection<HomeworkStudentAnswer> Answers { get; set; } = [];
}

public sealed class HomeworkStudentAnswer
{
    public Guid Id { get; set; }
    public Guid HomeworkSubmissionAttemptId { get; set; }
    public Guid HomeworkQuestionId { get; set; }
    public string? TextAnswer { get; set; }
    public HomeworkAnswerGradingStatus GradingStatus { get; set; } = HomeworkAnswerGradingStatus.Pending;
    public decimal? Score { get; set; }
    public string? TeacherFeedback { get; set; }
    public Guid? GradedByUserId { get; set; }
    public DateTimeOffset? GradedAtUtc { get; set; }
    public HomeworkSubmissionAttempt Attempt { get; set; } = null!;
    public HomeworkQuestion HomeworkQuestion { get; set; } = null!;
    public LocalUserAccount? GradedByUser { get; set; }
    public ICollection<HomeworkStudentSelectedOption> SelectedOptions { get; set; } = [];
    public ICollection<HomeworkStudentBlankAnswer> BlankAnswers { get; set; } = [];
}

public sealed class HomeworkStudentSelectedOption
{
    public Guid HomeworkStudentAnswerId { get; set; }
    public Guid HomeworkQuestionOptionId { get; set; }
    public HomeworkStudentAnswer HomeworkStudentAnswer { get; set; } = null!;
    public HomeworkQuestionOption HomeworkQuestionOption { get; set; } = null!;
}

public sealed class HomeworkStudentBlankAnswer
{
    public Guid Id { get; set; }
    public Guid HomeworkStudentAnswerId { get; set; }
    public Guid HomeworkQuestionBlankId { get; set; }
    public string Answer { get; set; } = string.Empty;
    public string NormalizedAnswer { get; set; } = string.Empty;
    public bool? IsMatched { get; set; }
    public decimal? Score { get; set; }
    public bool IsManualOverride { get; set; }
    public HomeworkStudentAnswer HomeworkStudentAnswer { get; set; } = null!;
    public HomeworkQuestionBlank HomeworkQuestionBlank { get; set; } = null!;
}

public sealed class HomeworkAudit
{
    public Guid Id { get; set; }
    public Guid HomeworkAssignmentId { get; set; }
    public Guid? StudentHomeworkId { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid ActorUserId { get; set; }
    public string? PayloadJson { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public HomeworkAssignment HomeworkAssignment { get; set; } = null!;
    public StudentHomework? StudentHomework { get; set; }
    public LocalUserAccount ActorUser { get; set; } = null!;
}
