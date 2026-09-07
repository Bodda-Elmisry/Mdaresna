using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace Mdaresna.Doamin.Models.SettingsManagement
{
    public class VerificationChallenge : AuditBase
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }

        [MaxLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        public VerificationPurposeEnum Purpose { get; set; }
        public VerificationChallengeStatusEnum Status { get; set; }

        [MaxLength(64)]
        public string? CodeHash { get; set; }

        public DateTime ExpiresAtUtc { get; set; }
        public DateTime? VerifiedAtUtc { get; set; }
        public DateTime? LockedAtUtc { get; set; }
        public int FailedVerificationAttempts { get; set; }

        [Timestamp]
        public byte[] RowVersion { get; set; } = Array.Empty<byte>();
    }
}
