using Mdaresna.Doamin.Models.Base;
using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Mdaresna.Doamin.Models.UserManagement;
using Mdaresna.Doamin.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Mdaresna.Doamin.Models.SchoolManagement.StudentManagement
{
    public class StudentAbsencePermit : AuditBase
    {
        public Guid Id { get; set; }

        public Guid StudentId { get; set; }

        [ForeignKey(nameof(StudentId))]
        public Student Student { get; set; }

        public Guid ParentId { get; set; }

        [ForeignKey(nameof(ParentId))]
        public User Parent { get; set; }

        public Guid ClassRoomId { get; set; }

        [ForeignKey(nameof(ClassRoomId))]
        public ClassRoom ClassRoom { get; set; }

        public DateTime Date { get; set; }

        [MaxLength(1000)]
        public string? Reason { get; set; }

        public AbsencePermitStatusEnum Status { get; set; } = AbsencePermitStatusEnum.Pending;

        public Guid? ReviewedById { get; set; }

        [ForeignKey(nameof(ReviewedById))]
        public User? ReviewedBy { get; set; }

        [MaxLength(1000)]
        public string? SupervisorNotes { get; set; }
    }
}
