using System.Text.Json;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Messaging;

public sealed record PlatformDeviceRegistration(
    string InstallationId,
    string FcmToken,
    string Platform,
    string LanguageCode,
    string? DeviceName);

public sealed record PlatformNotificationInput(
    string Type,
    string TitleAr,
    string TitleEn,
    string BodyAr,
    string BodyEn,
    string? ActionUrl = null,
    IReadOnlyDictionary<string, string>? Data = null,
    DateTimeOffset? ExpiresAtUtc = null);

public sealed record PlatformNotificationItem(
    Guid Id,
    string Type,
    string Title,
    string Body,
    string? ActionUrl,
    IReadOnlyDictionary<string, string> Data,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? ReadAtUtc);

public sealed record PlatformNotificationPage(
    IReadOnlyList<PlatformNotificationItem> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int UnreadCount);

public interface IPlatformNotificationService
{
    Task RegisterDeviceAsync(Guid accountId, PlatformDeviceRegistration registration,
        CancellationToken cancellationToken = default);
    Task RemoveDeviceAsync(Guid accountId, string installationId,
        CancellationToken cancellationToken = default);
    Task<PlatformNotificationPage> ListAsync(Guid accountId, string languageCode,
        int pageNumber, int pageSize, CancellationToken cancellationToken = default);
    Task<int> GetUnreadCountAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task MarkReadAsync(Guid accountId, Guid notificationId,
        CancellationToken cancellationToken = default);
    Task MarkAllReadAsync(Guid accountId, CancellationToken cancellationToken = default);
    Task QueueAsync(IEnumerable<Guid> accountIds, PlatformNotificationInput input,
        CancellationToken cancellationToken = default);
}

