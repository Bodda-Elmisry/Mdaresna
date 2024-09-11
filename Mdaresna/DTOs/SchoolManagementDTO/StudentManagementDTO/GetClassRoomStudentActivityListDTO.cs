namespace Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO
{
    public class GetClassRoomStudentActivityListDTO
    {
        public Guid StudentId { get; set; }
        public Guid? ActivityId { get; set; }
        public decimal? ResultFrom { get; set; }
        public decimal? ResultTo { get; set; }
    }
}
