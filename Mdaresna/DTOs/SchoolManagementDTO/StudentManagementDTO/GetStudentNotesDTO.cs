namespace Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO
{
    public class GetStudentNotesDTO
    {
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public string? Notes { get; set; }
        public Guid? CourseId { get; set; }
        public Guid? SupervisorId { get; set; }
        public Guid? ClassRoomId { get; set; }
        public Guid StudentId { get; set; }
        public int pageNumber { get; set; }


    }
}
