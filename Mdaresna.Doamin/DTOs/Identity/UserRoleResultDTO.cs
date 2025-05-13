using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.Identity
{
    public class UserRoleResultDTO
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string UserFullName { get; set; }
        public string? UserImageUrl { get; set; }
        public string UserPhoneNumber { get; set; }
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public string? RoleDescription { get; set; }
        public Guid? SchoolId { get; set; }
        public string? SchoolName { get; set; }
    }
}
