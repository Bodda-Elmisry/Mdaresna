namespace Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement
{
    public class SchoolImage
    {
        public Guid Id { get; set; }
        public Guid SchoolId { get; set; }
        public School School { get; set; }
        public string ImagePath { get; set; }

    }
}
