using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class ClassRoomStudentExamResultDTO
    {
        public Guid ExamId { get; set; }
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
        public decimal ExamRate { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public decimal? TotalResults { get; set; }
        public bool IsAttend { get; set; }
    }
}
