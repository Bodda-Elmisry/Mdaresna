using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class ClassRoomStudentActivityResultDTO
    {
        public Guid ActivityId { get; set; }
        public DateTime ActivityDate { get; set; }
        public string WeekDay { get; set; }
        public string ActivityDetails { get; set; }
        public Guid ClassRoomId { get; set; }
        public string ClassRoom { get; set; }
        public Guid SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public decimal ActivityRate { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public decimal? Result { get; set; }
        public bool IsAttend { get; set; }
    }
}
