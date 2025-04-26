using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.Base
{
    public class AuditBase
    {
        
        public DateTime? CreateDate { get; set; }
        public DateTime? LastModifyDate { get; set; }
        public bool Deleted { get; set; } = false;
    }
}
