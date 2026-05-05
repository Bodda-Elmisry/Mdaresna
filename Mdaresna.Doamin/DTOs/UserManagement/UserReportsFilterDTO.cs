using System;

namespace Mdaresna.Doamin.DTOs.UserManagement
{
    public class UserReportsFilterDTO
    {
        public Guid? SchoolId { get; set; }
        public string? UserName { get; set; }
        public int? MinReportsCount { get; set; }
        public int? MaxReportsCount { get; set; }
        public int PageNumber { get; set; } = 1;
    }
}
