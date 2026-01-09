using System;

namespace Mdaresna.Doamin.DTOs.SchoolManagement
{
    public class SchoolPostReportsFilterDTO
    {
        public string? SchoolName { get; set; }
        public int? MinReportsCount { get; set; }
        public int? MaxReportsCount { get; set; }
        public int PageNumber { get; set; } = 1;
    }
}
