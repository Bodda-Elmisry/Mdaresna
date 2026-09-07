using Mdaresna.Doamin.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace Mdaresna.Doamin.Models.SettingsManagement
{
    public class UserLegalPolicyAcceptance : AuditBase
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid LegalPolicyVersionId { get; set; }
        public DateTime AcceptedAtUtc { get; set; }

        [MaxLength(64)]
        public string? AcceptedIpAddress { get; set; }

        [MaxLength(500)]
        public string? AcceptedUserAgent { get; set; }

        public DateTime? RevokedAtUtc { get; set; }
        public Guid? RevokedByUserId { get; set; }

        [MaxLength(500)]
        public string? RevocationReason { get; set; }
    }
}
