namespace Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO
{
    public class CreateClassRoomStudentAssignmentDTO
    {
        public DateTime AssignmentDate { get; set; }
        public string WeekDay { get; set; }
        public string Details { get; set; }
        public Guid CourseId { get; set; }
        public Guid ClassRoomId { get; set; }
        public Guid SupervisorId { get; set; }
        public decimal Rate { get; set; }
        public Guid StudentId { get; set; }
        public decimal Result { get; set; }
        public bool? IsDelivered { get; set; }
        public DateTime? DeliveredDate { get; set; }
    }
}
