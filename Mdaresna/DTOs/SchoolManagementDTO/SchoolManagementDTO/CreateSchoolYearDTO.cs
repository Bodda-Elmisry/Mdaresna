namespace Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO
{
    public class CreateSchoolYearDTO
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public bool Active { get; set; }
        public bool Completed { get; set; }
        public Guid SchoolId { get; set; }
    }
}
