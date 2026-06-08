using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.UserManagement;

namespace Mdaresna.Doamin.Models.SettingsManagement;

public class ReportQueue
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public School School { get; set; } = new();
    public Guid? GradeId { get; set; }
    public SchoolGrade? Grade { get; set; }
    public Guid? ClassroomId { get; set; }
    public ClassRoom? Classroom { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Guid? MonthId { get; set; }
    public SchoolYearMonth? Month { get; set; }
    public string? WeekName { get; set; }
    public StudentReportTypesEnum ReportType { get; set; }
    public ReportQueueStatusEnum Status { get; set; }
    public Guid CreatedById { get; set; }
    public User CreatedBy { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public Guid? ReviewdById { get; set; }
    public User? ReviewdBy { get; set; }
    public DateTime? PublishedAt { get; set; }
    public string? Errors { get; set; }
    public int? AffectedRows { get; set; }
    public int RetryCount { get; set; }
    public string? Notes { get; set; }

}
