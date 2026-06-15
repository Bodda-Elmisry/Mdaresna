using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.Doamin.Models.Base.Relation
{
    public class ClassRoomIdRelation
    {
        public Guid ClassRoomId { get; set; }

        [ForeignKey(nameof(ClassRoomId))]
        public ClassRoom ClassRoom { get; set; }
    }
}
