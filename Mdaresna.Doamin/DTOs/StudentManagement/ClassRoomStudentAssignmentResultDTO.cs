using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class ClassRoomStudentAssignmentResultDTO
    {
        public Guid AssignmentId { get; set; }
        public DateTime AssignmentDate { get; set; }
        public string WeekDay { get; set; }
        public string Details { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public Guid ClassRoomId { get; set; }
        public string ClassRoomName { get; set; }
        public Guid SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public decimal AssignmentRate { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public decimal Result { get; set; }
        public bool? IsDelivered { get; set; }
        public DateTime? DeliveredDate { get; set; }

    }
}
