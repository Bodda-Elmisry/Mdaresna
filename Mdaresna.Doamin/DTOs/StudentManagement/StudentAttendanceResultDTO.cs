using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class StudentAttendanceResultDTO
    {
        public Guid Id { get; set; }
        public Guid SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public Guid ClassRoomId { get; set; }
        public string ClassRoomName { get; set; }
        public Guid StudentId { get; set; }
        public string StudentName { get; set; }
        public DateTime Date { get; set; }
        public string WeekDay { get; set; }
        public bool IsAttend { get; set; }
    }
}
