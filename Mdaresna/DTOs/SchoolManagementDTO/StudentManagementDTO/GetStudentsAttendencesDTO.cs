namespace Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO
{
    public class GetStudentsAttendencesDTO
    {
        public Guid? StudentId { get; set; }
        public Guid? ClassRoomId { get; set; }
        public int PageNumber { get; set; }
    }
}
