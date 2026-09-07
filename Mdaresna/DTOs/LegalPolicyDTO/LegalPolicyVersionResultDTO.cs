namespace Mdaresna.DTOs.LegalPolicyDTO
{
    public class LegalPolicyVersionResultDTO
    {
        public Guid Id { get; set; }
        public string Version { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string PrivacyPolicyAr { get; set; } = string.Empty;
        public string PrivacyPolicyEn { get; set; } = string.Empty;
        public string UgcTermsAr { get; set; } = string.Empty;
        public string UgcTermsEn { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime EffectiveDateUtc { get; set; }
        public DateTime? CreateDate { get; set; }
        public Guid? CreatedByUserId { get; set; }
        public int ActiveAcceptancesCount { get; set; }
    }
}
