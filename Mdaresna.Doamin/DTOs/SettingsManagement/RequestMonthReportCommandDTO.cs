namespace Mdaresna.Doamin.DTOs.SettingsManagement;

public class RequestMonthReportCommandDTO
{
    public Guid SchoolId { get; set; }

    public Guid? GradeId { get; set; }

    public Guid? ClassroomId { get; set; }

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public Guid MonthId { get; set; }

    public Guid CreatedById { get; set; }

    public Guid CeratedById { get; set; }

    public Guid GetRequestedById()
    {
        return CreatedById != Guid.Empty ? CreatedById : CeratedById;
    }
}
