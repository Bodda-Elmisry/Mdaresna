using Mdaresna.Doamin.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement
{
    public class SchoolPostImage : AuditBase
    {
        public Guid Id { get; set; }

        public Guid PostId { get; set; }

        [ForeignKey(nameof(PostId))]
        public SchoolPost Post { get; set; }

        public string ImageUrl { get; set; }
    }
}
