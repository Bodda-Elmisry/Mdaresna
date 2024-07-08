namespace Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO
{
    public class CreateClassRoomExamDTO
    {
        public DateTime ExamDate { get; set; }
        public string WeekDay { get; set; }
        public string ExamDetails { get; set; }
        public Guid ClassRoomId { get; set; }
        public Guid SupervisorId { get; set; }
        public Guid MonthId { get; set; }
        public Guid CourseId { get; set; }
        public decimal Rate { get; set; }
    }
}
