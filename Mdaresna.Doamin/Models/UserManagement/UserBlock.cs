using Mdaresna.Doamin.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.Doamin.Models.UserManagement
{
    public class UserBlock : AuditBase
    {
        public Guid Id { get; set; }
        public Guid BlockerUserId { get; set; }

        [ForeignKey(nameof(BlockerUserId))]
        public User BlockerUser { get; set; }

        public Guid BlockedUserId { get; set; }

        [ForeignKey(nameof(BlockedUserId))]
        public User BlockedUser { get; set; }
    }
}
