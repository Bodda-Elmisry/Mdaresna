using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.ClassRoomManagement
{
    public class ClassRoomExamResultDTO
    {
        public Guid Id { get; set; }
        public DateTime ExamDate { get; set; }
        public string WeekDay { get; set; }
        public string ExamDetails { get; set; }
        public Guid ClassRoomId { get; set; }
        public string ClassRoom { get; set; }
        public Guid SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public Guid MonthId { get; set; }
        public string Month { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public decimal Rate { get; set; }
    }
}
