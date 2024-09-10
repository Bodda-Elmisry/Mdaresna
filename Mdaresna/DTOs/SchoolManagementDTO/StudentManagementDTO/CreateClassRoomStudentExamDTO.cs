namespace Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO
{
    public class CreateClassRoomStudentExamDTO
    {
        public DateTime ExamDate { get; set; }
        public string WeekDay { get; set; }
        public string ExamDetails { get; set; }
        public Guid ClassRoomId { get; set; }
        public Guid SupervisorId { get; set; }
        public Guid MonthId { get; set; }
        public Guid CourseId { get; set; }
        public decimal Rate { get; set; }
        public Guid StudentId { get; set; }
        public decimal? TotalResult { get; set; }
        public bool IsAttend { get; set; }
    }
}
