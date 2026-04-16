namespace Mdaresna.Doamin.DTOs.UserManagement
{
    public class UserReportResultDTO
    {
        public Guid Id { get; set; }
        public Guid ReporterUserId { get; set; }
        public string ReporterUserName { get; set; }
        public string ReporterFullName { get; set; }
        public string ReporterPhoneNumber { get; set; }
        public Guid ReportedUserId { get; set; }
        public string ReportedUserName { get; set; }
        public string ReportedFullName { get; set; }
        public string ReportedUserPhoneNumber { get; set; }
        public string Report { get; set; }
    }
}
