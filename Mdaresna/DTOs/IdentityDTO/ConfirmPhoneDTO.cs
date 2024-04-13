using System.ComponentModel.DataAnnotations;

namespace Mdaresna.DTOs.IdentityDTO
{
    public class ConfirmPhoneDTO
    {
        [MaxLength(200)]
        public string PhoneNumber { get; set; }

        [MaxLength(20)]
        public string Key { get; set; }
    }
}
