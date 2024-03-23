using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement
{
    public class ClassRoomExam : ClassRoomBaseModel
    {
        public DateTime ExamDate { get; set; }

        public Guid MonthId { get; set; }

        [ForeignKey(nameof(MonthId))]
        public SchoolYearMonth Month { get; set; }

    }
}
