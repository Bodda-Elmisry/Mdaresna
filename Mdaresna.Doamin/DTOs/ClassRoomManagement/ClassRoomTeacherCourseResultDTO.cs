using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.ClassRoomManagement
{
    public class ClassRoomTeacherCourseResultDTO
    {
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public Guid ClassRoomId { get; set; }
        public string ClassRoomName { get; set; }
    }
}
