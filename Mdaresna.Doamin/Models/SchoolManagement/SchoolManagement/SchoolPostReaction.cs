using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.UserManagement;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement
{
    public class SchoolPostReaction : AuditBase
    {
        public Guid Id { get; set; }

        public Guid PostId { get; set; }

        [ForeignKey(nameof(PostId))]
        public SchoolPost Post { get; set; }

        public Guid UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public User User { get; set; }

        public SchoolPostReactionTypeEnum ReactionType { get; set; } =
            SchoolPostReactionTypeEnum.Like;
    }
}
