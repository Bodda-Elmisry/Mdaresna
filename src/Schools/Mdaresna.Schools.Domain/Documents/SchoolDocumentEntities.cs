using Mdaresna.Schools.Domain.Academics;

namespace Mdaresna.Schools.Domain.Documents;

public enum SchoolDocumentStatus { Active, Deleted }
public enum SchoolDocumentValidationStatus { Pending, Validated, Rejected }

public sealed class SchoolDocument
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public SchoolDocumentStatus Status { get; set; } = SchoolDocumentStatus.Active;
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public ICollection<SchoolDocumentVersion> Versions { get; set; } = [];
    public ICollection<TeachingPlanDocument> TeachingPlans { get; set; } = [];
}

public sealed class SchoolDocumentVersion
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public int VersionNumber { get; set; }
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string Sha256 { get; set; } = string.Empty;
    public string StorageKey { get; set; } = string.Empty;
    public SchoolDocumentValidationStatus ValidationStatus { get; set; }
    public Guid CreatedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public SchoolDocument Document { get; set; } = null!;
}

public sealed class SchoolDocumentAudit
{
    public Guid Id { get; set; }
    public Guid DocumentId { get; set; }
    public string Action { get; set; } = string.Empty;
    public Guid ActorUserId { get; set; }
    public string? Details { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public SchoolDocument Document { get; set; } = null!;
}
