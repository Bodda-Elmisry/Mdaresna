using Mdaresna.Doamin.Models.Base;
using System;
using System.ComponentModel.DataAnnotations;

namespace Mdaresna.Doamin.Models.SettingsManagement
{
    public class SMSLog : AuditBase
    {
        public Guid Id { get; set; }

        public Guid? SMSProviderId { get; set; }

        [MaxLength(50)]
        public string PhoneNumber { get; set; }

        public string Message { get; set; }

        public string Response { get; set; }

        public bool IsSuccess { get; set; }

        public SMSProvider? SMSProvider { get; set; }
    }
}
