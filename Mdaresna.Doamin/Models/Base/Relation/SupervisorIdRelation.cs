using Mdaresna.Doamin.Models.UserManagement;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.Doamin.Models.Base.Relation
{
    public class SupervisorIdRelation : AuditBase
    {
        public Guid SupervisorId { get; set; }

        [ForeignKey(nameof(SupervisorId))]
        public User Supervisor { get; set; }
    }
}
