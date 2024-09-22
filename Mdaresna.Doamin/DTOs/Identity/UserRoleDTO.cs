using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.Identity
{
    public class UserRoleDTO
    {
        public Guid UserId { get; set; }
        public Guid RoleId { get; set; }
        public Guid? SchoolId { get; set; }
    }
}
