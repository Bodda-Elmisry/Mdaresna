namespace Mdaresna.DTOs.LegalPolicyDTO
{
    public class CreateLegalPolicyDTO
    {
        public string Version { get; set; } = string.Empty;
        public string TitleAr { get; set; } = string.Empty;
        public string TitleEn { get; set; } = string.Empty;
        public string PrivacyPolicyAr { get; set; } = string.Empty;
        public string PrivacyPolicyEn { get; set; } = string.Empty;
        public string UgcTermsAr { get; set; } = string.Empty;
        public string UgcTermsEn { get; set; } = string.Empty;
        public DateTime? EffectiveDateUtc { get; set; }
    }
}
