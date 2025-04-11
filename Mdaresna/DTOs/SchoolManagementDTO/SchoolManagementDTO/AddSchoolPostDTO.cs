namespace Mdaresna.DTOs.SchoolManagementDTO.SchoolManagementDTO
{
    public class AddSchoolPostDTO
    {
        public IEnumerable<IFormFile>? Images { get; set; }
        public string Content { get; set; }
        public Guid PosterId { get; set; }
        public Guid SchoolId { get; set; }
    }
}
