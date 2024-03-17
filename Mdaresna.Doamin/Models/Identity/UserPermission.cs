using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Mdaresna.Doamin.Models.Identity
{
    public class UserPermission
    {
        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual Mdaresna.Doamin.Models.User.User User { get; set; }

        public Guid PermissionId { get; set; }

        [ForeignKey(nameof(PermissionId))]
        public virtual Permission Permission { get; set; }
    }
}
