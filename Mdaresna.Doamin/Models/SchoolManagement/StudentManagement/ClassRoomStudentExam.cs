using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using System.ComponentModel.DataAnnotations.Schema;

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
