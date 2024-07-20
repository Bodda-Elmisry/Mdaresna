using Mdaresna.Doamin.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.ClassRoomManagement
{
    public class CreateClassRoomExamInitialDataDTO
    {
        public IEnumerable<DropDownDTO> Supervisors { get; set; }
        public IEnumerable<DropDownDTO> ClassRooms { get; set; }
        public IEnumerable<DropDownDTO> Courses { get; set; }
        public IEnumerable<DropDownDTO> CoursesMonthes { get; set; }
    }
}
