using System;

namespace Mdaresna.Doamin.DTOs.SchoolManagement
{
    public class SchoolPostReportsCountResultDTO
    {
        public Guid PostId { get; set; }
        public string Content { get; set; }
        public Guid PosterId { get; set; }
        public string PosterName { get; set; }
        public Guid SchoolId { get; set; }
        public string SchoolName { get; set; }
        public int ReportsCount { get; set; }
        public DateTime LastModifyDate { get; set; }
    }
}
