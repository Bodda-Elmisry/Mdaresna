namespace Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO
{
    public class CreateClassRoomTeacherCourseDTO
    {
        public Guid TeacherId { get; set; }
        public Guid ClassRoomId { get; set; }
        public Guid CourseId { get; set; }
    }
}
