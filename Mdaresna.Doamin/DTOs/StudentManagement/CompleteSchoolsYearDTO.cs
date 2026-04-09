namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class CompleteSchoolsYearDTO
    {
        public IEnumerable<Guid>? SchoolIds { get; set; }
        public bool AllSchools { get; set; }
    }
}
