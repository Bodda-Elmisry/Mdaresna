namespace Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO
{
    public class CreateSchoolYearMonthDTO
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; }
        public Guid YearId { get; set; }
    }
}
