namespace Mdaresna.DTOs.UserManagementDTO
{
    public class AddUserBlockDTO
    {
        public Guid BlockerUserId { get; set; }
        public Guid BlockedUserId { get; set; }
    }
}
