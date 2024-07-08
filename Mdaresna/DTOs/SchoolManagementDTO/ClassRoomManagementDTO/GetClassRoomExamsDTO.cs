namespace Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO
{
    public class GetClassRoomExamsDTO
    {
        public IEnumerable<Guid> Months { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string WeekDay { get; set; }
        public Guid? CalssRoomId { get; set; }
        public Guid? SupervisorId { get; set; }
        public Guid? CourseId { get; set; }
        public decimal? Rate { get; set; }
    }
}
