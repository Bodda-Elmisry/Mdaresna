namespace Mdaresna.Doamin.DTOs.Identity
{
    public class WhatsAppPreparedMessageDTO
    {
        public Guid RequestId { get; set; }
        public string PhoneNumber { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string WhatsAppUrl { get; set; } = string.Empty;
    }
}
