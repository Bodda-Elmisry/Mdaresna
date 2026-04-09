namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class UpdateStudentsPayedStatusDTO
    {
        public IEnumerable<Guid>? SchoolIds { get; set; }
        public bool AllSchools { get; set; }
    }
}
