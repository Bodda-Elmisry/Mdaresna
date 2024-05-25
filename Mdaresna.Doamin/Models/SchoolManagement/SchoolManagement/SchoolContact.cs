using Mdaresna.Doamin.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement
{
    public class SchoolContact : AuditBase
    {
        public Guid Id { get; set; }

        [MaxLength(300)]
        public string Value { get; set; }

        public Guid SchoolId { get; set; }

        [ForeignKey(nameof(SchoolId))]
        public School School { get; set; }

        public Guid ContactTypeId { get; set; }

        [ForeignKey(nameof(ContactTypeId))]
        public SchoolContactType ContactType { get; set; }
    }
}
