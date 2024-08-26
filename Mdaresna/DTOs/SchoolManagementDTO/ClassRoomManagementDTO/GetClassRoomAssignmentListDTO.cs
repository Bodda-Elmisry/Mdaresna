namespace Mdaresna.DTOs.SchoolManagementDTO.ClassRoomManagementDTO
{
    public class GetClassRoomAssignmentListDTO
    {
        //Guid? classRoomId, Guid? SupervisorId, Guid? courseId, string? details, decimal? rate, DateTime? fromdate, DateTime? todate, int pageNumber
        public Guid? classRoomId { get; set; }
        public Guid? SupervisorId { get; set; }
        public Guid? courseId { get; set; }
        public string? details { get; set; }
        public decimal? rate { get; set; }
        public DateTime? fromdate { get; set; }
        public DateTime? todate { get; set;}
        public int pageNumber { get; set; }
    }
}
