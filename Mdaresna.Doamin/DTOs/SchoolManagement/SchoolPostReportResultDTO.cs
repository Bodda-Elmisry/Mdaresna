using System;

namespace Mdaresna.Doamin.DTOs.SchoolManagement
{
    public class SchoolPostReportResultDTO
    {
        public Guid Id { get; set; }
        public Guid SchoolId { get; set; }
        public string SchoolName { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; }
        public string UserFullName { get; set; }
        public string UserPhoneNumber { get; set; }
        public Guid PostId { get; set; }
        public string Report { get; set; }
    }
}
