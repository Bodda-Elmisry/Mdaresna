using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.SchoolManagement
{
    public class SchoolTeacherCourseResultDTO
    {
        public Guid TeacherId { get; set; }
        public string TeacherName { get; set; }
        public Guid SchoolId { get; set; }
        public string SchoolName { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
    }
}
