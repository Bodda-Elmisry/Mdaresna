namespace Mdaresna.DTOs.LegalPolicyDTO
{
    public class LegalPolicyAcceptanceResultDTO
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? Email { get; set; }
        public Guid PolicyVersionId { get; set; }
        public string PolicyVersion { get; set; } = string.Empty;
        public DateTime AcceptedAtUtc { get; set; }
        public string? AcceptedIpAddress { get; set; }
        public DateTime? RevokedAtUtc { get; set; }
        public Guid? RevokedByUserId { get; set; }
        public string? RevocationReason { get; set; }
    }
}
