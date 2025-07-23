using Mdaresna.Doamin.Models.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.UserManagement
{
    public class UserDevice : AuditBase
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; }
        public string DeviceId { get; set; }
        public string Platform { get; set; } // android, ios, web, desktop
        public string FcmToken { get; set; }
        public string? SignalRConnectionId { get; set; }
        public DateTime LastSeen { get; set; }
    }
}
