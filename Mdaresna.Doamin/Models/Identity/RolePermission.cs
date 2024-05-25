using Mdaresna.Doamin.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.Identity
{
    public class RolePermission : AuditBase
    {
        public Guid RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; }

        public Guid PermissionId { get; set; }

        [ForeignKey(nameof(PermissionId))]
        public virtual Permission Permission { get; set; }
    }
}
