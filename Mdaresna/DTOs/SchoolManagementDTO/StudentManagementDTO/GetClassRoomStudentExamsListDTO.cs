namespace Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO
{
    public class GetClassRoomStudentExamsListDTO
    {
        public Guid StudentId { get; set; }
        public decimal? TotalResultFrom { get; set; }
        public decimal? TotalResultTo { get; set; }
        public bool? IsAttend { get; set; }
        public int pageNumber { get; set; }

    }
}
