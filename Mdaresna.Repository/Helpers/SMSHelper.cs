using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Doamin.Enums;
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

        public static async Task<string> SendConfirmationKey(
            SMSProvider provider,
            User user,
            string plainKey,
            VerificationPurposeEnum purpose = VerificationPurposeEnum.Registration)
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
                    BuildConfirmationMessage(plainKey, purpose)
                    );

                // Never log the provider URL because it contains credentials,
                // the recipient phone number, and the one-time code.
                Console.WriteLine("Sending SMS verification request.");
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
            return System.Security.Cryptography.RandomNumberGenerator
                .GetInt32(0, 1_000_000)
                .ToString("D6");
        }

        public static string BuildConfirmationMessage(User user, string plainKey)
        {
            return BuildConfirmationMessage(
                plainKey,
                VerificationPurposeEnum.Registration);
        }

        public static string BuildConfirmationMessage(
            string plainKey,
            VerificationPurposeEnum purpose)
        {
            var action = purpose == VerificationPurposeEnum.PasswordReset
                ? "resetting your password"
                : "creating your account";
            return $"Mdaresna verification code for {action}: {plainKey}. " +
                   "Do not share this code with anyone.";
        }

        public static bool IsSuccessfulResponse(string? response)
        {
            return !string.IsNullOrWhiteSpace(response) &&
                   !response.StartsWith(
                       "SOMETHING WENT AWRY",
                       StringComparison.OrdinalIgnoreCase);
        }
    }
}
