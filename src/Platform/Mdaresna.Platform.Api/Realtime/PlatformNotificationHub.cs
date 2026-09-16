using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Mdaresna.Platform.Api.Realtime;

[Authorize]
public sealed class PlatformNotificationHub : Hub
{
    public const string Path = "/hubs/platform-notifications";
    public const string NotificationCreatedMethod = "notificationCreated";

    public override async Task OnConnectedAsync()
    {
        var accountId = Context.User?.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (!Guid.TryParse(accountId, out var parsedAccountId))
        {
            Context.Abort();
            return;
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(parsedAccountId));
        await base.OnConnectedAsync();
    }

    internal static string GroupName(Guid accountId) => $"platform-account:{accountId:N}";
}
