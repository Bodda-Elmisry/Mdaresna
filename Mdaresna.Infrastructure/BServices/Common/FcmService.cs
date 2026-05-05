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
            var message = new Message
            {
                Token = token,
                Data = new Dictionary<string, string>
                {
                    ["title"] = title,
                    ["body"] = body,
                },
                Notification = new Notification
                {
                    Title = title,
                    Body = body
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
            var message = new MulticastMessage
            {
                Tokens = tokens,
                Data = new Dictionary<string, string>
                {
                    ["title"] = title,
                    ["body"] = body,
                },
                Notification = new Notification
                {
                    Title = title,
                    Body = body
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
    }
}
