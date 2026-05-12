using Mdaresna.Doamin.Models.Base.Relation;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.Doamin.Models.SchoolManagement.StudentManagement
{
    public class StudentNote : ClassRoomSupervisorStudentRelation
    {
        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        public string Notes { get; set; }

        public Guid? CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public SchoolCourse? Course { get; set; }
    }
}
