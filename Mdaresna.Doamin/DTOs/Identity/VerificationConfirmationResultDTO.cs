using Mdaresna.Doamin.Enums;

namespace Mdaresna.Doamin.DTOs.Identity
{
    public class VerificationConfirmationResultDTO
    {
        public bool Confirmed { get; set; }
        public Guid? UserId { get; set; }
        public VerificationPurposeEnum? Purpose { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
