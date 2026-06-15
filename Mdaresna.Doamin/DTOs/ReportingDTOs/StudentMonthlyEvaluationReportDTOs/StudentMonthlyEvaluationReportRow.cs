namespace Mdaresna.Doamin.DTOs.ReportingDTOs.StudentMonthlyEvaluationReportDTOs
{
    public class StudentMonthlyEvaluationReportRow
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
        public decimal ActivityEvaluation { get; set; }
        public decimal AssignmentEvaluation { get; set; }
        public decimal AttendanceEvaluation { get; set; }
        public decimal ExamEvaluation { get; set; }
        public decimal TotalEvaluation { get; set; }
        public string Assessment { get; set; } = string.Empty;
    }
}
