using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.Identity
{
    public class RolePermissionResultDTO
    {
        public Guid RoleId { get; set; }
        public string RoleName { get; set; }
        public string? Description { get; set; }
        public bool Active { get; set; }
        public bool SchoolRole { get; set; }
        public bool AdminRole { get; set; }
        public Guid PermissionId { get; set; }
        public string PermissionName { get; set; }
        public string PermissionDescription { get; set; }
    }
}
