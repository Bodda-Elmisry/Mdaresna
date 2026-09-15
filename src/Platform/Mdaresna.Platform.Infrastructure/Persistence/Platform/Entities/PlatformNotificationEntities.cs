using Mdaresna.Platform.Domain.Access;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;

public sealed class PlatformUserDevice
{
    public Guid Id { get; set; }
    public IdentityAccountId AccountId { get; set; }
    public string InstallationId { get; set; } = string.Empty;
    public string FcmToken { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string LanguageCode { get; set; } = "ar";
    public string? DeviceName { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public DateTimeOffset LastSeenAtUtc { get; set; }
}

public sealed class PlatformNotification
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string TitleAr { get; set; } = string.Empty;
    public string TitleEn { get; set; } = string.Empty;
    public string BodyAr { get; set; } = string.Empty;
    public string BodyEn { get; set; } = string.Empty;
    public string? ActionUrl { get; set; }
    public string DataJson { get; set; } = "{}";
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ExpiresAtUtc { get; set; }
}

public sealed class PlatformNotificationRecipient
{
    public Guid NotificationId { get; set; }
    public IdentityAccountId AccountId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? ReadAtUtc { get; set; }
}

public sealed class PlatformNotificationDelivery
{
    public Guid Id { get; set; }
    public Guid NotificationId { get; set; }
    public Guid DeviceId { get; set; }
    public string FcmTokenSnapshot { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public int AttemptCount { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? NextAttemptAtUtc { get; set; }
    public DateTimeOffset? SentAtUtc { get; set; }
    public string? LastError { get; set; }
}
