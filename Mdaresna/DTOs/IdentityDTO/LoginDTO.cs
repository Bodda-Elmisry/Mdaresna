namespace Mdaresna.DTOs.IdentityDTO
{
    public class LoginDTO
    {
        public string PhoneNumber { get; set; }

        public string Password { get; set; }
        public Guid? SchoolId { get; set; }
    }
}
