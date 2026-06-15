using Mdaresna.Doamin.Enums;

namespace Mdaresna.Doamin.DTOs.ReportingDTOs;

public class StudentReportResultDTO
{
    public Guid Id { get; set; }

    public Guid SchoolId { get; set; }

    public Guid StudentId { get; set; }

    public Guid? ReportQueueId { get; set; }

    public Guid? GradeId { get; set; }

    public Guid? ClassRoomId { get; set; }

    public Guid? MonthId { get; set; }

    public string? WeekName { get; set; }

    public string ReportDetails { get; set; } = "{}";

    public DateTime CreatedAt { get; set; }

    public bool IsActive { get; set; }

    public int Version { get; set; }

    public StudentReportTypesEnum ReportType { get; set; }
}
