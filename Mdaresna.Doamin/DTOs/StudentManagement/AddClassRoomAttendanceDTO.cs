namespace Mdaresna.Doamin.DTOs.StudentManagement
{
    public class AddClassRoomAttendanceDTO
    {
        public Guid SupervisorId { get; set; }
        public Guid ClassRoomId { get; set; }
        public DateTime Date { get; set; }
        public string WeekDay { get; set; }
        public IEnumerable<StudentAttendanceDTO> StudentsAttenndaceList { get; set; }
    }
}
