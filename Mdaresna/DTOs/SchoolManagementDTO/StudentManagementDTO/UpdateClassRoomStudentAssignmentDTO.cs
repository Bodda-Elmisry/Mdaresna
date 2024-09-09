namespace Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO
{
    public class UpdateClassRoomStudentAssignmentDTO 
    {
        public Guid AssignmentId { get; set; }
        public Guid StudentId { get; set; }
        public decimal Result { get; set; }
        public bool? IsDelivered { get; set; }
        public DateTime? DeliveredDate { get; set; }
    }
}
