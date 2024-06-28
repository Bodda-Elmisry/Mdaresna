using Mdaresna.Doamin.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.SchoolManagement
{
    public class SchoolTeacherCourseInitialDTO
    {
        public IEnumerable<DropDownDTO> SchoolTeachers { get; set; }
        public IEnumerable<DropDownDTO> SchoolCourses { get; set; }
    }
}
