using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Mdaresna.Infrastructure.Data;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Infrastructure.Hubs
{
    public class NotificationHub : Hub
    {
        private readonly AppDbContext _db;

        public NotificationHub(AppDbContext db)
        {
            _db = db;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.GetHttpContext()?.Request.Query["userId"];
            var deviceId = Context.GetHttpContext()?.Request.Query["deviceId"];

            if (!string.IsNullOrEmpty(deviceId))
            {
                var device = await _db.UserDevices.FirstOrDefaultAsync(d => d.DeviceId == deviceId.Value);
                if (device != null)
                {
                    device.SignalRConnectionId = Context.ConnectionId;
                    device.LastSeen = DateTime.UtcNow;
                    await _db.SaveChangesAsync();
                }
            }

            await base.OnConnectedAsync();
        }
    }
}
