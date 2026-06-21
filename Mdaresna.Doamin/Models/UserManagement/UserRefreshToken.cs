using Mdaresna.Doamin.Models.Base;
using System;

namespace Mdaresna.Doamin.Models.UserManagement
{
    public class UserRefreshToken : AuditBase
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public virtual User User { get; set; }
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsRevoked { get; set; }
    }
}
