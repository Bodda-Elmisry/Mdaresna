namespace Mdaresna.DTOs.SettingsManagementDTO
{
    public class GetReportQueuesPagedDTO : GetReportQueuesDTO
    {
        public int PageNumber { get; set; } = 1;

        public int? PageSize { get; set; }
    }
}
