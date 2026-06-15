using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using System.ComponentModel.DataAnnotations.Schema;

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
