namespace Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO
{
    public class CreateClassRoomActivityDTO
    {
        public DateTime ActivityDate { get; set; }
        public string WeekDay { get; set; }
        public string Details { get; set; }
        public Guid CourseId { get; set; }
        public Guid ClassRoomId { get; set; }
        public Guid SupervisorId { get; set; }
        public decimal Rate { get; set; }
        public IEnumerable<Guid> StudentIds { get; set; }
    }
}
