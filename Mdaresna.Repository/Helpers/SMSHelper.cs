using Azure;
using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Repository.Helpers
{
    public class SMSHelper
    {

        public static async Task<string> SendConfirmationKey(SMSProvider provider, User user)
        {
            string response = string.Empty;
            try
            {

                var url = string.Format(
                    provider.APIUrl,
                    provider.ProviderUserName,
                    provider.ProviderPassword,
                    provider.SenderName,
                    user.PhoneNumber,
                    string.Format("Welcome to Mdaresna\n Your Key: {0}", user.PhoneConfirmationCode)
                    );

                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;

                HttpWebResponse resp = request.GetResponse() as HttpWebResponse;
                StreamReader sr = new StreamReader(resp.GetResponseStream());
                response = sr.ReadToEnd();
                sr.Close();

                return response;
            }
            catch(WebException we)
            {
                response = string.Format("SOMETHING WENT AWRY! STatus = {0} and Message = {1}", we.Status, we.InnerException.Message);
                return response;
            }
        }

        public static string GenerateConfirmationKey()
        {
            string NumericChars = "0123456789";

            var _random = new Random();
            int length = 5;
            string key = string.Empty;
            while (length > 0)
            {
                key += NumericChars[_random.Next(NumericChars.Length)];
                length--;
            }

            return key;
        }
    }
}
