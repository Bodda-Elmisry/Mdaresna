using Mdaresna.DTOs.Common;

namespace Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO
{
    public class SchoolIdTeacherIdCourseIdDTO : SchoolIdTeacherIdDTO
    {
        public Guid CourseId { get; set; }
    }
}
