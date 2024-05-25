using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.Base
{
    public class BaseModel : AuditBase
    {
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }
    }
}
