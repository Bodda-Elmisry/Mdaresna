using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.AdminManagement;
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
    public class ClassRoomResultDTO
    {
        public Guid Id { get; set; }

        public string Name { get; set; }
        public int maxOfStudents { get; set; }
        public Guid SupervisorId { get; set; }
        public string SupervisorName { get; set; }
        public bool Active { get; set; }
        public string WCSUrl { get; set; }
        public Guid SchoolId { get; set; }
        public string SchoolName { get; set; }
        public Guid LanguageId { get; set; }
        public string LanguageName { get; set; }
        public Guid GradeId { get; set; }
        public string Gradename { get; set; }
        public ClassRoomGenderEnum Gender { get; set; }
    }
}
