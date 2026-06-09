using Mdaresna.Doamin.Enums;

namespace Mdaresna.Doamin.DTOs.SettingsManagement;

public class PublishReportQueueResponseDTO
{
    public Guid ReportQueueId { get; set; }

    public Guid SchoolId { get; set; }

    public Guid? MonthId { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public ReportQueueStatusEnum Status { get; set; }

    public DateTime PublishedAt { get; set; }

    public Guid? PublishedById { get; set; }

    public int ReportsCount { get; set; }

    public int StudentsCount { get; set; }

    public int ParentsCount { get; set; }

    public int NotificationsAttempted { get; set; }

    public int NotificationsFailed { get; set; }
}
