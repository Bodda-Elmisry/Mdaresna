using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.Models.SchoolManagement.StudentManagement
{
    public class StudentExamRate : StudentBaseModel
    {
        public Guid ExamId { get; set; }

        [ForeignKey(nameof(ExamId))]
        public ClassRoomExam Exam { get; set; }

        public Guid RateHeaderId { get; set; }

        [ForeignKey(nameof(RateHeaderId))]
        public SchoolExamRateHeader RateHeader { get; set; }

        public decimal? Result { get; set; }
    }
}
