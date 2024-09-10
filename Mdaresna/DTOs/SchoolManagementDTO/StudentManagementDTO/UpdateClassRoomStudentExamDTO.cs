namespace Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO
{
    public class UpdateClassRoomStudentExamDTO
    {
        public Guid ExamId { get; set; }
        public Guid StudentId { get; set; }
        public decimal? TotalResult { get; set; }
        public bool IsAttend { get; set; }
    }
}
