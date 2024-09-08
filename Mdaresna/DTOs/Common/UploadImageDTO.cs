namespace Mdaresna.DTOs.Common
{
    public class UploadImageDTO
    {
        public Guid UserId { get; set; }
        public IFormFile File { get; set; }
        //public bool IsStudent { get; set; }
        /*1 = student, 2 = teacher, 3 = school personal image*/
        public int Type { get; set; }
    }
}
