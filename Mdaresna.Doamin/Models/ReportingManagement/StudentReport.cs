using Mdaresna.Doamin.Enums;

namespace Mdaresna.Doamin.Models.ReportingManagement;

public class StudentReport
{
    public Guid Id { get; set; }

    public Guid SchoolId { get; set; }

    public Guid StudentId { get; set; }

    public Guid? MonthId { get; set; }

    public string? WeekName { get; set; }

    public string ReportDetails { get; set; } = "{}";

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public int Version { get; set; }

    public StudentReportTypesEnum ReportType { get; set; }
}
