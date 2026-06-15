using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.Doamin.Models.Base.Relation
{
    public class ClassRoomSupervisorCourseReltaion : ClassRoomSupervisorRelation
    {
        public Guid CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public SchoolCourse Course { get; set; }
    }
}
