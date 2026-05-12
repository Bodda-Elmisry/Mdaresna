namespace Mdaresna.Doamin.DTOs.ReportingDTOs.StudentWeeklyReportDTOs
{
    public class StudentActivityWeeklyReportResponseDTO
    {
        public string WeekDay { get; set; } = string.Empty;
        public string WeekDate { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public bool IsAttend { get; set; }
        public decimal ActivityRate { get; set; }
        public decimal StudentResult { get; set; }
        public decimal Percintage { get { return ((this.StudentResult / this.ActivityRate) * 100); } }
        public string Details { get; set; } = string.Empty;
    }
}
