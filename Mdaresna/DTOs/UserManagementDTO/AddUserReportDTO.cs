using System.ComponentModel.DataAnnotations;

namespace Mdaresna.DTOs.UserManagementDTO
{
    public class AddUserReportDTO
    {
        [Required]
        public Guid ReporterUserId { get; set; }

        [Required]
        public Guid ReportedUserId { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
