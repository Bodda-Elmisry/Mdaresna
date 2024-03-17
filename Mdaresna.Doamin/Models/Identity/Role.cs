using Mdaresna.Doamin.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.Identity
{
    public class Role : BaseModel
    {

        public string? Description { get; set; }
        
        public bool Active { get; set; }
        
        public bool SchoolRole { get; set; }

        public bool AdminRole { get; set; }

        public Guid? SchoolId { get; set; }
    }
}
