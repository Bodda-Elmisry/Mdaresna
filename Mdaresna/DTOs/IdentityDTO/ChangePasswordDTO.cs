namespace Mdaresna.DTOs.IdentityDTO
{
    public class ChangePasswordDTO
    {
        public Guid Id { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
