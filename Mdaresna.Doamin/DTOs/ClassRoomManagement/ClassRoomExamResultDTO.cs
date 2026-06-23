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
        public string WeekDay { get; set; } = string.Empty;
        public string ExamDetails { get; set; } = string.Empty;
        public Guid ClassRoomId { get; set; }
        public string ClassRoom { get; set; } = string.Empty;
        public Guid SupervisorId { get; set; }
        public string SupervisorName { get; set; } = string.Empty;
        public Guid MonthId { get; set; }
        public string Month { get; set; } = string.Empty;
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public int AssignedStudentsCount { get; set; }
        public int RatedStudentsCount { get; set; }
    }
}
