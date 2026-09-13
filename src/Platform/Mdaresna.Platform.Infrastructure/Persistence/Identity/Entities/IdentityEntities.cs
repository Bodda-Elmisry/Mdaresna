namespace Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;

public enum AccountStatus
{
    PendingVerification = 1,
    Active = 2,
    Locked = 3,
    Disabled = 4
}

public enum LoginIdentifierType
{
    Email = 1,
    Phone = 2,
    SchoolUsername = 3,
    StudentCode = 4
}

public enum MfaMethodType
{
    Authenticator = 1,
    Email = 2,
    Sms = 3
}

public sealed class Account
{
    public Guid Id { get; set; }
    public AccountStatus Status { get; set; }
    public string? DisplayName { get; set; }
    public string? PreferredLocale { get; set; }
    public string? TimeZone { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public ICollection<LoginIdentifier> LoginIdentifiers { get; set; } = [];
    public PasswordCredential? PasswordCredential { get; set; }
    public ICollection<IdentitySession> Sessions { get; set; } = [];
    public ICollection<MfaMethod> MfaMethods { get; set; } = [];
    public AccountActivationChallenge? ActivationChallenge { get; set; }
    public AccountPasswordResetChallenge? PasswordResetChallenge { get; set; }
}

public sealed class AccountActivationChallenge
{
    public Guid AccountId { get; set; }
    public byte[] CodeHash { get; set; } = [];
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? ConsumedAtUtc { get; set; }
    public DateTimeOffset LastSentAtUtc { get; set; }
    public DateTimeOffset SendWindowStartUtc { get; set; }
    public int SendCount { get; set; }
    public int FailedAttempts { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Account Account { get; set; } = null!;
}

public sealed class AccountPasswordResetChallenge
{
    public Guid AccountId { get; set; }
    public byte[] CodeHash { get; set; } = [];
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? ConsumedAtUtc { get; set; }
    public DateTimeOffset LastSentAtUtc { get; set; }
    public DateTimeOffset SendWindowStartUtc { get; set; }
    public int SendCount { get; set; }
    public int FailedAttempts { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Account Account { get; set; } = null!;
}

public sealed class LoginIdentifier
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public LoginIdentifierType Type { get; set; }
    public string NormalizedValue { get; set; } = string.Empty;
    public string DisplayValue { get; set; } = string.Empty;
    public Guid? SchoolId { get; set; }
    public bool IsVerified { get; set; }
    public DateTimeOffset? VerifiedAtUtc { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Account Account { get; set; } = null!;
}

public sealed class PasswordCredential
{
    public Guid AccountId { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public string HashingAlgorithm { get; set; } = string.Empty;
    public int HashingVersion { get; set; }
    public string SecurityStamp { get; set; } = string.Empty;
    public int FailedSignInCount { get; set; }
    public DateTimeOffset? LockoutEndUtc { get; set; }
    public bool MustChangePassword { get; set; }
    public DateTimeOffset ChangedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Account Account { get; set; } = null!;
}

public sealed class IdentitySession
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public string RefreshTokenHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public DateTimeOffset? LastSeenAtUtc { get; set; }
    public DateTimeOffset? RevokedAtUtc { get; set; }
    public Guid? ReplacedBySessionId { get; set; }
    public string? RevocationReason { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Account Account { get; set; } = null!;
}

public sealed class MfaMethod
{
    public Guid Id { get; set; }
    public Guid AccountId { get; set; }
    public MfaMethodType Type { get; set; }
    public string? SecretReference { get; set; }
    public string? DestinationHint { get; set; }
    public bool IsEnabled { get; set; }
    public bool IsPrimary { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? LastUsedAtUtc { get; set; }
    public byte[] RowVersion { get; set; } = [];

    public Account Account { get; set; } = null!;
}

public sealed class IdentitySecurityEvent
{
    public Guid Id { get; set; }
    public Guid? AccountId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public bool Succeeded { get; set; }
    public DateTimeOffset OccurredAtUtc { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? MetadataJson { get; set; }
}

public sealed class IdentityOutboxMessage
{
    public Guid Id { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public ushort SchemaVersion { get; set; }
    public string PayloadJson { get; set; } = string.Empty;
    public DateTimeOffset OccurredAtUtc { get; set; }
    public DateTimeOffset? ProcessedAtUtc { get; set; }
    public DateTimeOffset? NextAttemptAtUtc { get; set; }
    public int AttemptCount { get; set; }
    public string? LastError { get; set; }
    public Guid CorrelationId { get; set; }
    public Guid? CausationId { get; set; }
    public string? TraceParent { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
