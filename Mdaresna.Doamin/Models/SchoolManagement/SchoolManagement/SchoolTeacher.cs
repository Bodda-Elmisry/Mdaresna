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
    public class SchoolTeacher : AuditBase
    {
        public Guid SchoolId { get; set; }
        
        [ForeignKey(nameof(SchoolId))]
        public School School { get; set; }

        public Guid TeacherId { get; set; }

        [ForeignKey(nameof(TeacherId))]
        public User Teacher { get; set; }


    }
}
