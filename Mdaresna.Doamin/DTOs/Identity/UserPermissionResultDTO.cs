using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.Identity
{
    public class UserPermissionResultDTO
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string UserFullName { get; set; }
        public Guid? SchoolId { get; set; }
        public string? SchoolName { get; set; }
        public Guid PermissionId { get; set; }
        public string? PermissionKey { get; set; }
        public string PermissionName { get; set; }
        public string PermissionName_AR { get; set; }
        public bool SchoolPermission { get; set; }
        public bool AppPermission { get; set; }
        public bool AllowMapToClassroom { get; set; }
        public Guid? RoleId { get; set; }
        public string RoleName { get; set; }
        public IEnumerable<Guid> Classrooms { get; set; }
    }
}
