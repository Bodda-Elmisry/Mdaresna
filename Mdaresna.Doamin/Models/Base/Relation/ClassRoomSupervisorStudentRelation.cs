using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.Base.Relation
{
    public class ClassRoomSupervisorStudentRelation : ClassRoomSupervisorRelation
    {
        public Guid StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; }
    }
}
