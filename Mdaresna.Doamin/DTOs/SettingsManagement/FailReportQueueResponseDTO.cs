using Mdaresna.Doamin.Enums;

namespace Mdaresna.Doamin.DTOs.SettingsManagement;

public class FailReportQueueResponseDTO
{
    public Guid ReportQueueId { get; set; }

    public Guid SchoolId { get; set; }

    public Guid? MonthId { get; set; }

    public ReportQueueStatusEnum Status { get; set; }

    public Guid? FailedById { get; set; }

    public DateTime FailedAt { get; set; }

    public string Notes { get; set; } = string.Empty;
}
