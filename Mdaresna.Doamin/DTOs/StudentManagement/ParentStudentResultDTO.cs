using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class ParentStudentResultDTO
    {
        public Guid StudentId { get; set; }
        public string StudentCode { get; set; }
        public string StudentName { get; set; }
        public string? ImageUrl { get; set; }
        public Guid SchoolId { get; set; }
        public string SchoolName { get; set; }
        public Guid CalssroomId { get; set; }
        public string ClassroomName { get; set; }
        public Guid GradeId { get; set; }
        public string GradeName { get; set; }
        public Guid ParentId { get; set; }
        public Guid RelationId { get; set; }
    }
}
