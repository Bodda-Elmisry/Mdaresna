using Mdaresna.Doamin.Enums;

namespace Mdaresna.DTOs.IdentityDTO
{
    public class WhatsAppVerificationRequestResultDTO
    {
        public Guid Id { get; set; }
        public Guid ChallengeId { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public VerificationPurposeEnum Purpose { get; set; }
        public WhatsAppVerificationRequestStatusEnum Status { get; set; }
        public DateTime RequestedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? PreparedAtUtc { get; set; }
        public Guid? PreparedByUserId { get; set; }
        public DateTime? SentConfirmedAtUtc { get; set; }
        public Guid? SentConfirmedByUserId { get; set; }
    }
}
