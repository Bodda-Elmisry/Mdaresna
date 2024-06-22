using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.SchoolManagement
{
    public class SchoolContactResultDTO
    {
        public Guid Id { get; set; }
        public Guid SchoolId { get; set; }
        public Guid ContactTypeId { get; set; }
        public string SchoolContactValue { get; set; }
        public string TypeName { get; set; }
        public string TypeIcon { get; set; }
        public string TypeDescription { get; set; }
    }
}
