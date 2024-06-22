namespace Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO
{
    public class CreateSchoolCourseDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid SchoolId { get; set; }
        public Guid LanguageId { get; set; }
    }
}
