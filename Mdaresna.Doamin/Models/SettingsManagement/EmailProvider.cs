using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.SettingsManagement
{
    public class EmailProvider
    {
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string SenderEmail { get; set; }

        [MaxLength(200)]
        public string SenderPassword { get; set; }

        [MaxLength(200)]
        public string EmailDomain { get; set; }

        public int Periority { get; set; }

        public bool Active { get; set; }
    }
}
