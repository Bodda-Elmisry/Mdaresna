using System;

namespace Mdaresna.Doamin.DTOs.SettingsManagement
{
    public class SMSLogResultDTO
    {
        public Guid Id { get; set; }
        public Guid? SMSProviderId { get; set; }
        public string PhoneNumber { get; set; }
        public string Message { get; set; }
        public string Response { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime? CreateDate { get; set; }
    }
}
