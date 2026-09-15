using System.Text.Json;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Mdaresna.Platform.Infrastructure.Messaging;

public sealed class PlatformFirebaseOptions
{
    public const string SectionName = "Notifications:Firebase";
    public bool Enabled { get; set; }
    public string ProjectId { get; set; } = string.Empty;
    public int DispatchIntervalSeconds { get; set; } = 10;
    public int BatchSize { get; set; } = 100;
}

public sealed record PlatformPushResult(bool Succeeded, bool InvalidToken, string? Error);

public interface IPlatformPushSender
{
    bool IsEnabled { get; }
    Task<PlatformPushResult> SendAsync(string token, string title, string body,
        IReadOnlyDictionary<string, string> data, CancellationToken cancellationToken = default);
}

public sealed class FirebasePlatformPushSender : IPlatformPushSender, IDisposable
{
    private const string FirebaseAppName = "mdaresna-platform-notifications";
    private readonly PlatformFirebaseOptions options;
    private readonly ILogger<FirebasePlatformPushSender> logger;
    private readonly object sync = new();
    private FirebaseApp? app;

    public FirebasePlatformPushSender(IOptions<PlatformFirebaseOptions> options,
        ILogger<FirebasePlatformPushSender> logger)
    {
        this.options = options.Value;
        this.logger = logger;
    }

    public bool IsEnabled => options.Enabled && !string.IsNullOrWhiteSpace(options.ProjectId);

    public async Task<PlatformPushResult> SendAsync(string token, string title, string body,
        IReadOnlyDictionary<string, string> data, CancellationToken cancellationToken = default)
    {
        if (!IsEnabled) return new(false, false, "Firebase delivery is disabled.");
        try
        {
            var messaging = FirebaseMessaging.GetMessaging(GetOrCreateApp());
            var message = new Message
            {
                Token = token,
                Notification = new Notification { Title = title, Body = body },
                Data = data.ToDictionary(x => x.Key, x => x.Value, StringComparer.Ordinal),
                Android = new AndroidConfig
                {
                    Priority = Priority.High
                },
                Apns = new ApnsConfig
                {
                    Aps = new Aps { Sound = "default", ContentAvailable = true }
                },
                Webpush = new WebpushConfig
                {
                    FcmOptions = data.TryGetValue("actionUrl", out var url) && Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out _)
                        ? new WebpushFcmOptions { Link = url }
                        : null
                }
            };
            await messaging.SendAsync(message, cancellationToken);
            return new(true, false, null);
        }
        catch (FirebaseMessagingException ex)
        {
            var code = ex.MessagingErrorCode?.ToString() ?? ex.ErrorCode.ToString();
            var invalid = code.Contains("Unregistered", StringComparison.OrdinalIgnoreCase) ||
                          code.Contains("InvalidArgument", StringComparison.OrdinalIgnoreCase) ||
                          code.Contains("SenderIdMismatch", StringComparison.OrdinalIgnoreCase);
            logger.LogWarning(ex, "FCM delivery failed with code {Code}", code);
            return new(false, invalid, code);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogError(ex, "FCM delivery failed unexpectedly");
            return new(false, false, ex.GetType().Name);
        }
    }

    private FirebaseApp GetOrCreateApp()
    {
        if (app is not null) return app;
        lock (sync)
        {
            app ??= FirebaseApp.GetInstance(FirebaseAppName) ?? FirebaseApp.Create(new AppOptions
            {
                Credential = GoogleCredential.GetApplicationDefault(),
                ProjectId = options.ProjectId
            }, FirebaseAppName);
            return app;
        }
    }

    public void Dispose()
    {
        app?.Delete();
        app = null;
    }
}

public sealed class PlatformNotificationDispatchWorker(
    IServiceScopeFactory scopeFactory,
    IOptions<PlatformFirebaseOptions> options,
    ILogger<PlatformNotificationDispatchWorker> logger) : BackgroundService
{
    private readonly PlatformFirebaseOptions settings = options.Value;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchOnceAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Platform notification dispatch cycle failed");
            }
            await Task.Delay(TimeSpan.FromSeconds(Math.Clamp(settings.DispatchIntervalSeconds, 2, 300)), stoppingToken);
        }
    }

    internal async Task DispatchOnceAsync(CancellationToken cancellationToken)
    {
        using var scope = scopeFactory.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<IPlatformPushSender>();
        if (!sender.IsEnabled) return;
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var now = DateTimeOffset.UtcNow;
        var deliveries = await db.NotificationDeliveries
            .Where(x => x.Status == "Pending" &&
                        (x.NextAttemptAtUtc == null || x.NextAttemptAtUtc <= now))
            .OrderBy(x => x.CreatedAtUtc)
            .Take(Math.Clamp(settings.BatchSize, 1, 500))
            .ToArrayAsync(cancellationToken);
        foreach (var delivery in deliveries)
        {
            var notification = await db.Notifications.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == delivery.NotificationId, cancellationToken);
            var device = await db.UserDevices.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == delivery.DeviceId, cancellationToken);
            if (notification is null || device is null || notification.ExpiresAtUtc <= now)
            {
                delivery.Status = "Skipped";
                delivery.LastError = device is null ? "Device registration no longer exists." : "Notification expired.";
                continue;
            }
            var arabic = device.LanguageCode == "ar";
            var data = ParseData(notification.DataJson);
            data["notificationId"] = notification.Id.ToString("D");
            data["type"] = notification.Type;
            if (!string.IsNullOrWhiteSpace(notification.ActionUrl)) data["actionUrl"] = notification.ActionUrl;
            var result = await sender.SendAsync(device.FcmToken,
                arabic ? notification.TitleAr : notification.TitleEn,
                arabic ? notification.BodyAr : notification.BodyEn,
                data, cancellationToken);
            delivery.AttemptCount++;
            delivery.LastError = result.Error;
            if (result.Succeeded)
            {
                delivery.Status = "Sent";
                delivery.SentAtUtc = DateTimeOffset.UtcNow;
                delivery.NextAttemptAtUtc = null;
            }
            else if (result.InvalidToken || delivery.AttemptCount >= 8)
            {
                delivery.Status = "Failed";
                delivery.NextAttemptAtUtc = null;
                if (result.InvalidToken)
                {
                    var stale = await db.UserDevices.SingleOrDefaultAsync(x => x.Id == device.Id, cancellationToken);
                    if (stale is not null) db.UserDevices.Remove(stale);
                }
            }
            else
            {
                var delayMinutes = Math.Min(60, Math.Pow(2, delivery.AttemptCount));
                delivery.NextAttemptAtUtc = DateTimeOffset.UtcNow.AddMinutes(delayMinutes);
            }
            await db.SaveChangesAsync(cancellationToken);
        }
        if (db.ChangeTracker.HasChanges()) await db.SaveChangesAsync(cancellationToken);
    }

    private static Dictionary<string, string> ParseData(string json)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json,
                       new JsonSerializerOptions(JsonSerializerDefaults.Web))
                   ?? new Dictionary<string, string>();
        }
        catch (JsonException)
        {
            return new Dictionary<string, string>();
        }
    }
}
