using System.Text.Json;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Api.Realtime;

internal sealed class PlatformNotificationRealtimeDispatcher(
    IServiceScopeFactory scopeFactory,
    IHubContext<PlatformNotificationHub> hub,
    ILogger<PlatformNotificationRealtimeDispatcher> logger) : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);
    private DateTimeOffset _cursor = DateTimeOffset.UtcNow.AddSeconds(-10);
    private HashSet<(Guid NotificationId, Guid AccountId)> _keysAtCursor = [];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DispatchNewNotificationsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Realtime notification dispatch failed");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task DispatchNewNotificationsAsync(CancellationToken cancellationToken)
    {
        await using var scope = scopeFactory.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<PlatformDbContext>();
        var rows = await (
                from recipient in db.NotificationRecipients.AsNoTracking()
                join notification in db.Notifications.AsNoTracking()
                    on recipient.NotificationId equals notification.Id
                where recipient.CreatedAtUtc >= _cursor
                orderby recipient.CreatedAtUtc, recipient.NotificationId
                select new
                {
                    recipient.NotificationId,
                    AccountId = recipient.AccountId.Value,
                    recipient.CreatedAtUtc,
                    notification.Type,
                    notification.DataJson
                })
            .ToListAsync(cancellationToken);

        if (rows.Count == 0) return;

        var latestTimestamp = _cursor;
        var latestKeys = new HashSet<(Guid NotificationId, Guid AccountId)>();
        foreach (var row in rows)
        {
            var key = (row.NotificationId, row.AccountId);
            if (row.CreatedAtUtc == _cursor && _keysAtCursor.Contains(key)) continue;

            var data = ParseData(row.DataJson);
            data["notificationId"] = row.NotificationId.ToString();
            data["type"] = row.Type;
            await hub.Clients.Group(PlatformNotificationHub.GroupName(row.AccountId))
                .SendAsync(
                    PlatformNotificationHub.NotificationCreatedMethod,
                    data,
                    cancellationToken);

            if (row.CreatedAtUtc > latestTimestamp)
            {
                latestTimestamp = row.CreatedAtUtc;
                latestKeys.Clear();
            }
            if (row.CreatedAtUtc == latestTimestamp) latestKeys.Add(key);
        }

        if (latestTimestamp > _cursor)
        {
            _cursor = latestTimestamp;
            _keysAtCursor = latestKeys;
        }
        else
        {
            foreach (var key in latestKeys) _keysAtCursor.Add(key);
        }
    }

    private static Dictionary<string, string> ParseData(string dataJson)
    {
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, string>>(dataJson) ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }
}
