namespace Mdaresna.DTOs.SchoolManagementDTO.StudentManagementDTO
{
    public class CreateStudentDTO
    {
        public string FirstName { get; set; }
        public string MidelName { get; set; }
        public string LastName { get; set; }
        public string ImageUrl { get; set; }
        public bool Active { get; set; }
        public Guid SchoolId { get; set; }
        public Guid ClassRoomId { get; set; }
        public bool IsPayed { get; set; }
        public DateTime? BirthDate { get; set; }
    }
}
