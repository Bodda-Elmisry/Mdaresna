using Mdaresna.Doamin.Enums;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace Mdaresna.Doamin.Helpers
{
    public static partial class SchoolContactValueValidator
    {
        public static bool TryValidate(
            ContactActionType actionType,
            string? value,
            out string errorMessage)
        {
            var trimmedValue = value?.Trim() ?? string.Empty;
            if (trimmedValue.Length == 0)
            {
                errorMessage = "Contact value is required.";
                return false;
            }

            var isValid = actionType switch
            {
                ContactActionType.Text => true,
                ContactActionType.WebUrl => IsValidWebUrl(trimmedValue),
                ContactActionType.Phone => IsValidPhone(trimmedValue),
                ContactActionType.Email => MailAddress.TryCreate(trimmedValue, out var address) &&
                                           address.Address == trimmedValue,
                ContactActionType.Map => true,
                _ => false
            };

            errorMessage = isValid
                ? string.Empty
                : actionType switch
                {
                    ContactActionType.WebUrl =>
                        "Enter a complete website URL starting with http:// or https://.",
                    ContactActionType.Phone => "Enter a valid phone number.",
                    ContactActionType.Email => "Enter a valid email address.",
                    _ => "Unsupported contact action type."
                };

            return isValid;
        }

        private static bool IsValidWebUrl(string value)
        {
            return Uri.TryCreate(value, UriKind.Absolute, out var uri) &&
                   (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) &&
                   !string.IsNullOrWhiteSpace(uri.Host);
        }

        private static bool IsValidPhone(string value)
        {
            return PhonePattern().IsMatch(value) && value.Count(char.IsDigit) >= 3;
        }

        [GeneratedRegex(@"^[0-9+().\-\s*#]+$")]
        private static partial Regex PhonePattern();
    }
}
