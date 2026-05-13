namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class StudentAbsencePermitResultDTO
    {
        public Guid Id { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public Guid ParentId { get; set; }
        public string ParentName { get; set; } = string.Empty;
        public Guid ClassRoomId { get; set; }
        public string ClassRoomName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public string? Reason { get; set; }
    }
}
