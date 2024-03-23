using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.SchoolManagement.StudentManagement
{
    public class ClassRoomStudentExam : StudentBaseModel
    {
        public Guid ExamId { get; set; }

        [ForeignKey(nameof(ExamId))]
        public ClassRoomExam Exam { get; set; }

        public decimal? TotalResult { get; set; }

        public bool IsAttend { get; set; }
    }
}
