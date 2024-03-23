using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.Base.Relation
{
    public class ClassRoomIdRelation
    {
        public Guid ClassRoomId { get; set; }

        [ForeignKey(nameof(ClassRoomId))]
        public ClassRoom ClassRoom { get; set; }
    }
}
