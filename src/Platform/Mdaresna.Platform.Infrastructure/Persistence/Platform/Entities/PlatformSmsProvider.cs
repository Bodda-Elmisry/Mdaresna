namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;

/// <summary>
/// Platform-owned SMS gateway configuration. The credential is stored only as
/// authenticated ciphertext; it must never be returned by a read API.
/// </summary>
public sealed class PlatformSmsProvider
{
    public Guid Id { get; set; }
    public string ProviderUserName { get; set; } = string.Empty;
    public string EncryptedPassword { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string ApiUrlTemplate { get; set; } = string.Empty;
    public int MessageCharactersLength { get; set; }
    public int Priority { get; set; }
    public bool IsActive { get; set; }
    public bool IsDeleted { get; set; }
    public string SuccessResponsePrefix { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
