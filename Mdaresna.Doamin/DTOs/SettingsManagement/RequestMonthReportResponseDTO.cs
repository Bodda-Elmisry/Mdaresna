namespace Mdaresna.Doamin.DTOs.SettingsManagement;

public class RequestMonthReportResponseDTO
{
    public Guid SchoolId { get; set; }

    public string SchoolName { get; set; } = string.Empty;

    public Guid? GradeId { get; set; }

    public string? GradeName { get; set; }

    public Guid? ClassroomId { get; set; }

    public string? ClassroomName { get; set; }

    public Guid MonthId { get; set; }

    public string MonthName { get; set; } = string.Empty;

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public Guid RequestedById { get; set; }

    public string RequestedByName { get; set; } = string.Empty;

    public DateTime RequestedAt { get; set; }
}
