namespace Mdaresna.DTOs.IdentityDTO
{
    public class LoginDTO
    {
        public string? LoginIdentifier { get; set; }

        // Kept temporarily so older app versions can continue signing in.
        public string? PhoneNumber { get; set; }

        public string Password { get; set; }
        public Guid? SchoolId { get; set; }
    }
}
