using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.ClassRoomManagement
{
    public class GetTeacherClassroomsCoursesDTO
    {
        public Guid TeacherId { get; set; }
        public Guid SchoolId { get; set; }
        public Guid? ClassroomId { get; set; }
        public Guid? CourseId { get; set; }

    }
}
