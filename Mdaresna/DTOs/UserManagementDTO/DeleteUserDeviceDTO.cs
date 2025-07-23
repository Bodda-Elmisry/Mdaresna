namespace Mdaresna.DTOs.UserManagementDTO
{
    public class DeleteUserDeviceDTO
    {
        public Guid UserId { get; set; }
        public string FcmTocken { get; set; }
    }
}
