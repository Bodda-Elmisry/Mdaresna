using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.Doamin.Models.Base.Relation
{
    public class ClassRoomSupervisorStudentRelation : ClassRoomSupervisorRelation
    {
        public Guid StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; }
    }
}
