namespace Mdaresna.Schools.Domain.School;

// Read-only projection of the Platform invoice; this is not a school cashbook entry.
public sealed class SchoolInvoiceCopy
{
    public Guid Id { get; set; }
    public Guid SchoolId { get; set; }
    public long Revision { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset SyncedAtUtc { get; set; }
    public string PayloadJson { get; set; } = "";
}