public sealed class PlatformNotificationService(PlatformDbContext db) : IPlatformNotificationService
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task RegisterDeviceAsync(Guid accountId, PlatformDeviceRegistration registration,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty) throw new ArgumentException("Account ID is required.", nameof(accountId));
        var installationId = Required(registration.InstallationId, 128, nameof(registration.InstallationId));
        var token = Required(registration.FcmToken, 2048, nameof(registration.FcmToken));
        var platform = registration.Platform.Trim().ToLowerInvariant();
        if (platform is not ("android" or "ios" or "web"))
            throw new ArgumentException("Platform must be android, ios, or web.", nameof(registration));
        var language = registration.LanguageCode.Trim().ToLowerInvariant();
        if (language is not ("ar" or "en"))
            throw new ArgumentException("Language must be ar or en.", nameof(registration));
        var deviceName = string.IsNullOrWhiteSpace(registration.DeviceName)
            ? null
            : Required(registration.DeviceName, 200, nameof(registration.DeviceName));
        var account = IdentityAccountId.From(accountId);
        var now = DateTimeOffset.UtcNow;

        var tokenOwners = await db.UserDevices
            .Where(x => x.FcmToken == token &&
                        (x.AccountId != account || x.InstallationId != installationId))
            .ToArrayAsync(cancellationToken);
        if (tokenOwners.Length > 0) db.UserDevices.RemoveRange(tokenOwners);

        var device = await db.UserDevices.SingleOrDefaultAsync(
            x => x.AccountId == account && x.InstallationId == installationId,
            cancellationToken);
        if (device is null)
        {
            db.UserDevices.Add(new PlatformUserDevice
            {
                Id = Guid.NewGuid(), AccountId = account, InstallationId = installationId,
                FcmToken = token, Platform = platform, LanguageCode = language,
                DeviceName = deviceName, CreatedAtUtc = now, UpdatedAtUtc = now, LastSeenAtUtc = now
            });
        }
        else
        {
            device.FcmToken = token;
            device.Platform = platform;
            device.LanguageCode = language;
            device.DeviceName = deviceName;
            device.UpdatedAtUtc = now;
            device.LastSeenAtUtc = now;
        }
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveDeviceAsync(Guid accountId, string installationId,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty) throw new ArgumentException("Account ID is required.", nameof(accountId));
        var normalized = Required(installationId, 128, nameof(installationId));
        var account = IdentityAccountId.From(accountId);
        var device = await db.UserDevices.SingleOrDefaultAsync(
            x => x.AccountId == account && x.InstallationId == normalized,
            cancellationToken);
        if (device is null) return;
        db.UserDevices.Remove(device);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task<PlatformNotificationPage> ListAsync(Guid accountId, string languageCode,
        int pageNumber, int pageSize, CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty) throw new ArgumentException("Account ID is required.", nameof(accountId));
        pageNumber = Math.Max(1, pageNumber);
        pageSize = Math.Clamp(pageSize, 1, 50);
        var account = IdentityAccountId.From(accountId);
        var now = DateTimeOffset.UtcNow;
        var query =
            from recipient in db.NotificationRecipients.AsNoTracking()
            join notification in db.Notifications.AsNoTracking()
                on recipient.NotificationId equals notification.Id
            where recipient.AccountId == account &&
                  (notification.ExpiresAtUtc == null || notification.ExpiresAtUtc > now)
            select new { recipient, notification };
        var total = await query.CountAsync(cancellationToken);
        var unread = await query.CountAsync(x => x.recipient.ReadAtUtc == null, cancellationToken);
        var rows = await query.OrderByDescending(x => x.notification.CreatedAtUtc)
            .ThenByDescending(x => x.notification.Id)
            .Skip((pageNumber - 1) * pageSize).Take(pageSize)
            .ToArrayAsync(cancellationToken);
        var arabic = string.Equals(languageCode, "ar", StringComparison.OrdinalIgnoreCase);
        return new PlatformNotificationPage(rows.Select(x => new PlatformNotificationItem(
            x.notification.Id, x.notification.Type,
            arabic ? x.notification.TitleAr : x.notification.TitleEn,
            arabic ? x.notification.BodyAr : x.notification.BodyEn,
            x.notification.ActionUrl, ParseData(x.notification.DataJson),
            x.notification.CreatedAtUtc, x.recipient.ReadAtUtc)).ToArray(),
            pageNumber, pageSize, total, unread);
    }

    public Task<int> GetUnreadCountAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty) throw new ArgumentException("Account ID is required.", nameof(accountId));
        var account = IdentityAccountId.From(accountId);
        var now = DateTimeOffset.UtcNow;
        return (from recipient in db.NotificationRecipients.AsNoTracking()
                join notification in db.Notifications.AsNoTracking()
                    on recipient.NotificationId equals notification.Id
                where recipient.AccountId == account && recipient.ReadAtUtc == null &&
                      (notification.ExpiresAtUtc == null || notification.ExpiresAtUtc > now)
                select recipient).CountAsync(cancellationToken);
    }

    public async Task MarkReadAsync(Guid accountId, Guid notificationId,
        CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty || notificationId == Guid.Empty)
            throw new ArgumentException("Account and notification IDs are required.");
        var account = IdentityAccountId.From(accountId);
        var recipient = await db.NotificationRecipients.SingleOrDefaultAsync(
            x => x.NotificationId == notificationId && x.AccountId == account,
            cancellationToken) ?? throw new PlatformResourceNotFoundException(
                "notification.not_found", "The notification was not found.");
        if (recipient.ReadAtUtc is not null) return;
        recipient.ReadAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkAllReadAsync(Guid accountId, CancellationToken cancellationToken = default)
    {
        if (accountId == Guid.Empty) throw new ArgumentException("Account ID is required.", nameof(accountId));
        var account = IdentityAccountId.From(accountId);
        var unread = await db.NotificationRecipients
            .Where(x => x.AccountId == account && x.ReadAtUtc == null)
            .ToArrayAsync(cancellationToken);
        if (unread.Length == 0) return;
        var now = DateTimeOffset.UtcNow;
        foreach (var item in unread) item.ReadAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task QueueAsync(IEnumerable<Guid> accountIds, PlatformNotificationInput input,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(accountIds);
        ArgumentNullException.ThrowIfNull(input);
        var recipients = accountIds.Where(x => x != Guid.Empty).Distinct().ToArray();
        if (recipients.Length == 0) return;
        var now = DateTimeOffset.UtcNow;
        if (input.ExpiresAtUtc is { } expires && expires <= now)
            throw new ArgumentException("Notification expiry must be in the future.", nameof(input));
        var notification = new PlatformNotification
        {
            Id = Guid.NewGuid(), Type = Required(input.Type, 150, nameof(input.Type)),
            TitleAr = Required(input.TitleAr, 200, nameof(input.TitleAr)),
            TitleEn = Required(input.TitleEn, 200, nameof(input.TitleEn)),
            BodyAr = Required(input.BodyAr, 1000, nameof(input.BodyAr)),
            BodyEn = Required(input.BodyEn, 1000, nameof(input.BodyEn)),
            ActionUrl = string.IsNullOrWhiteSpace(input.ActionUrl) ? null : Required(input.ActionUrl, 500, nameof(input.ActionUrl)),
            DataJson = JsonSerializer.Serialize(input.Data ?? new Dictionary<string, string>(), JsonOptions),
            CreatedAtUtc = now, ExpiresAtUtc = input.ExpiresAtUtc
        };
        db.Notifications.Add(notification);
        foreach (var accountId in recipients)
        {
            db.NotificationRecipients.Add(new PlatformNotificationRecipient
            {
                NotificationId = notification.Id, AccountId = IdentityAccountId.From(accountId), CreatedAtUtc = now
            });
        }
        var accountValues = recipients.Select(IdentityAccountId.From).ToArray();
        var devices = await db.UserDevices.AsNoTracking()
            .Where(x => accountValues.Contains(x.AccountId)).ToArrayAsync(cancellationToken);
        foreach (var device in devices)
        {
            db.NotificationDeliveries.Add(new PlatformNotificationDelivery
            {
                Id = Guid.NewGuid(), NotificationId = notification.Id, DeviceId = device.Id,
                FcmTokenSnapshot = device.FcmToken, Status = "Pending", CreatedAtUtc = now
            });
        }
    }

    private static string Required(string? value, int maxLength, string parameterName)
    {
        var normalized = value?.Trim() ?? string.Empty;
        if (normalized.Length is 0 || normalized.Length > maxLength)
            throw new ArgumentException($"{parameterName} is required and must be at most {maxLength} characters.", parameterName);
        return normalized;
    }

    private static IReadOnlyDictionary<string, string> ParseData(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOptions)
                   ?? new Dictionary<string, string>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>();
        }
    }
}
