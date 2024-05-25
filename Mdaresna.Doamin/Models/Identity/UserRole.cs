using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.Identity
{
    public class UserRole : AuditBase
    {
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual User User { get; set; }

        public Guid RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; }

        public Guid? SchoolId { get; set; }

        [ForeignKey(nameof(SchoolId))]
        public School? School { get; set; }
    }
}
