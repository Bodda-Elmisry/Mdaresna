namespace Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO
{
    public class GetClassRoomStudentAssignmentListDTO
    {
        public Guid StudentId { get; set; }
        public Guid? AssignementId { get; set; }
        public decimal? ResultFrom { get; set; }
        public decimal? ResultTo { get; set; }
        public bool? IsDelivered { get; set; }
        public DateTime? DeliveredDateFrom { get; set; }
        public DateTime? DeliveredDateTo { get; set; }
    }
}
