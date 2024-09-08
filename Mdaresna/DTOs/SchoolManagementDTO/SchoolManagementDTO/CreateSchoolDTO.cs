using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.UserManagement;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO
{
    public class CreateSchoolDTO
    {
        public string SchoolName { get; set; }

        public string About { get; set; }

        public string Vesion { get; set; }

        //public string ImageUrl { get; set; }

        public Guid SchoolTypeId { get; set; }

        public Guid SchoolAdminId { get; set; }
    }
}
