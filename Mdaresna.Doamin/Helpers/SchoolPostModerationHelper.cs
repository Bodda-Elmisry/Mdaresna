using Mdaresna.Doamin.Enums;
using System.Text;

namespace Mdaresna.Doamin.Helpers
{
    public sealed class SchoolPostModerationDecision
    {
        public bool AllowSubmission { get; init; }
        public SchoolPostModerationStatusEnum Status { get; init; }
        public string? ReasonCode { get; init; }
        public string Message { get; init; } = string.Empty;
    }

    public static class SchoolPostModerationHelper
    {
        private static readonly string[] BlockedFragments =
        {
            "fuck",
            "fucking",
            "shit",
            "bitch",
            "slut",
            "whore",
            "porn",
            "nude",
            "nudes",
            "xxx",
            "rape",
            "killyou",
            "killyourself",
            "suicide",
            "terrorist",
            "s3x",
            "s3xy",
            "سكس",
            "اباح",
            "عاري",
            "عاريه",
            "اغتصاب",
            "اقتلك",
            "اقتلنفسك",
            "انتحار",
            "ارهابي"
        };

        public static SchoolPostModerationDecision Evaluate(string? content, bool hasImages)
        {
            var normalizedContent = Normalize(content);

            if (ContainsBlockedFragment(normalizedContent))
            {
                return new SchoolPostModerationDecision
                {
                    AllowSubmission = false,
                    Status = SchoolPostModerationStatusEnum.Rejected,
                    ReasonCode = "blocked_terms",
                    Message = "This post includes blocked language and could not be submitted."
                };
            }

            if (hasImages)
            {
                return new SchoolPostModerationDecision
                {
                    AllowSubmission = true,
                    Status = SchoolPostModerationStatusEnum.PendingReview,
                    ReasonCode = "media_requires_review",
                    Message = "Your post was submitted for moderation and will appear after review."
                };
            }

            return new SchoolPostModerationDecision
            {
                AllowSubmission = true,
                Status = SchoolPostModerationStatusEnum.Approved,
                Message = "Post created successfully"
            };
        }

        private static bool ContainsBlockedFragment(string normalizedContent)
        {
            if (string.IsNullOrWhiteSpace(normalizedContent))
            {
                return false;
            }

            return BlockedFragments.Any(normalizedContent.Contains);
        }

        private static string Normalize(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }

            var builder = new StringBuilder(value.Length);
            foreach (var character in value.ToLowerInvariant())
            {
                builder.Append(character switch
                {
                    '0' => 'o',
                    '1' => 'i',
                    '3' => 'e',
                    '4' => 'a',
                    '5' => 's',
                    '7' => 't',
                    'أ' => 'ا',
                    'إ' => 'ا',
                    'آ' => 'ا',
                    'ة' => 'ه',
                    'ى' => 'ي',
                    _ when char.IsLetter(character) || char.IsDigit(character) => character,
                    _ => '\0'
                });
            }

            return builder.ToString().Replace("\0", string.Empty);
        }
    }
}
