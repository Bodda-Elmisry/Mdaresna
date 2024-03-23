using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Mdaresna.Doamin.Models.UserManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.Base
{
    public class ClassRoomBaseModel
    {
        public Guid Id { get; set; }

        [MaxLength(25)]
        public string WeekDay { get; set; }

        public string Details { get; set; }

        public Guid ClassRoomId { get; set; }

        [ForeignKey(nameof(ClassRoomId))]
        public ClassRoom ClassRoom { get; set; }

        public Guid SupervisorId { get; set; }

        [ForeignKey(nameof(SupervisorId))]
        public User Supervisor { get; set; }

        public Guid CourseId { get; set; }

        [ForeignKey(nameof(CourseId))]
        public SchoolCourse Course { get; set; }

        public decimal Rate { get; set; }
    }
}
