namespace Mdaresna.DTOs.UserManagementDTO
{
    public class DeviceRegistrationDto
    {
        public Guid UserId { get; set; }
        public string DeviceId { get; set; }
        public string Platform { get; set; }
        public string FcmToken { get; set; }
    }
}
