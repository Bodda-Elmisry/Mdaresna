using Mdaresna.Doamin.Enums;

namespace Mdaresna.Doamin.DTOs.ReportingDTOs.StudentWeeklyReportDTOs
{
    public class StudentWeeklyReportRequestDTO
    {
        public Guid StudentId { get; set; }
        public ReportPeriodFilterEnum SelectedWeek { get; set; }
    }
}
