namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class GetStudentAbsencePermitsDTO
    {
        public Guid? StudentId { get; set; }
        public Guid? ParentId { get; set; }
        public int PageNumber { get; set; } = 1;
    }
}
