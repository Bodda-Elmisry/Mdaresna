using Mdaresna.Doamin.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.ClassRoomManagement
{
    public class ClassRoomTeacherCourseInitialDTO
    {
        public IEnumerable<DropDownDTO> Courses { get; set; }
        public IEnumerable<DropDownDTO> ClassRooms { get; set; }
    }
}
