namespace Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO
{
    public class UpdateStudentDTO : CreateStudentDTO
    {
        public Guid Id { get; set; }
        public string Code { get; set; }
    }
}
