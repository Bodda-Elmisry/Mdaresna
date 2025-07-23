using FirebaseAdmin.Messaging;
using Mdaresna.Infrastructure.Hubs;
using Mdaresna.Repository.IBServices.Common;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Infrastructure.BServices.Common
{
    public class NotificationHubService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hub;

        public NotificationHubService(IHubContext<NotificationHub> hub)
        {
            this._hub = hub;
        }

        // token = SignalRConnectionId
        public async Task SendAsync(string token, string title, string body)
        {
            await _hub.Clients.Client(token)
                .SendAsync("ReceiveNotification", title, body);
            
        }

        public async Task SendToMultiUsersAsync(List<string> tokens, string title, string body)
        {
            await _hub.Clients.Clients(tokens)
                .SendAsync("ReceiveNotification", title, body);
        }
    }
}
