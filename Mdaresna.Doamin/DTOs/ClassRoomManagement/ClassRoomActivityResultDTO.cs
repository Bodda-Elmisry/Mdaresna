using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.ClassRoomManagement
{
    public class ClassRoomActivityResultDTO
    {
        public Guid Id { get; set; }
        public DateTime ActivityDate { get; set; }
        public string WeekDay { get; set; } = string.Empty;
        public string ActivityDetails { get; set; } = string.Empty;
        public Guid ClassRoomId { get; set; }
        public string ClassRoom { get; set; } = string.Empty;
        public Guid SupervisorId { get; set; }
        public string SupervisorName { get; set; } = string.Empty;
        public Guid CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public decimal Rate { get; set; }
        public int AssignedStudentsCount { get; set; }
        public int RatedStudentsCount { get; set; }
        public int TotalCount { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? LastModifyDate { get; set; }

    }
}
