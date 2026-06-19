namespace Mdaresna.DTOs.SettingsManagementDTO;

public class GetStudentReportsByReportIdDTO
{
    public Guid? ReportId { get; set; }

    public Guid? ReportQueueId { get; set; }

    public Guid? GradeId { get; set; }

    public Guid? ClassRoomId { get; set; }

    public Guid GetReportQueueId()
    {
        if (ReportQueueId.HasValue && ReportQueueId.Value != Guid.Empty)
        {
            return ReportQueueId.Value;
        }

        return ReportId.GetValueOrDefault();
    }
}
