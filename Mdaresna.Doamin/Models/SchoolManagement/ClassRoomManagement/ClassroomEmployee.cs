using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement
{
    public class ClassroomEmployee : AuditBase
    {
        public Guid EmployeeId { get; set; }

        [ForeignKey(nameof(EmployeeId))]
        public User Employee { get; set; }

        public Guid ClassRoomId { get; set; }

        [ForeignKey(nameof(ClassRoomId))]
        public ClassRoom ClassRoom { get; set; }
    }
}
