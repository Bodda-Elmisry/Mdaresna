using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Mdaresna.Repository.IBServices.Common;
using Microsoft.AspNetCore.Builder.Extensions;

namespace Mdaresna.Infrastructure.BServices.Common
{
    public class FcmService : INotificationService
    {
        private const string DefaultAndroidChannelId = "mdaresna_default";

        public FcmService()
        {
            if (FirebaseApp.DefaultInstance == null)
            {
                FirebaseApp.Create(new AppOptions
                {
                    Credential = GoogleCredential.FromFile("firebase-service-account.json")
                });
            }
        }

        public async Task SendAsync(string token, string title, string body)
        {
            var payload = CreateNotificationPayload(title, body);
            var message = new Message
            {
                Token = token,
                Data = CreateDataPayload(title, payload),
                Notification = new Notification
                {
                    Title = title,
                    Body = payload.DisplayBody
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = DefaultAndroidChannelId,
                        Sound = "default",
                    }
                }
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }

        public async Task SendToMultiUsersAsync(List<string> tokens, string title, string body)
        {
            var payload = CreateNotificationPayload(title, body);
            var message = new MulticastMessage
            {
                Tokens = tokens,
                Data = CreateDataPayload(title, payload),
                Notification = new Notification
                {
                    Title = title,
                    Body = payload.DisplayBody
                },
                Android = new AndroidConfig
                {
                    Priority = Priority.High,
                    Notification = new AndroidNotification
                    {
                        ChannelId = DefaultAndroidChannelId,
                        Sound = "default",
                    }
                }
            };

            var result = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
            Console.WriteLine("SuccessCount = " + result.SuccessCount.ToString());
            Console.WriteLine("FailureCount = " + result.FailureCount.ToString());

            for (var i = 0; i < result.Responses.Count; i++)
            {
                if (!result.Responses[i].IsSuccess)
                {
                    Console.WriteLine(
                        $"FCM send failed for token {tokens[i]}: {result.Responses[i].Exception?.Message}");
                }
            }
        }

        private static Dictionary<string, string> CreateDataPayload(
            string title,
            NotificationPayload payload)
        {
            return new Dictionary<string, string>
            {
                ["title"] = title,
                ["body"] = payload.DisplayBody,
                ["rawBody"] = payload.RawBody,
                ["schoolId"] = payload.SchoolId,
                ["schoolBalance"] = payload.SchoolBalance,
            };
        }

        private static NotificationPayload CreateNotificationPayload(string title, string body)
        {
            var payload = new NotificationPayload
            {
                DisplayBody = body,
                RawBody = body,
            };

            if (string.IsNullOrWhiteSpace(body) || !body.Contains('|'))
            {
                return payload;
            }

            var bodyParts = body.Split('|');
            if (bodyParts.Length < 2)
            {
                return payload;
            }

            var schoolIdCandidate = bodyParts[^1].Trim();
            if (!Guid.TryParse(schoolIdCandidate, out _))
            {
                return payload;
            }

            payload.SchoolId = schoolIdCandidate;
            var hasBalance =
                title.Contains("Units Changed", StringComparison.OrdinalIgnoreCase) &&
                bodyParts.Length > 2;

            if (hasBalance)
            {
                payload.SchoolBalance = bodyParts[^2].Trim();
                payload.DisplayBody = string.Join("|", bodyParts.Take(bodyParts.Length - 2)).Trim();
            }
            else
            {
                payload.DisplayBody = string.Join("|", bodyParts.Take(bodyParts.Length - 1)).Trim();
            }

            if (string.IsNullOrWhiteSpace(payload.DisplayBody))
            {
                payload.DisplayBody = body;
            }

            return payload;
        }

        private sealed class NotificationPayload
        {
            public string DisplayBody { get; set; } = string.Empty;
            public string RawBody { get; set; } = string.Empty;
            public string SchoolId { get; set; } = string.Empty;
            public string SchoolBalance { get; set; } = string.Empty;
        }
    }
}
