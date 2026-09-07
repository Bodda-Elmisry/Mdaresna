using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace Mdaresna.Doamin.Models.SettingsManagement
{
    public class VerificationDeliveryAttempt : AuditBase
    {
        public Guid Id { get; set; }
        public Guid VerificationChallengeId { get; set; }

        [MaxLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        public VerificationPurposeEnum Purpose { get; set; }
        public VerificationDeliveryChannelEnum Channel { get; set; }
        public bool IsSuccess { get; set; }

        [MaxLength(500)]
        public string? ProviderResponse { get; set; }

        public Guid? SentByUserId { get; set; }
    }
}
