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
    public class ClassRoomStudentAssignment : StudentBaseModel
    {
        public Guid AssignmentId { get; set; }

        [ForeignKey(nameof(AssignmentId))]
        public ClassRoomAssignment Assignment { get; set; }

        public decimal Result { get; set; }

        public bool? IsDelivered { get; set; }

        public DateTime? DeliveredDate { get; set; }
    }
}
