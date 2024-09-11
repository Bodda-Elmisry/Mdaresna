namespace Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO
{
    public class CreateStudentNoteDTO
    {
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public Guid? CourseId { get; set; }
        public Guid SupervisorId { get; set; }
        public Guid ClassRoomId { get; set; }
        public Guid StudentId { get; set; }
    }
}
