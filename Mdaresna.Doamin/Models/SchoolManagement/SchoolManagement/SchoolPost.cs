using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement
{
    public class SchoolPost : AuditBase
    {
        public Guid Id { get; set; }

        public string Content { get; set; }

        public Guid PosterId { get; set; }

        [ForeignKey(nameof(PosterId))]
        public User Poster { get; set; }

        public DateTime PostDate { get; set; }

        public Guid SchoolId { get; set; }

        [ForeignKey(nameof(SchoolId))]
        public School School { get; set; }


    }
}
