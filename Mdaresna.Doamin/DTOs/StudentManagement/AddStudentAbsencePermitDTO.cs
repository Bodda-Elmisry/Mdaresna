namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class AddStudentAbsencePermitDTO
    {
        public Guid StudentId { get; set; }
        public Guid ParentId { get; set; }
        public DateTime Date { get; set; }
        public string? Reason { get; set; }
    }
}
