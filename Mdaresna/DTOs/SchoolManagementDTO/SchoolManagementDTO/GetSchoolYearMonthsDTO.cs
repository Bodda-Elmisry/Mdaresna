using Mdaresna.DTOs.Common;

namespace Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO
{
    public class GetSchoolYearMonthsDTO : YearIdDTO
    {
        public string Name { get; set; }
        public bool? IsActive { get; set; }
    }
}
