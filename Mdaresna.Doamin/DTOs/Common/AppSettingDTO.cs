using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.Common
{
    public class AppSettingDTO
    {
        public string ImagesPath { get; set; } = "Images";
        public int? PageSize { get; set; }
        public int SmsVerificationMaxAttemptsPerWindow { get; set; } = 3;
        public int SmsVerificationWindowMinutes { get; set; } = 30;
        public int SmsVerificationMaxAttemptsPerDay { get; set; } = 6;
        public int SmsVerificationResendCooldownSeconds { get; set; } = 60;
        public int VerificationCodeExpiryMinutes { get; set; } = 10;
        public int VerificationCodeMaxAttempts { get; set; } = 5;
        public int WhatsAppVerificationRequestExpiryMinutes { get; set; } = 30;
        public string DefaultPhoneCountryCode { get; set; } = "20";
    }
}
