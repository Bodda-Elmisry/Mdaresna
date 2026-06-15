using Mdaresna.Doamin.Models.Base;
using System.ComponentModel.DataAnnotations;

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
