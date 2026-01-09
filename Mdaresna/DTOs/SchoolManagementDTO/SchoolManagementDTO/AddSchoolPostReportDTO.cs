using System;
using System.ComponentModel.DataAnnotations;

namespace Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO
{
    public class AddSchoolPostReportDTO
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        public Guid PostId { get; set; }

        [Required]
        public string Description { get; set; }
    }
}
