namespace Mdaresna.Doamin.DTOs.ReportingDTOs.StudentWeeklyReportDTOs
{
    public class StudentWeeklyReportResponseDTO
    {
        public string StudentName { get; set; } = string.Empty;
        public string StudentCode { get; set; } = string.Empty;
        public string SchoolName { get; set; } = string.Empty;
        public string ClassroomName { get; set; } = string.Empty;
        public string GradeName { get; set; } = string.Empty;
        public string FromDate { get; set; } = string.Empty;
        public string ToDate { get; set; } = string.Empty;
        public string Year { get; set; } = string.Empty; // active year
        public List<StudentAttendanceWeeklyReportResponseDTO> AttendanceReport { get; set; } = new();
        public List<StudentAssignmentWeeklyReportResponseDTO> AssignmentReport { get; set; } = new();
        public List<StudentExamWeeklyReportResponseDTO> ExamReport { get; set; } = new();
        public List<StudentActivityWeeklyReportResponseDTO> ActivityReport { get; set; } = new();
        public List<StudentNoteWeeklyReportResponseDTO> NoteReport { get; set; } = new();
    }
}
