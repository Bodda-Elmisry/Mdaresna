using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.Base.Relation
{
    public class ClassRoomSupervisorCourseReltaion : ClassRoomSupervisorRelation
    {
        public Guid CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public SchoolCourse Course { get; set; }
    }
}
