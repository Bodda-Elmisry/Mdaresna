using Mdaresna.Doamin.Models.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.Doamin.Models.UserManagement
{
    public class UserReport : AuditBase
    {
        public Guid Id { get; set; }
        public Guid ReporterUserId { get; set; }

        [ForeignKey(nameof(ReporterUserId))]
        public User ReporterUser { get; set; }

        public Guid ReportedUserId { get; set; }

        [ForeignKey(nameof(ReportedUserId))]
        public User ReportedUser { get; set; }

        public string Description { get; set; }
    }
}
