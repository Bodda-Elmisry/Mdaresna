using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class StudentParentResultDTO
    {
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public string StudentImage { get; set; }
        public Guid ParentId { get; set; }
        public string ParentName { get; set; }
        public string PhoneNumber { get; set; }
        public string ParentImage { get; set; }
        public Guid RelationId { get; set; }
        public string RelationName { get; set; }

    }
}
