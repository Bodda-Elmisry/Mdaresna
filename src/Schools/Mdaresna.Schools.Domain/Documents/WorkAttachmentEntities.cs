using Mdaresna.Schools.Domain.Students;
namespace Mdaresna.Schools.Domain.Documents;

public interface IWorkAttachment
{
    Guid Id { get; set; }
    Guid OwnerId { get; set; }
    Guid DocumentVersionId { get; set; }
    bool IsDeleted { get; set; }
    SchoolDocumentVersion Version { get; set; }
}

public sealed class HomeworkAttachment : IWorkAttachment
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public Guid DocumentVersionId { get; set; }
    public bool IsDeleted { get; set; }
    public HomeworkAssignment Owner { get; set; } = null!;
    public SchoolDocumentVersion Version { get; set; } = null!;
}

public sealed class ClassActivityAttachment : IWorkAttachment
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public Guid DocumentVersionId { get; set; }
    public bool IsDeleted { get; set; }
    public ClassActivity Owner { get; set; } = null!;
    public SchoolDocumentVersion Version { get; set; } = null!;
}

public sealed class LessonNoteAttachment : IWorkAttachment
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public Guid DocumentVersionId { get; set; }
    public bool IsDeleted { get; set; }
    public StudentLessonNote Owner { get; set; } = null!;
    public SchoolDocumentVersion Version { get; set; } = null!;
}

public static class WorkAttachmentRules
{
    public const int MaxFiles = 10;
    public const long MaxVideoBytes = 50 * 1024 * 1024;
    public const long MaxFileBytes = 25 * 1024 * 1024;
    public static (string Extension,string ContentType)? FileType(ReadOnlySpan<byte> bytes)
    {
        if (bytes.StartsWith("%PDF-"u8)) return (".pdf","application/pdf");
        return Mdaresna.Schools.Domain.Social.SocialRules.MediaType(bytes);
    }
    public static bool ValidSize(long size,string contentType) => size > 0 && size <= (contentType.StartsWith("video/") ? MaxVideoBytes : contentType.StartsWith("image/") ? 8 * 1024 * 1024 : MaxFileBytes);
}

