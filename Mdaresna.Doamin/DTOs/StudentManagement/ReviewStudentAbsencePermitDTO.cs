using Mdaresna.Doamin.Enums;

namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class ReviewStudentAbsencePermitDTO
    {
        public Guid StudentAbsencePermitId { get; set; }
        public Guid ReviewerId { get; set; }
        public AbsencePermitStatusEnum Status { get; set; }
        public string? SupervisorNotes { get; set; }
    }
}
