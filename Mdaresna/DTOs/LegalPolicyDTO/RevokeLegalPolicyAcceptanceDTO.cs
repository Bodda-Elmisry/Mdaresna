namespace Mdaresna.DTOs.LegalPolicyDTO
{
    public class RevokeLegalPolicyAcceptanceDTO
    {
        public Guid UserId { get; set; }
        public Guid? PolicyVersionId { get; set; }
        public string? Reason { get; set; }
    }
}
