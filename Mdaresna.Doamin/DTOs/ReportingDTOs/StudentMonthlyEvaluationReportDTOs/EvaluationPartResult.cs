namespace Mdaresna.Doamin.DTOs.ReportingDTOs.StudentMonthlyEvaluationReportDTOs
{
    public class EvaluationPartResult
    {
        public Guid StudentId { get; set; }
        public Guid ClassRoomId { get; set; }
        public Guid CourseId { get; set; }
        public decimal TotalRate { get; set; }
        public decimal StudentResult { get; set; }
        public decimal Evaluation { get; set; }
    }
}
