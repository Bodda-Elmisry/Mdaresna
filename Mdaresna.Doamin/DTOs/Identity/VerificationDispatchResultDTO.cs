using Mdaresna.Doamin.Enums;

namespace Mdaresna.Doamin.DTOs.Identity
{
    public class VerificationDispatchResultDTO
    {
        public Guid ChallengeId { get; set; }
        public Guid UserId { get; set; }
        public VerificationPurposeEnum Purpose { get; set; }
        public bool SmsSent { get; set; }
        public int SmsAttemptsUsed { get; set; }
        public int SmsAttemptsRemaining { get; set; }
        public bool CanResendSms { get; set; }
        public bool CanRequestWhatsApp { get; set; }
        public DateTime? ResendAvailableAtUtc { get; set; }
        public DateTime CodeExpiresAtUtc { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
