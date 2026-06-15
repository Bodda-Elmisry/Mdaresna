using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.Doamin.Models.Base.Relation
{
    public class CourseIdRelation
    {
        public Guid CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public SchoolCourse Course { get; set; }
    }
}
