using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement
{
    public class SchoolImage
    {
        public Guid Id { get; set; }
        public Guid SchoolId { get; set; }
        public School School { get; set; }
        public string ImagePath { get; set; }

    }
}
