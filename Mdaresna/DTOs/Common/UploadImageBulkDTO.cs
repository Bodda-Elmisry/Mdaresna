namespace Mdaresna.DTOs.Common
{
    public class UploadImageBulkDTO
    {
        public Guid UserId { get; set; }
        public IEnumerable<IFormFile> Files { get; set; }
        //public bool IsStudent { get; set; }
        /*1 = student, 2 = teacher, 3 = school personal image*/
        public int Type { get; set; }
    }
}
