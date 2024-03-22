using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.SchoolManagement
{
    public class ClassRoom : BaseModel
    {
        public int maxOfStudents { get; set; }

        public Guid SupervisorId { get; set; }

        [ForeignKey(nameof(SupervisorId))]
        public User Supervisor { get; set; }

        public bool Active { get; set; }

        public string WCSUrl { get; set; }

        public Guid SchoolId { get; set; }

        [ForeignKey(nameof(SchoolId))]
        public School School { get; set; }

        public Guid LanguageId { get; set; }

        [ForeignKey(nameof(LanguageId))]
        public ClassRoomLanguage Language { get; set; }

        public Guid GradeId { get; set; }

        [ForeignKey(nameof(GradeId))]
        public SchoolGrade Grade { get; set; }
    }
}
