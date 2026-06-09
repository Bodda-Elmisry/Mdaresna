namespace Mdaresna.Doamin.DTOs.SettingsManagement;

public class PublishReportQueueCommandDTO
{
    public Guid ReportQueueId { get; set; }

    public Guid? PublishedById { get; set; }

    public Guid? ReviewedById { get; set; }

    public Guid? ReviewdById { get; set; }

    public Guid? GetPublisherId()
    {
        if (PublishedById.HasValue && PublishedById.Value != Guid.Empty)
        {
            return PublishedById.Value;
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
