namespace Mdaresna.Schools.Domain.Exams;

public enum ExamWorkflowStage { Schedule, Committees, DepartmentReview, SchoolApproval, Publish }
public enum ExamWorkflowStatus { Configured, Active, Returned, Completed, Cancelled }
public enum ExamWorkflowStepStatus { Waiting, Active, Completed, Cancelled }

public sealed class ExamWorkflow
{
    public Guid ExamSeriesId { get; set; }
    public ExamWorkflowStatus Status { get; set; }
    public int Revision { get; set; } = 1;
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public ExamSeries Series { get; set; } = null!;
    public ICollection<ExamWorkflowStep> Steps { get; set; } = [];
}
public sealed class ExamWorkflowStep
{
    public Guid Id { get; set; }
    public Guid ExamSeriesId { get; set; }
    public Guid? ExamPaperId { get; set; }
    public ExamWorkflowStage Stage { get; set; }
    public int Order { get; set; }
    public Guid AssigneeUserId { get; set; }
    public Guid? BackupUserId { get; set; }
    public Guid? SupervisorUserId { get; set; }
    public DateTimeOffset? DueAtUtc { get; set; }
    public ExamWorkflowStepStatus Status { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public DateTimeOffset? RemindedAtUtc { get; set; }
    public DateTimeOffset? EscalatedAtUtc { get; set; }
    public string? ReturnReason { get; set; }
    public ExamWorkflow Workflow { get; set; } = null!;
}
// Optional delivery channels consume these durable records after the DB transaction commits.
public sealed class ExamNotificationOutbox
{
    public Guid Id { get; set; }
    public string EventKey { get; set; } = "";
    public Guid NotificationId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? DeliveredAtUtc { get; set; }
    public int Attempts { get; set; }
}
public static class ExamWorkflowRules
{
    public static string Permission(ExamWorkflowStage stage) => stage switch {
        ExamWorkflowStage.Schedule => "school.exams.schedule",
        ExamWorkflowStage.Committees => "school.exams.committees.manage",
        ExamWorkflowStage.DepartmentReview or ExamWorkflowStage.SchoolApproval => "school.exams.approve",
        ExamWorkflowStage.Publish => "school.exams.publish",
        _ => throw new ArgumentOutOfRangeException(nameof(stage))
    };
    public static string Key(ExamWorkflowStage stage, Guid? paperId) => $"{stage}:{paperId?.ToString("D") ?? "series"}";
    public static bool AllEarlierCompleted(IEnumerable<ExamWorkflowStep> steps, int order) =>
        steps.Where(x => x.Order < order).All(x => x.Status == ExamWorkflowStepStatus.Completed);
}
