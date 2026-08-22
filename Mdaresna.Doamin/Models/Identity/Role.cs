using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.Doamin.Models.Identity
{
    public class Role : BaseModel
    {

        public string? Description { get; set; }
        
        public bool Active { get; set; }
        
        public bool SchoolRole { get; set; }

        public bool AdminRole { get; set; }

        public Guid? SchoolId { get; set; }

        [ForeignKey(nameof(SchoolId))]
        public virtual School? School { get; set; }
    }
}
