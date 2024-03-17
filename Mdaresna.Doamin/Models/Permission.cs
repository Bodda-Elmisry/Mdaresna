using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models
{
    public class Permission
    {
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string Name { get; set; }

        public string? Description { get; set; }
    }
}
