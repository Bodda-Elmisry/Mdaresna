using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class StudentNoteResultDTO
    {
        public Guid Id { get; set; }
        public DateTime Date { get; set; }
        public string Notes { get; set; }
        public Guid? CourseId { get; set; }
        public string? CourseName { get; set; }
        public Guid SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public Guid ClassRoomId { get; set; }
        public string ClassRoomName { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }

    }
}
