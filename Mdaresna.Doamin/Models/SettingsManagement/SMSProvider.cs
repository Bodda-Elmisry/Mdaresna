using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.SettingsManagement
{
    public class SMSProvider
    {
        public Guid Id { get; set; }

        [MaxLength(300)]
        public string ProviderUserName { get; set; }

        [MaxLength(300)]
        public string ProviderPassword { get; set; }

        [MaxLength(300)]
        public string SenderName { get; set; }

        public int ProviderPeriority { get; set; }

        public bool Active { get; set; }
    }
}
