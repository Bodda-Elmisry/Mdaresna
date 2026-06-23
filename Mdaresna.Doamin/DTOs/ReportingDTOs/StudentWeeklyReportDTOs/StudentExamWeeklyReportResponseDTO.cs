namespace Mdaresna.Doamin.DTOs.ReportingDTOs.StudentWeeklyReportDTOs
{
    public class StudentExamWeeklyReportResponseDTO
    {
        public string WeekDay { get; set; } = string.Empty;
        public string WeekDate { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public bool IsAttend { get; set; }
        public decimal ExamRate { get; set; }
        public decimal StudentResult { get; set; }
        public bool IsEvaluated { get; set; }
        public decimal Percintage { get { return ((this.StudentResult / this.ExamRate) * 100); } }
        public string Details { get; set; } = string.Empty;
    }
}
