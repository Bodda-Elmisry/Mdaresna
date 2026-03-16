using System;

namespace Mdaresna.DTOs.SettingsManagementDTO
{
    public class GetSMSLogsDTO
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? PhoneNumber { get; set; }
        public Guid? SMSProviderId { get; set; }
        public bool? IsSuccess { get; set; }
        public string? Message { get; set; }
        public string? Response { get; set; }
        public int PageNumber { get; set; }
    }
}
