namespace Mdaresna.DTOs.Common
{
    public class UploadImageDTO
    {
        public Guid UserId { get; set; }
        public IFormFile File { get; set; }
        public bool IsStudent { get; set; }
    }
}
