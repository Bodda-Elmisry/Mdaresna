namespace Mdaresna.Schools.Domain.Social;

public enum SocialPostStatus { Draft = 1, PendingReview = 2, Published = 3, Hidden = 4, Deleted = 5 }
public sealed class SocialPost
{
    public long Id { get; set; }
    public Guid AuthorUserId { get; set; }
    public string Text { get; set; } = "";
    public SocialPostStatus Status { get; set; } = SocialPostStatus.Draft;
    public bool CommentsEnabled { get; set; } = true;
    public int Revision { get; set; } = 1;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public DateTimeOffset? PublishedAtUtc { get; set; }
}
public sealed class SocialMedia
{
    public Guid Id { get; set; }
    public long PostId { get; set; }
    public Guid DocumentVersionId { get; set; }
    public int SortOrder { get; set; }
    public bool IsDeleted { get; set; }
}
public sealed class SocialLike
{
    public long PostId { get; set; }
    public Guid UserId { get; set; }
    public bool IsActive { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
public sealed class SocialComment
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public Guid UserId { get; set; }
    public string Text { get; set; } = "";
    public bool IsDeleted { get; set; }
    public int Revision { get; set; } = 1;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? EditedAtUtc { get; set; }
}
public sealed class SocialReport
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public long? CommentId { get; set; }
    public Guid UserId { get; set; }
    public string TargetKey { get; set; } = "post";
    public string Reason { get; set; } = "";
    public string Details { get; set; } = "";
    public string EvidenceJson { get; set; } = "";
    public DateTimeOffset CreatedAtUtc { get; set; }
    public Guid? ResolvedByUserId { get; set; }
    public DateTimeOffset? ResolvedAtUtc { get; set; }
    public string? Resolution { get; set; }
}
public sealed class SocialAudit
{
    public long Id { get; set; }
    public long PostId { get; set; }
    public Guid ActorUserId { get; set; }
    public string Action { get; set; } = "";
    public string SnapshotJson { get; set; } = "";
    public DateTimeOffset CreatedAtUtc { get; set; }
}
public sealed class SocialSettings
{
    public int Id { get; set; } = 1;
    public bool RequireApproval { get; set; }
    public int Revision { get; set; } = 1;
}
public sealed class SocialReviewer
{
    public Guid UserId { get; set; }
}

public static class SocialRules
{
    public const int MaxMedia = 10;
    public const long MaxImageBytes = 8 * 1024 * 1024;
    public const long MaxVideoBytes = 50 * 1024 * 1024;
    public static readonly string[] ReportReasons = ["privacy", "abuse", "inappropriate", "misleading", "other"];
    public static bool CanRead(SocialPost p, Guid actor, bool moderator) =>
        p.Status == SocialPostStatus.Published || moderator || p.AuthorUserId == actor && p.Status != SocialPostStatus.Deleted;
    public static bool ValidText(string? text, int max) => text is not null && text.Length <= max;
    public static (string Extension, string ContentType)? MediaType(ReadOnlySpan<byte> header)
    {
        var image = School.SchoolPresentationRules.ImageType(header.ToArray());
        if (image.HasValue) return image;
        // Only browser-compatible MP4 brands; filenames and client MIME are not trusted.
        if (header.Length >= 16 && header.Slice(4, 4).SequenceEqual("ftyp"u8) &&
            (header.Slice(8, 4).SequenceEqual("isom"u8) || header.Slice(8, 4).SequenceEqual("mp42"u8) ||
             header.Slice(8, 4).SequenceEqual("mp41"u8) || header.Slice(8, 4).SequenceEqual("avc1"u8)))
            return (".mp4", "video/mp4");
        return null;
    }
}
