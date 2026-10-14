using Mdaresna.Schools.Domain.Academics;
namespace Mdaresna.Schools.Domain.Documents;
public sealed class BookVersionDigitalDocument
{
    public Guid BookVersionId { get; set; }
    public Guid DocumentVersionId { get; set; }
    public bool ShareWithFamily { get; set; }
    public bool AllowDownload { get; set; }
    public int PageCount { get; set; }
    public int Revision { get; set; } = 1;
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public Guid UpdatedByUserId { get; set; }
    public BookVersion BookVersion { get; set; } = null!;
    public SchoolDocumentVersion Version { get; set; } = null!;
}
