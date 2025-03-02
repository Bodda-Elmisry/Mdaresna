namespace Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO
{
    public class GetSchoolDTO
    {
        public string? Name { get; set; }
        public bool? Active { get; set; }
        public Guid? SchoolTypeId { get; set; }
        public Guid? CoinTypeId { get; set; }
        public Guid? SchoolAdminId { get; set; }
        public bool? NewSchools { get; set; }
        public int PageNumber { get; set; }

    }
}
