using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class StudentResultDTO
    {
        public Guid Id { get; set; }

        public string Code { get; set; }

        public string FirstName { get; set; }

        public string MiddelName { get; set; }

        public string LastName { get; set; }

        public string ImageUrl { get; set; }

        public bool Active { get; set; }

        public Guid SchoolId { get; set; }

        public string SchoolName { get; set; }

        public Guid ClassRoomId { get; set; }

        public string ClassRoomName { get; set; }

        public bool IsPayed { get; set; }

        public DateTime? BirthDate { get; set; }
    }
}
