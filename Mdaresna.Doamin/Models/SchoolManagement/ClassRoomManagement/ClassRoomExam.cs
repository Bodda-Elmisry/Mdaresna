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
    public class ClassRoomExam
    {
        public Guid Id { get; set; }

        public DateTime ExamDate { get; set; }

        public string WeekDay { get; set; }

        public string Details { get; set; }

        public Guid ClassRoomId { get; set; }

        [ForeignKey(nameof(ClassRoomId))]
        public ClassRoom ClassRoom { get; set; }

        public Guid SupervisorId { get; set; }

        [ForeignKey(nameof(SupervisorId))]
        public User Supervisor { get; set; }

        public Guid MonthId { get; set; }

        [ForeignKey(nameof(MonthId))]
        public SchoolYearMonth Month { get; set; }

        public Guid CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public SchoolCourse Course { get; set; }

        public decimal Rate { get; set; }
    }
}
