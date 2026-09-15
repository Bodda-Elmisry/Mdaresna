using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Infrastructure.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.Api.Controllers.Notifications;

[ApiController]
[Authorize]
[Route("api/platform/v1/me")]
public sealed class PlatformNotificationsController(IPlatformNotificationService notifications) : ControllerBase
{
    [HttpPut("devices/{installationId}")]
    public async Task<IActionResult> RegisterDevice(
        [FromRoute, MaxLength(128)] string installationId,
        [FromBody] RegisterPlatformDeviceRequest request,
        CancellationToken cancellationToken)
    {
        await notifications.RegisterDeviceAsync(CurrentAccountId(), new PlatformDeviceRegistration(
            installationId, request.FcmToken, request.Platform, request.LanguageCode, request.DeviceName),
            cancellationToken);
        return Ok(ApiResponse<object?>.Success(null,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpDelete("devices/{installationId}")]
    public async Task<IActionResult> RemoveDevice(
        [FromRoute, MaxLength(128)] string installationId,
        CancellationToken cancellationToken)
    {
        await notifications.RemoveDeviceAsync(CurrentAccountId(), installationId, cancellationToken);
        return Ok(ApiResponse<object?>.Success(null,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpGet("notifications")]
    public async Task<IActionResult> List(
        [FromQuery] string languageCode = "ar",
        [FromQuery, Range(1, int.MaxValue)] int pageNumber = 1,
        [FromQuery, Range(1, 50)] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var page = await notifications.ListAsync(CurrentAccountId(), languageCode, pageNumber, pageSize,
            cancellationToken);
        return Ok(ApiResponse<PlatformNotificationPage>.Success(page,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpGet("notifications/unread-count")]
    public async Task<IActionResult> UnreadCount(CancellationToken cancellationToken)
    {
        var count = await notifications.GetUnreadCountAsync(CurrentAccountId(), cancellationToken);
        return Ok(ApiResponse<object>.Success(new { unreadCount = count },
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPut("notifications/{notificationId:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid notificationId, CancellationToken cancellationToken)
    {
        await notifications.MarkReadAsync(CurrentAccountId(), notificationId, cancellationToken);
        return Ok(ApiResponse<object?>.Success(null,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPut("notifications/read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        await notifications.MarkAllReadAsync(CurrentAccountId(), cancellationToken);
        return Ok(ApiResponse<object?>.Success(null,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    private Guid CurrentAccountId() =>
        Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var accountId) && accountId != Guid.Empty
            ? accountId
            : throw new InvalidOperationException("A validated Platform account is required.");
}

public sealed record RegisterPlatformDeviceRequest(
    [Required, MaxLength(2048)] string FcmToken,
    [Required, MaxLength(16)] string Platform,
    [Required, MaxLength(5)] string LanguageCode,
    [MaxLength(200)] string? DeviceName);
