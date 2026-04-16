namespace Mdaresna.Doamin.DTOs.UserManagement
{
    public class UserReportsCountResultDTO
    {
        public Guid UserId { get; set; }
        public string? UserName { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public int ReportsCount { get; set; }
        public DateTime? LastModifyDate { get; set; }
    }
}
