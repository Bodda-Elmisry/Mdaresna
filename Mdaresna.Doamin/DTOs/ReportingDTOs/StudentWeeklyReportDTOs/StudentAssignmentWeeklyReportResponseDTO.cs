namespace Mdaresna.Doamin.DTOs.ReportingDTOs.StudentWeeklyReportDTOs
{
    public class StudentAssignmentWeeklyReportResponseDTO
    {
        public string WeekDay { get; set; } = string.Empty;
        public string WeekDate { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string TeacherName { get; set; } = string.Empty;
        public bool IsDelievared { get; set; }
        public string DeliveredDate { get; set; } = string.Empty;
        public decimal AssignmentRate { get; set; }
        public decimal StudentResult { get; set; }
        public decimal Percintage { get { return ((this.StudentResult / this.AssignmentRate) * 100); } }
        public string Details { get; set; } = string.Empty;



    }
}
