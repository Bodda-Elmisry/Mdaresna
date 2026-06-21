namespace Mdaresna.Doamin.DTOs.ReportingDTOs.StudentMonthlyEvaluationReportDTOs
{
    public class MonthlyEvaluationBaseReportRow
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public Guid SchoolId { get; set; }
        public string SchoolName { get; set; } = string.Empty;
        public Guid GradeId { get; set; }
        public Guid ClassRoomId { get; set; }
        public string ClassRoomName { get; set; } = string.Empty;
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public bool ExcludeFromMonthlyTotal { get; set; }
    }
}
