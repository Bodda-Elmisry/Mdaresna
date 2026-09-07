using Mdaresna.Doamin.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace Mdaresna.Doamin.Models.SettingsManagement
{
    public class LegalPolicyVersion : AuditBase
    {
        public Guid Id { get; set; }

        [MaxLength(50)]
        public string Version { get; set; } = string.Empty;

        [MaxLength(300)]
        public string TitleAr { get; set; } = string.Empty;

        [MaxLength(300)]
        public string TitleEn { get; set; } = string.Empty;

        public string PrivacyPolicyAr { get; set; } = string.Empty;
        public string PrivacyPolicyEn { get; set; } = string.Empty;
        public string UgcTermsAr { get; set; } = string.Empty;
        public string UgcTermsEn { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime EffectiveDateUtc { get; set; }
        public Guid? CreatedByUserId { get; set; }
    }
}
