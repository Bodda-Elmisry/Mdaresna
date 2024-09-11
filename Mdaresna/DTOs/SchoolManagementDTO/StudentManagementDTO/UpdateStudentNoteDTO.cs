namespace Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO
{
    public class UpdateStudentNoteDTO
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public Guid? CourseId { get; set; }
    }
}
