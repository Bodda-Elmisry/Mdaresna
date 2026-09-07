using Mdaresna.Doamin.Models.UserManagement;

namespace Mdaresna.DTOs.IdentityDTO
{
    public class RegisterResultDTO
    {
        public bool Regidterd { get; set; }
        public string MSG { get; set; }
        public Mdaresna.Doamin.DTOs.Identity.VerificationDispatchResultDTO? Verification { get; set; }
    }
}
