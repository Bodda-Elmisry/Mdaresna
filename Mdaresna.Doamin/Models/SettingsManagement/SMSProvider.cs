using Mdaresna.Doamin.Models.Base;
using System.ComponentModel.DataAnnotations;

namespace Mdaresna.Doamin.Models.SettingsManagement
{
    public class SMSProvider : AuditBase
    {
        public Guid Id { get; set; }

        [MaxLength(300)]
        public string ProviderUserName { get; set; }

        [MaxLength(300)]
        public string ProviderPassword { get; set; }

        [MaxLength(300)]
        public string SenderName { get; set; }

        public string APIUrl { get; set; }

        public int MessageCharactersLength { get; set; }

        public int ProviderPeriority { get; set; }

        public bool Active { get; set; }
    }
}
