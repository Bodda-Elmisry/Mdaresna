namespace Mdaresna.Doamin.DTOs.SettingsManagement;

public class FailReportQueueCommandDTO
{
    public Guid ReportQueueId { get; set; }

    public string Reason { get; set; } = string.Empty;

    public Guid? FailedById { get; set; }

    public Guid? ReviewedById { get; set; }

    public Guid? ReviewdById { get; set; }

    public Guid? GetReviewerId()
    {
        if (FailedById.HasValue && FailedById.Value != Guid.Empty)
        {
            return FailedById.Value;
        }

        if (ReviewedById.HasValue && ReviewedById.Value != Guid.Empty)
        {
            return ReviewedById.Value;
        }

        if (ReviewdById.HasValue && ReviewdById.Value != Guid.Empty)
        {
            return ReviewdById.Value;
        }

        return null;
    }
}
