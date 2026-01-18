using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mdaresna.Repository.Helpers
{
    public class SMSHelper
    {
        private static readonly HttpClient httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(8)
        };

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

                Console.WriteLine($"SMS Url = {url}");
                response = await httpClient.GetStringAsync(url);
                return response;
            }
            catch (TaskCanceledException)
            {
                return "SOMETHING WENT AWRY! Status = Timeout";
            }
            catch (WebException we)
            {
                var message = we.InnerException?.Message ?? we.Message;
                response = string.Format("SOMETHING WENT AWRY! Status = {0} and Message = {1}", we.Status, message);
                return response;
            }
            catch (Exception ex)
            {
                return string.Format("SOMETHING WENT AWRY! Message = {0}", ex.Message);
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
