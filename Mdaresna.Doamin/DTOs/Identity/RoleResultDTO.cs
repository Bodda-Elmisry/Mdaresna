using Mdaresna.Doamin.Models.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.Identity
{
    public class RoleResultDTO
    {
        public Guid RoleId { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool Active { get; set; }
        public bool SchoolRole { get; set; }
        public bool AdminRole { get; set; }

        public int PermissionsCount { get; set; }
    }
}
