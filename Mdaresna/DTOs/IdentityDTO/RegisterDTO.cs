using System.ComponentModel.DataAnnotations;

namespace Mdaresna.DTOs.IdentityDTO
{
    public class RegisterDTO
    {
        [MaxLength(200)]
        public string PhoneNumber { get; set; }
    }
}
