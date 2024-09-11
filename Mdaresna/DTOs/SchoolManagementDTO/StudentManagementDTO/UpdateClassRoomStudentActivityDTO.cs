namespace Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO
{
    public class UpdateClassRoomStudentActivityDTO
    {
        public Guid ActivityId { get; set; }
        public Guid StudentId { get; set; }
        public decimal? Result { get; set; }
        public bool IsAttend { get; set; }

    }
}
