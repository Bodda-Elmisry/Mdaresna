namespace Mdaresna.DTOs.UserManagementDTO
{
    public class UpdateUserDTO
    {
        public Guid Id { get; set; }
        public string? UserName { get; set; }
        public string FirstName { get; set; }
        public string? MiddelName { get; set; }
        public string LastName { get; set; }
        public DateTime? BirthDay { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? Country { get; set; }
        public string? Email { get; set; }
    }
}
