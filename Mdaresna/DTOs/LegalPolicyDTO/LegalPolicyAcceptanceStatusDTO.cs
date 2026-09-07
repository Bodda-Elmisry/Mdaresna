namespace Mdaresna.DTOs.LegalPolicyDTO
{
    public class LegalPolicyAcceptanceStatusDTO
    {
        public bool HasActivePolicy { get; set; }
        public bool Accepted { get; set; }
        public bool RequiresAcceptance => HasActivePolicy && !Accepted;
        public DateTime? AcceptedAtUtc { get; set; }
        public LegalPolicyVersionResultDTO? Policy { get; set; }
    }
}
