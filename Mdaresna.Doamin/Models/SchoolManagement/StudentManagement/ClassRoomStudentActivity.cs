using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.Doamin.Models.SchoolManagement.StudentManagement
{
    public class ClassRoomStudentActivity : StudentBaseModel
    {
        public Guid ActivityId { get; set; }

        [ForeignKey(nameof(ActivityId))]
        public ClassRoomActivity Activity { get; set; }

        public decimal? Result { get; set; }

        public bool IsAttend { get; set; }
    }
}
