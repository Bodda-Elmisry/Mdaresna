namespace Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO
{
    public class CreateSchoolExamRateHeaderDTO
    {
        public string Name { get; set; }
        public string Notes { get; set; }
        public decimal Percentage { get; set; }
        public Guid SchoolId { get; set; }
    }
}
