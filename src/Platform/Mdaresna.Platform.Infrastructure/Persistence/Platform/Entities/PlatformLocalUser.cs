namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;

/// <summary>
/// The Platform-owned login/profile for one central person. School and Family
/// applications own equivalent records in their own databases.
/// </summary>
public sealed class PlatformLocalUser
{
    public Guid Id { get; set; }
    public Guid PersonId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string NormalizedUserName { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string Status { get; set; } = "PendingActivation";
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public PlatformLocalCredential? Credential { get; set; }
    public PlatformLocalPasswordResetChallenge? PasswordResetChallenge { get; set; }
}

public sealed class PlatformLocalPasswordResetChallenge
{
    public Guid UserId { get; set; }
    public byte[] CodeHash { get; set; } = [];
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? ConsumedAtUtc { get; set; }
    public DateTimeOffset LastSentAtUtc { get; set; }
    public DateTimeOffset SendWindowStartUtc { get; set; }
    public int SendCount { get; set; }
    public int FailedAttempts { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public PlatformLocalUser User { get; set; } = null!;
}

public sealed class PlatformLocalCredential
{
    public Guid UserId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string HashingAlgorithm { get; set; } = string.Empty;
    public int HashingVersion { get; set; }
    public string SecurityStamp { get; set; } = string.Empty;
    public int FailedSignInCount { get; set; }
    public DateTimeOffset? LockoutEndUtc { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTimeOffset ChangedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public PlatformLocalUser User { get; set; } = null!;
}
