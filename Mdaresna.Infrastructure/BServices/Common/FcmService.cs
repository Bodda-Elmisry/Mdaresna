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
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };

            await FirebaseMessaging.DefaultInstance.SendAsync(message);
        }

        public async Task SendToMultiUsersAsync(List<string> tokens, string title, string body)
        {
            var message = new MulticastMessage
            {
                Tokens = tokens,
                Notification = new Notification
                {
                    Title = title,
                    Body = body
                }
            };

            var result = await FirebaseMessaging.DefaultInstance.SendEachForMulticastAsync(message);
            Console.WriteLine("SuccessCount = " + result.SuccessCount.ToString());
            Console.WriteLine("FailureCount = " + result.FailureCount.ToString());
        }
    }
}
