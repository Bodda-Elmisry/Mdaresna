namespace Mdaresna.DTOs.Common
{
    public class UploadSchoolLogoDTO
    {
        public Guid SchoolId { get; set; }
        public IFormFile File { get; set; }
    }
}
