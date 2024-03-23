using Mdaresna.Doamin.Models.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.SchoolManagement.StudentManagement
{
    public class StudentAttendance : StudentFullRelationsBaseModel
    {
        public Guid Id { get; set; }

        public DateTime Date { get; set; }

        [MaxLength(25)]
        public string WeekDay { get; set; }

        public bool IsAttend { get; set; }


    }
}
