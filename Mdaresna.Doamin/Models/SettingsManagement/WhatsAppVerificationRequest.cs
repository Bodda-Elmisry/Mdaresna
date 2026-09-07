using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace Mdaresna.Doamin.Models.SettingsManagement
{
    public class WhatsAppVerificationRequest : AuditBase
    {
        public Guid Id { get; set; }
        public Guid VerificationChallengeId { get; set; }
        public WhatsAppVerificationRequestStatusEnum Status { get; set; }
        public DateTime RequestedAtUtc { get; set; }
        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? PreparedAtUtc { get; set; }
        public Guid? PreparedByUserId { get; set; }
        public DateTime? SentConfirmedAtUtc { get; set; }
        public Guid? SentConfirmedByUserId { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
