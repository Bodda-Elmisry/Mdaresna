using System.ComponentModel.DataAnnotations;

namespace Mdaresna.DTOs.IdentityDTO
{
    public class ConfirmPhoneDTO
    {
        public Guid ChallengeId { get; set; }

        [MaxLength(200)]
        public string PhoneNumber { get; set; }

        [MaxLength(20)]
        public string Key { get; set; }
    }
}
