using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.Base.Relation
{
    public class SupervisorIdRelation : AuditBase
    {
        public Guid SupervisorId { get; set; }

        [ForeignKey(nameof(SupervisorId))]
        public User Supervisor { get; set; }
    }
}
