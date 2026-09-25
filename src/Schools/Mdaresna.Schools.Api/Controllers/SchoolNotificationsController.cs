using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/schools/v1/notifications")]
public sealed class SchoolNotificationsController(ISchoolDbContextFactory dbFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var userId = CurrentUserId(); if (userId == Guid.Empty) return Unauthorized();
        var rows = await db.SchoolUserNotifications.AsNoTracking().Where(x => x.RecipientUserId == userId)
            .OrderByDescending(x => x.CreatedAtUtc).Take(50)
            .Select(x => new SchoolNotificationResponse(x.Id, x.Type, x.TitleAr, x.TitleEn, x.BodyAr, x.BodyEn,
                x.RelatedEntityType, x.RelatedEntityId, x.IsRead, x.ReadAtUtc, x.CreatedAtUtc)).ToArrayAsync(ct);
        var unread = await db.SchoolUserNotifications.CountAsync(x => x.RecipientUserId == userId && !x.IsRead, ct);
        return Ok(ApiResponse<SchoolNotificationsResponse>.Success(new(rows, unread), correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:guid}/read")]
    public async Task<IActionResult> MarkRead(Guid id, CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var userId = CurrentUserId();
        var item = await db.SchoolUserNotifications.SingleOrDefaultAsync(x => x.Id == id && x.RecipientUserId == userId, ct);
        if (item is null) return NotFound(ApiResponse<object?>.Failure(404, "notifications.not_found", "Notification was not found.", correlationId: HttpContext.TraceIdentifier));
        if (!item.IsRead) { item.IsRead = true; item.ReadAtUtc = DateTimeOffset.UtcNow; item.UpdatedAtUtc = item.ReadAtUtc.Value; await db.SaveChangesAsync(ct); }
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> MarkAllRead(CancellationToken ct)
    {
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var userId = CurrentUserId(); var now = DateTimeOffset.UtcNow;
        var items = await db.SchoolUserNotifications.Where(x => x.RecipientUserId == userId && !x.IsRead).ToListAsync(ct);
        foreach (var item in items) { item.IsRead = true; item.ReadAtUtc = now; item.UpdatedAtUtc = now; }
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private async Task<SchoolsDbContext?> RequireDb(CancellationToken ct) =>
        await dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, ct);
    private Guid CurrentUserId() => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : Guid.Empty;
}

public sealed record SchoolNotificationResponse(Guid Id, string Type, string TitleAr, string TitleEn, string BodyAr,
    string BodyEn, string? RelatedEntityType, string? RelatedEntityId, bool IsRead, DateTimeOffset? ReadAtUtc,
    DateTimeOffset CreatedAtUtc);
public sealed record SchoolNotificationsResponse(IReadOnlyList<SchoolNotificationResponse> Items, int UnreadCount);
