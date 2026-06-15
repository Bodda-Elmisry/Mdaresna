using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.UserManagement;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.Doamin.Models.SchoolManagement.StudentManagement
{
    public class StudentParent : StudentBaseModel
    {
        public Guid ParentId { get; set; }

        [ForeignKey(nameof(ParentId))]
        public User Parent { get; set; }

        public Guid RelationId { get; set; }

        [ForeignKey(nameof(RelationId))]
        public RelationType Relation { get; set; }
    }
}
