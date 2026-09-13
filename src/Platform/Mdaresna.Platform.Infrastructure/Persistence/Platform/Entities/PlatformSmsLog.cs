namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;

/// <summary>
/// One Platform SMS delivery attempt. Recipient, message (which can contain an
/// OTP), and untrusted provider response are authenticated ciphertext only.
/// </summary>
public sealed class PlatformSmsLog
{
    public Guid Id { get; set; }
    public string SourceSystem { get; set; } = "platform";
    public string MessageTypeCode { get; set; } = "other";
    public Guid? SchoolId { get; set; }
    public Guid? SourceMessageId { get; set; }
    public Guid? ProviderId { get; set; }
    public string RecipientEncrypted { get; set; } = string.Empty;
    public string RecipientMasked { get; set; } = string.Empty;
    public string MessageEncrypted { get; set; } = string.Empty;
    public string? ResponseEncrypted { get; set; }
    public int? HttpStatusCode { get; set; }
    public string Status { get; set; } = "Pending";
    public string? FailureReason { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }

    public PlatformSmsProvider? Provider { get; set; }
}
