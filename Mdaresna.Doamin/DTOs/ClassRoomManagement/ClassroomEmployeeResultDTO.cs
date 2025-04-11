using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.ClassRoomManagement
{
    public class ClassroomEmployeeResultDTO
    {
        public Guid EmployeeId { get; set; }
        public string EmployeeName { get; set; }
        public Guid ClassRoomId { get; set; }
        public string ClassRoomName { get; set; }
    }
}
