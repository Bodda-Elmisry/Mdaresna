using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.ClassRoomManagement
{
    public class ClassRoomAssignmentResultDTO
    {
        public Guid Id { get; set; }
        public DateTime AssignmentDate { get; set; }
        public string WeekDay { get; set; }
        public string Details { get; set; }
        public Guid CourseId { get; set; }
        public string CourseName { get; set; }
        public Guid ClassRoomId { get; set; }
        public string ClassRoomName { get; set; }
        public Guid SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public decimal Rate { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? LastModifyDate { get; set; }
    }
}
