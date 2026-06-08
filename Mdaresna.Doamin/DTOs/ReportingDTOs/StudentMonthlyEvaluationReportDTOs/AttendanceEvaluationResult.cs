namespace Mdaresna.Doamin.DTOs.ReportingDTOs.StudentMonthlyEvaluationReportDTOs
{
    public class AttendanceEvaluationResult
    {
        public Guid StudentId { get; set; }
        public Guid ClassRoomId { get; set; }
        public decimal Evaluation { get; set; }
    }
}
