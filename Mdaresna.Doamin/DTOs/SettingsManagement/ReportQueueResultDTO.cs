using Mdaresna.Doamin.Enums;

namespace Mdaresna.Doamin.DTOs.SettingsManagement;

public class ReportQueueResultDTO
{
    public Guid Id { get; set; }

    public Guid SchoolId { get; set; }

    public string SchoolName { get; set; } = string.Empty;

    public Guid? GradeId { get; set; }

    public string? GradeName { get; set; }

    public Guid? ClassroomId { get; set; }

    public string? ClassroomName { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public Guid? MonthId { get; set; }

    public string? MonthName { get; set; }

    public string? WeekName { get; set; }

    public StudentReportTypesEnum ReportType { get; set; }

    public ReportQueueStatusEnum Status { get; set; }

    public Guid CreatedById { get; set; }

    public string CreatedByName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Guid? ReviewdById { get; set; }

    public string? ReviewdByName { get; set; }

    public DateTime? PublishedAt { get; set; }

    public string? Errors { get; set; }

    public int? AffectedRows { get; set; }

    public int RetryCount { get; set; }

    public string? Notes { get; set; }
}
