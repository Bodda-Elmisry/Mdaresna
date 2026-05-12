namespace Mdaresna.Doamin.DTOs.ReportingDTOs.StudentWeeklyReportDTOs
{
    public class StudentAttendanceWeeklyReportResponseDTO
    {
        public string WeekDate { get; set; } = string.Empty;
        public string WeekDay { get; set; } = string.Empty;
        public bool IsAttend { get; set; }
        public bool IsExcption { get; set; }
    }
}
