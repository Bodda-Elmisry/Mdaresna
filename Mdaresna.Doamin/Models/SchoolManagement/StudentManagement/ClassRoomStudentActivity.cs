using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
