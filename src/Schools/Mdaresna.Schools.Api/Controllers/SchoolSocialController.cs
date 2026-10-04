using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Documents;
using Mdaresna.Schools.Domain.Documents;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.Social;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController, Authorize(Policy = "school-permission:school.social.view"), Route("api/schools/v1/social")]
public sealed class SchoolSocialController(ISchoolDbContextFactory factory, ISchoolDocumentStorage storage) : ControllerBase
{
    private Guid Actor => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : Guid.Empty;
    private string Code => User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? "";
    private bool Has(string action) => User.HasClaim(SchoolClaimTypes.Permission, "school.social." + action);
    private bool Moderator => Has("moderate");
    private bool Editor(SocialPost p) => Has("manage") || p.AuthorUserId == Actor && Has("edit_own");
    private bool DraftEditor(SocialPost p) => Editor(p) || p.AuthorUserId == Actor && p.Status == SocialPostStatus.Draft && Has("create");
    private bool Deleter(SocialPost p) => Has("delete_any") || p.AuthorUserId == Actor && Has("delete_own");
    private bool Reader(SocialPost p) => SocialRules.CanRead(p, Actor, Moderator || Has("manage") || Has("delete_any"));
    private Task<SchoolsDbContext?> Db(CancellationToken ct) => factory.CreateAsync(Code, ct);
    private IActionResult OkData(object? value) => Ok(ApiResponse<object?>.Success(value));
    private IActionResult Fail(string code, int status = 400) => StatusCode(status, ApiResponse<object?>.Failure(status, code, code));

    [HttpGet("posts")]
    public async Task<IActionResult> Posts(long? before, string mode = "feed", int limit = 15, CancellationToken ct = default)
    {
        if (before < 1 || limit is < 1 or > 50 || mode is not ("feed" or "mine" or "review" or "reports")) return Fail("social.invalid");
        if (mode is "review" or "reports" && !Moderator) return Forbid();
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var q = db.SocialPosts.AsNoTracking();
        q = mode switch {
            "mine" => q.Where(x => x.AuthorUserId == Actor && x.Status != SocialPostStatus.Deleted),
            "review" => q.Where(x => x.Status == SocialPostStatus.PendingReview),
            "reports" => q.Where(x => db.SocialReports.Any(r => r.PostId == x.Id && r.ResolvedAtUtc == null)),
            _ => q.Where(x => x.Status == SocialPostStatus.Published)
        };
        if (before.HasValue) q = q.Where(x => x.Id < before);
        var rows = await q.OrderByDescending(x => x.Id).Take(limit + 1).ToArrayAsync(ct);
        var more = rows.Length > limit; rows = rows.Take(limit).ToArray();
        return OkData(new { items = await Cards(db, rows.Select(x => x.Id).ToArray(), ct), nextCursor = more ? (long?)rows[^1].Id : null });
    }

    [HttpGet("posts/{id:long}")]
    public async Task<IActionResult> Post(long id, CancellationToken ct)
    {
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var p = await db.SocialPosts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (p is null || !Reader(p)) return Fail("social.not_found", 404);
        return OkData((await Cards(db, [id], ct)).Single());
    }

    [HttpPost("posts"), Authorize(Policy = "school-permission:school.social.create")]
    public async Task<IActionResult> Create(SocialEdit r, CancellationToken ct)
    {
        if (!SocialRules.ValidText(r.Text, 10000)) return Fail("social.invalid");
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var since = DateTimeOffset.UtcNow.AddMinutes(-1);
        if (await db.SocialPosts.CountAsync(x => x.AuthorUserId == Actor && x.CreatedAtUtc > since, ct) >= 5) return Fail("social.rate_limit", 429);
        var p = new SocialPost { AuthorUserId = Actor, Text = r.Text.Trim(), CommentsEnabled = r.CommentsEnabled,
            CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        db.SocialPosts.Add(p); await db.SaveChangesAsync(ct);
        Audit(db, p, "Created"); await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return OkData(new { p.Id, p.Revision });
    }

    [HttpPut("posts/{id:long}")]
    public async Task<IActionResult> Edit(long id, SocialEdit r, CancellationToken ct)
    {
        if (!SocialRules.ValidText(r.Text, 10000)) return Fail("social.invalid");
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var p = await db.SocialPosts.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (p is null || !Reader(p)) return Fail("social.not_found", 404);
        if (!DraftEditor(p)) return Forbid();
        if (p.Revision != r.Revision) return Fail("social.conflict", 409);
        if (p.Status is SocialPostStatus.Hidden or SocialPostStatus.Deleted) return Fail("social.state", 409);
        if (string.IsNullOrWhiteSpace(r.Text) && !await db.SocialMedia.AnyAsync(x => x.PostId == id && !x.IsDeleted, ct) && p.Status != SocialPostStatus.Draft) return Fail("social.empty");
        Audit(db, p, "BeforeEdit"); p.Text = r.Text.Trim(); p.CommentsEnabled = r.CommentsEnabled;
        // Editing live text must not bypass the configured review requirement.
        var approval = await db.SocialSettings.Select(x => x.RequireApproval).SingleAsync(ct);
        if (p.Status == SocialPostStatus.Published && approval) {
            p.Status = SocialPostStatus.PendingReview; await NotifyReviewers(db, p, "PendingReview", ct);
        }
        Touch(p); Audit(db, p, "Edited"); return await Save(db, p, ct);
    }

    [HttpPost("posts/{id:long}/actions")]
    public async Task<IActionResult> Action(long id, SocialAction r, CancellationToken ct)
    {
        if (r.Reason is null || r.Reason.Length > 1000) return Fail("social.invalid");
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var p = await db.SocialPosts.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (p is null || !Reader(p)) return Fail("social.not_found", 404);
        if (p.Revision != r.Revision) return Fail("social.conflict", 409);
        var media = await db.SocialMedia.Where(x => x.PostId == id && !x.IsDeleted).ToArrayAsync(ct);
        switch (r.Action) {
            case "submit":
                if (!DraftEditor(p)) return Forbid();
                if (p.Status != SocialPostStatus.Draft) return Fail("social.state", 409);
                if (string.IsNullOrWhiteSpace(p.Text) && media.Length == 0) return Fail("social.empty");
                var approval = await db.SocialSettings.Select(x => x.RequireApproval).SingleAsync(ct);
                p.Status = approval || !Has("publish") ? SocialPostStatus.PendingReview : SocialPostStatus.Published;
                if (p.Status == SocialPostStatus.PendingReview) await NotifyReviewers(db, p, "PendingReview", ct);
                break;
            case "approve":
                if (!Moderator) return Forbid();
                if (p.Status != SocialPostStatus.PendingReview) return Fail("social.state", 409);
                p.Status = SocialPostStatus.Published; await Notify(db, p.AuthorUserId, p, "Approved", "تم نشر منشور المدرسة", "School post published", ct);
                break;
            case "return":
                if (!Moderator) return Forbid();
                if (p.Status != SocialPostStatus.PendingReview || string.IsNullOrWhiteSpace(r.Reason)) return Fail("social.reason_required");
                p.Status = SocialPostStatus.Draft;
                await Notify(db, p.AuthorUserId, p, "Returned", "أُعيد المنشور للتعديل؛ افتحه للاطلاع على السبب", "Post returned; open it to read the reason", ct);
                break;
            case "hide":
                if (!Moderator) return Forbid();
                if (p.Status != SocialPostStatus.Published || string.IsNullOrWhiteSpace(r.Reason)) return Fail("social.reason_required");
                p.Status = SocialPostStatus.Hidden; break;
            case "restore":
                if (!Moderator) return Forbid();
                if (p.Status != SocialPostStatus.Hidden || string.IsNullOrWhiteSpace(r.Reason)) return Fail("social.reason_required");
                p.Status = SocialPostStatus.PendingReview; break;
            case "delete":
                if (!Deleter(p)) return Forbid();
                if (p.Status == SocialPostStatus.Deleted) return Fail("social.state", 409);
                p.Status = SocialPostStatus.Deleted; break;
            default: return Fail("social.invalid");
        }
        if (p.Status == SocialPostStatus.Published) {
            if (string.IsNullOrWhiteSpace(p.Text) && media.Length == 0) return Fail("social.empty");
            var ids = media.Select(x => x.DocumentVersionId).ToArray();
            if (await db.DocumentVersions.AnyAsync(x => ids.Contains(x.Id) && x.ValidationStatus == SchoolDocumentValidationStatus.Rejected, ct)) return Fail("social.media_invalid");
            p.PublishedAtUtc = DateTimeOffset.UtcNow;
        }
        Touch(p); Audit(db, p, r.Action, r.Reason); return await Save(db, p, ct);
    }

    [HttpPost("posts/{id:long}/media"), RequestSizeLimit(54_000_000)]
    public async Task<IActionResult> Upload(long id, IFormFile file, [FromForm] int revision, CancellationToken ct)
    {
        if (file.Length is < 16 or > SocialRules.MaxVideoBytes) return Fail("social.media_invalid");
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var p = await db.SocialPosts.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (p is null || !Reader(p)) return Fail("social.not_found", 404);
        if (!DraftEditor(p)) return Forbid();
        if (p.Status != SocialPostStatus.Draft) return Fail("social.state", 409);
        if (p.Revision != revision) return Fail("social.conflict", 409);
        if (await db.SocialMedia.CountAsync(x => x.PostId == id && !x.IsDeleted, ct) >= SocialRules.MaxMedia) return Fail("social.media_limit");
        await using var input = file.OpenReadStream(); var header = new byte[32];
        var n = await input.ReadAsync(header, ct); var type = SocialRules.MediaType(header.AsSpan(0, n));
        if (type is null || type.Value.ContentType.StartsWith("image/") && file.Length > SocialRules.MaxImageBytes) return Fail("social.media_invalid");
        input.Position = 0; var documentId = Guid.NewGuid(); var versionId = Guid.NewGuid();
        var stored = await storage.SaveAsync(Code, documentId, versionId, input, type.Value.Extension, ct);
        var document = new SchoolDocument { Id = documentId, Title = "School social media", CreatedByUserId = Actor,
            CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow };
        document.Versions.Add(new SchoolDocumentVersion { Id = versionId, VersionNumber = 1, OriginalFileName = $"{versionId:N}{type.Value.Extension}",
            ContentType = type.Value.ContentType, SizeBytes = stored.SizeBytes, Sha256 = stored.Sha256, StorageKey = stored.StorageKey,
            ValidationStatus = SchoolDocumentValidationStatus.Pending, CreatedByUserId = Actor, CreatedAtUtc = DateTimeOffset.UtcNow });
        db.Documents.Add(document);
        db.DocumentAudits.Add(new SchoolDocumentAudit { Id = Guid.NewGuid(), DocumentId = documentId, ActorUserId = Actor,
            Action = "SocialMediaUploaded", CreatedAtUtc = DateTimeOffset.UtcNow });
        db.SocialMedia.Add(new SocialMedia { Id = Guid.NewGuid(), PostId = id, DocumentVersionId = versionId,
            SortOrder = (await db.SocialMedia.Where(x => x.PostId == id).MaxAsync(x => (int?)x.SortOrder, ct) ?? -1) + 1 });
        Touch(p); Audit(db, p, "MediaUploaded"); return await Save(db, p, ct);
    }

    [HttpDelete("posts/{id:long}/media/{mediaId:guid}")]
    public async Task<IActionResult> RemoveMedia(long id, Guid mediaId, [FromQuery] int revision, CancellationToken ct)
    {
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var p = await db.SocialPosts.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (p is null || !Reader(p)) return Fail("social.not_found", 404);
        if (!DraftEditor(p)) return Forbid();
        if (p.Status != SocialPostStatus.Draft) return Fail("social.state", 409);
        if (p.Revision != revision) return Fail("social.conflict", 409);
        var media = await db.SocialMedia.SingleOrDefaultAsync(x => x.Id == mediaId && x.PostId == id && !x.IsDeleted, ct);
        if (media is null) return Fail("social.not_found", 404);
        media.IsDeleted = true; Touch(p); Audit(db, p, "MediaRemoved", mediaId.ToString()); return await Save(db, p, ct);
    }

    [HttpGet("posts/{id:long}/media/{mediaId:guid}/content")]
    public async Task<IActionResult> Content(long id, Guid mediaId, CancellationToken ct)
    {
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var p = await db.SocialPosts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (p is null || !Reader(p)) return Fail("social.not_found", 404);
        var version = await (from m in db.SocialMedia from v in db.DocumentVersions
            where m.Id == mediaId && m.PostId == id && (!m.IsDeleted || Moderator) && v.Id == m.DocumentVersionId && v.ValidationStatus != SchoolDocumentValidationStatus.Rejected
            select v).SingleOrDefaultAsync(ct);
        if (version is null) return Fail("social.not_found", 404);
        var stream = await storage.OpenReadAsync(version.StorageKey, ct);
        if (stream is null) return Fail("social.not_found", 404);
        Response.Headers.CacheControl = "private, no-store"; Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(stream, version.ContentType, enableRangeProcessing: true);
    }

    [HttpPut("posts/{id:long}/like"), Authorize(Policy = "school-permission:school.social.interact")]
    public async Task<IActionResult> Like(long id, SocialLikeInput r, CancellationToken ct)
    {
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var p = await db.SocialPosts.SingleOrDefaultAsync(x => x.Id == id && x.Status == SocialPostStatus.Published, ct);
        if (p is null) return Fail("social.not_found", 404);
        var like = await db.SocialLikes.SingleOrDefaultAsync(x => x.PostId == id && x.UserId == Actor, ct);
        if (like?.IsActive == r.Liked || like is null && !r.Liked) return OkData(null);
        if (like is null) { like = new SocialLike { PostId = id, UserId = Actor, CreatedAtUtc = DateTimeOffset.UtcNow }; db.SocialLikes.Add(like); }
        like.IsActive = r.Liked; like.UpdatedAtUtc = DateTimeOffset.UtcNow;
        Touch(p); return await Save(db, p, ct);
    }

    [HttpGet("posts/{id:long}/comments")]
    public async Task<IActionResult> Comments(long id, long? before, int limit = 20, CancellationToken ct = default)
    {
        if (before < 1 || limit is < 1 or > 50) return Fail("social.invalid");
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var p = await db.SocialPosts.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id, ct);
        if (p is null || !Reader(p)) return Fail("social.not_found", 404);
        var canEdit = Has("edit_own");
        var rows = await db.SocialComments.AsNoTracking().Where(x => x.PostId == id && !x.IsDeleted && (!before.HasValue || x.Id < before))
            .OrderByDescending(x => x.Id).Take(limit + 1).Select(x => new { x.Id, x.UserId, x.Text, x.Revision, x.CreatedAtUtc, x.EditedAtUtc,
                authorName = db.LocalUsers.Where(u => u.Id == x.UserId).Select(u => u.Person.DisplayName).First(),
                canEdit = x.UserId == Actor && canEdit, }).ToArrayAsync(ct);
        return OkData(new { items = rows.Take(limit), nextCursor = rows.Length > limit ? (long?)rows[limit - 1].Id : null });
    }

    [HttpPost("posts/{id:long}/comments"), Authorize(Policy = "school-permission:school.social.interact")]
    public async Task<IActionResult> AddComment(long id, SocialCommentInput r, CancellationToken ct)
    {
        if (!SocialRules.ValidText(r.Text, 2000) || string.IsNullOrWhiteSpace(r.Text)) return Fail("social.invalid");
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var p = await db.SocialPosts.SingleOrDefaultAsync(x => x.Id == id && x.Status == SocialPostStatus.Published, ct);
        if (p is null) return Fail("social.not_found", 404);
        if (!p.CommentsEnabled) return Fail("social.comments_closed", 409);
        var since = DateTimeOffset.UtcNow.AddMinutes(-1);
        if (await db.SocialComments.CountAsync(x => x.UserId == Actor && x.CreatedAtUtc > since, ct) >= 10) return Fail("social.rate_limit", 429);
        db.SocialComments.Add(new SocialComment { PostId = id, UserId = Actor, Text = r.Text.Trim(), CreatedAtUtc = DateTimeOffset.UtcNow });
        Touch(p);
        // One notification per post / author / school UTC day; no comment body or self-notification.
        await Notify(db, p.AuthorUserId, p, "Comments-" + DateTimeOffset.UtcNow.ToString("yyyyMMdd"), "تعليقات جديدة على منشور المدرسة", "New comments on school post", ct);
        return await Save(db, p, ct);
    }

    [HttpPut("posts/{id:long}/comments/{commentId:long}")]
    public async Task<IActionResult> EditComment(long id, long commentId, SocialCommentInput r, CancellationToken ct)
    {
        if (!SocialRules.ValidText(r.Text, 2000) || string.IsNullOrWhiteSpace(r.Text)) return Fail("social.invalid");
        return await ChangeComment(id, commentId, r, false, ct);
    }
    [HttpDelete("posts/{id:long}/comments/{commentId:long}")]
    public Task<IActionResult> DeleteComment(long id, long commentId, [FromQuery] int revision, CancellationToken ct) =>
        ChangeComment(id, commentId, new SocialCommentInput("", revision), true, ct);
    private async Task<IActionResult> ChangeComment(long id, long commentId, SocialCommentInput r, bool delete, CancellationToken ct)
    {
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var p = await db.SocialPosts.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (p is null || !Reader(p)) return Fail("social.not_found", 404);
        var c = await db.SocialComments.SingleOrDefaultAsync(x => x.PostId == id && x.Id == commentId && !x.IsDeleted, ct);
        if (c is null) return Fail("social.not_found", 404);
        if (!Has("manage") && !(c.UserId == Actor && Has(delete ? "delete_own" : "edit_own"))) return Forbid();
        if (c.Revision != r.Revision) return Fail("social.conflict", 409);
        if (p.Status != SocialPostStatus.Published && !(delete && Has("manage"))) return Fail("social.state", 409);
        Audit(db, p, delete ? "CommentDeleted" : "CommentEdited", JsonSerializer.Serialize(c));
        if (delete) c.IsDeleted = true; else { c.Text = r.Text.Trim(); c.EditedAtUtc = DateTimeOffset.UtcNow; }
        c.Revision++; Touch(p); return await Save(db, p, ct);
    }

    [HttpPost("posts/{id:long}/reports")]
    public async Task<IActionResult> Report(long id, SocialReportInput r, CancellationToken ct)
    {
        if (!SocialRules.ReportReasons.Contains(r.Reason) || r.Details is null || r.Details.Length > 1000 || r.Reason == "other" && string.IsNullOrWhiteSpace(r.Details)) return Fail("social.invalid");
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var p = await db.SocialPosts.SingleOrDefaultAsync(x => x.Id == id && x.Status == SocialPostStatus.Published, ct);
        if (p is null) return Fail("social.not_found", 404);
        var comment = r.CommentId.HasValue ? await db.SocialComments.SingleOrDefaultAsync(x => x.Id == r.CommentId && x.PostId == id && !x.IsDeleted, ct) : null;
        if (r.CommentId.HasValue && comment is null) return Fail("social.not_found", 404);
        var target = r.CommentId.HasValue ? "comment:" + r.CommentId : "post";
        var existing = await db.SocialReports.SingleOrDefaultAsync(x => x.PostId == id && x.UserId == Actor && x.TargetKey == target, ct);
        if (existing is { ResolvedAtUtc: null }) return OkData(null);
        var since = DateTimeOffset.UtcNow.AddHours(-1);
        if (await db.SocialReports.CountAsync(x => x.UserId == Actor && x.CreatedAtUtc > since, ct) >= 5) return Fail("social.rate_limit", 429);
        var first = !await db.SocialReports.AnyAsync(x => x.PostId == id && x.ResolvedAtUtc == null, ct);
        var media = await (from m in db.SocialMedia from v in db.DocumentVersions where m.PostId == id && !m.IsDeleted && v.Id == m.DocumentVersionId
            select new { m.Id, v.ContentType, v.SizeBytes }).ToArrayAsync(ct);
        if (existing is not null) Audit(db, p, "ReportReopened", JsonSerializer.Serialize(existing));
        var report = existing ?? new SocialReport { PostId = id, UserId = Actor, CommentId = r.CommentId, TargetKey = target };
        report.Reason = r.Reason; report.Details = r.Details.Trim(); report.CreatedAtUtc = DateTimeOffset.UtcNow;
        report.EvidenceJson = JsonSerializer.Serialize(new { post = p, comment, media });
        report.ResolvedAtUtc = null; report.ResolvedByUserId = null; report.Resolution = null;
        if (existing is null) db.SocialReports.Add(report);
        if (first) await NotifyReviewers(db, p, "Reports", ct);
        Touch(p); return await Save(db, p, ct);
    }

    [HttpGet("posts/{id:long}/reports"), Authorize(Policy = "school-permission:school.social.moderate")]
    public async Task<IActionResult> Reports(long id, long? before, int limit = 20, CancellationToken ct = default)
    {
        if (limit is < 1 or > 50 || before < 1) return Fail("social.invalid");
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var q = db.SocialReports.AsNoTracking().Where(x => x.PostId == id && (!before.HasValue || x.Id < before));
        var rows = await q.OrderByDescending(x => x.Id).Take(limit + 1).Select(x => new { x.Id, x.UserId, x.CommentId, x.Reason, x.Details,
            x.CreatedAtUtc, x.EvidenceJson, x.Resolution, x.ResolvedAtUtc,
            reporterName = db.LocalUsers.Where(u => u.Id == x.UserId).Select(u => u.Person.DisplayName).First() }).ToArrayAsync(ct);
        return OkData(new { items = rows.Take(limit), nextCursor = rows.Length > limit ? (long?)rows[limit - 1].Id : null });
    }

    [HttpPost("posts/{id:long}/resolve"), Authorize(Policy = "school-permission:school.social.moderate")]
    public async Task<IActionResult> Resolve(long id, SocialAction r, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(r.Reason) || r.Reason.Length > 1000 || r.Action is not ("dismiss" or "hide" or "delete" or "delete_comment")) return Fail("social.reason_required");
        if (r.Action == "delete" && !Has("delete_any") || r.Action == "delete_comment" && !Has("manage")) return Forbid();
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var p = await db.SocialPosts.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (p is null) return Fail("social.not_found", 404);
        if (p.Revision != r.Revision) return Fail("social.conflict", 409);
        var reports = await db.SocialReports.Where(x => x.PostId == id && x.ResolvedAtUtc == null && (r.Action != "delete_comment" || x.CommentId == r.CommentId)).ToArrayAsync(ct);
        if (reports.Length == 0) return Fail("social.state", 409);
        if (r.Action == "hide" && p.Status != SocialPostStatus.Deleted) p.Status = SocialPostStatus.Hidden;
        if (r.Action == "delete") p.Status = SocialPostStatus.Deleted;
        if (r.Action == "delete_comment") {
            var c = await db.SocialComments.SingleOrDefaultAsync(x => x.PostId == id && x.Id == r.CommentId, ct);
            if (c is null) return Fail("social.not_found", 404);
            c.IsDeleted = true; c.Revision++;
        }
        foreach (var report in reports) { report.ResolvedAtUtc = DateTimeOffset.UtcNow; report.ResolvedByUserId = Actor; report.Resolution = r.Action + ": " + r.Reason.Trim(); }
        Touch(p); Audit(db, p, "ReportsResolved", JsonSerializer.Serialize(r));
        await Notify(db, p.AuthorUserId, p, "Moderated", "تمت مراجعة بلاغات على منشورك", "Reports on your post were reviewed", ct);
        return await Save(db, p, ct);
    }

    [HttpGet("settings")]
    public async Task<IActionResult> Settings(CancellationToken ct)
    {
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var s = await db.SocialSettings.SingleAsync(ct);
        var candidates = Has("settings") ? await Eligible(db).OrderBy(u => u.Person.DisplayName).Select(u => new { u.Id, name = u.Person.DisplayName }).ToArrayAsync(ct) : [];
        return OkData(new { s.RequireApproval, s.Revision, maxMedia = SocialRules.MaxMedia, maxImageBytes = SocialRules.MaxImageBytes,
            maxVideoBytes = SocialRules.MaxVideoBytes, reviewers = Has("settings") ? await db.SocialReviewers.Select(x => x.UserId).ToArrayAsync(ct) : [], candidates });
    }

    [HttpPut("settings"), Authorize(Policy = "school-permission:school.social.settings")]
    public async Task<IActionResult> SaveSettings(SocialSettingsInput r, CancellationToken ct)
    {
        if (r.Reviewers is null || r.Reviewers.Length > 100 || r.Reviewers.Distinct().Count() != r.Reviewers.Length) return Fail("social.invalid");
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var s = await db.SocialSettings.SingleAsync(ct);
        if (s.Revision != r.Revision) return Fail("social.conflict", 409);
        if (await Eligible(db).CountAsync(u => r.Reviewers.Contains(u.Id), ct) != r.Reviewers.Length || r.RequireApproval && !await Eligible(db).AnyAsync(ct)) return Fail("social.reviewer_invalid");
        var old = await db.SocialReviewers.ToArrayAsync(ct);
        db.SocialReviewers.RemoveRange(old.Where(x => !r.Reviewers.Contains(x.UserId)));
        foreach (var id in r.Reviewers.Except(old.Select(x => x.UserId))) db.SocialReviewers.Add(new SocialReviewer { UserId = id });
        s.RequireApproval = r.RequireApproval; s.Revision++;
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateConcurrencyException) { return Fail("social.conflict", 409); }
        return OkData(new { s.Revision });
    }

    private async Task<object[]> Cards(SchoolsDbContext db, long[] ids, CancellationToken ct)
    {
        var actor = Actor; var edit = Has("edit_own"); var manage = Has("manage"); var del = Has("delete_any"); var ownDel = Has("delete_own"); var create = Has("create");
        var rows = await db.SocialPosts.AsNoTracking().Where(x => ids.Contains(x.Id)).OrderByDescending(x => x.Id).Select(p => new {
            p.Id, p.Text, p.Status, p.AuthorUserId, p.CommentsEnabled, p.Revision, p.CreatedAtUtc, p.UpdatedAtUtc, p.PublishedAtUtc,
            canEdit = manage || p.AuthorUserId == actor && (edit || create && p.Status == SocialPostStatus.Draft),
            canDelete = del || ownDel && p.AuthorUserId == actor,
            likeCount = db.SocialLikes.Count(x => x.PostId == p.Id && x.IsActive),
            liked = db.SocialLikes.Any(x => x.PostId == p.Id && x.UserId == actor && x.IsActive),
            commentCount = db.SocialComments.Count(x => x.PostId == p.Id && !x.IsDeleted),
            pendingReports = Moderator ? db.SocialReports.Count(x => x.PostId == p.Id && x.ResolvedAtUtc == null) : 0,
            lastReason = p.AuthorUserId == actor || manage || Moderator ? db.SocialAudits.Where(x => x.PostId == p.Id && x.Action == "return").OrderByDescending(x => x.Id).Select(x => x.SnapshotJson).FirstOrDefault() : null
        }).ToArrayAsync(ct);
        var media = await (from m in db.SocialMedia from v in db.DocumentVersions where ids.Contains(m.PostId) && !m.IsDeleted && v.Id == m.DocumentVersionId
            orderby m.SortOrder select new { m.Id, m.PostId, m.SortOrder, v.ContentType, v.SizeBytes }).ToArrayAsync(ct);
        return rows.Select(p => (object)new { p.Id, p.Text, p.Status, p.AuthorUserId, p.CommentsEnabled, p.Revision, p.CreatedAtUtc, p.UpdatedAtUtc, p.PublishedAtUtc,
            p.canEdit, p.canDelete, p.likeCount, p.liked, p.commentCount, p.pendingReports, p.lastReason, media = media.Where(x => x.PostId == p.Id).ToArray() }).ToArray();
    }
    private IQueryable<LocalUserAccount> Eligible(SchoolsDbContext db) => db.LocalUsers.Where(u => u.Status == LocalUserStatus.Active && u.Person.Status == PersonStatus.Active &&
        u.Roles.Any(r => r.Role.IsActive && r.Role.Permissions.Any(p => p.Permission.Code == "school.social.moderate")) &&
        u.Roles.Any(r => r.Role.IsActive && r.Role.Permissions.Any(p => p.Permission.Code == "school.social.view")));
    private async Task NotifyReviewers(SchoolsDbContext db, SocialPost p, string type, CancellationToken ct)
    {
        var configured = await db.SocialReviewers.Select(x => x.UserId).ToArrayAsync(ct);
        var eligible = Eligible(db); if (configured.Length > 0) eligible = eligible.Where(x => configured.Contains(x.Id));
        foreach (var user in await eligible.Select(x => x.Id).ToArrayAsync(ct))
            await Notify(db, user, p, type, type == "Reports" ? "بلاغات على منشور تحتاج مراجعتك" : "منشور بانتظار مراجعتك",
                type == "Reports" ? "Post reports need your review" : "Post awaiting your review", ct, type == "Reports");
    }
    private async Task Notify(SchoolsDbContext db, Guid recipient, SocialPost p, string type, string ar, string en, CancellationToken ct, bool self = false)
    {
        if (recipient == Actor && !self) return;
        var active = await db.LocalUsers.AnyAsync(u => u.Id == recipient && u.Status == LocalUserStatus.Active && u.Person.Status == PersonStatus.Active &&
            u.Roles.Any(r => r.Role.IsActive && r.Role.Permissions.Any(q => q.Permission.Code == "school.social.view")), ct);
        if (!active) return;
        var key = $"social/{p.Id}/{type}/{(type.StartsWith("Comments-") ? 0 : p.Revision)}";
        if (await db.SchoolUserNotifications.AnyAsync(x => x.RecipientUserId == recipient && x.EventKey == key, ct)) return;
        db.SchoolUserNotifications.Add(new SchoolUserNotification { Id = Guid.NewGuid(), RecipientUserId = recipient, EventKey = key,
            Type = "SchoolSocial" + type, TitleAr = ar, TitleEn = en, BodyAr = "افتح المنشور للاطلاع على أحدث حالة واتخاذ الإجراء المتاح لك.",
            BodyEn = "Open the post to see its current state and available actions.", RelatedEntityType = "SocialPost", RelatedEntityId = p.Id.ToString(),
            CreatedAtUtc = DateTimeOffset.UtcNow, UpdatedAtUtc = DateTimeOffset.UtcNow });
    }
    private void Audit(SchoolsDbContext db, SocialPost p, string action, string? reason = null) =>
        db.SocialAudits.Add(new SocialAudit { PostId = p.Id, ActorUserId = Actor, Action = action,
            SnapshotJson = JsonSerializer.Serialize(new { post = p, reason }), CreatedAtUtc = DateTimeOffset.UtcNow });
    private static void Touch(SocialPost p) { p.Revision++; p.UpdatedAtUtc = DateTimeOffset.UtcNow; }
    private async Task<IActionResult> Save(SchoolsDbContext db, SocialPost p, CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return Fail("social.conflict", 409); }
        catch (DbUpdateException ex) when (KnownCollision(ex)) { return Fail("social.conflict", 409); }
        return OkData(new { p.Id, p.Revision });
    }
    public static bool KnownCollision(DbUpdateException ex)
    {
        var indexes = new[] { "PK_social_likes", "IX_social_reports_PostId_UserId_TargetKey", "IX_school_user_notifications_RecipientUserId_EventKey" };
        return ex.InnerException is Npgsql.PostgresException { SqlState: "23505" } pg && indexes.Contains(pg.ConstraintName) ||
            ex.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 } sql && indexes.Any(x => sql.Message.Contains(x, StringComparison.Ordinal));
    }
}
public sealed record SocialEdit(string Text, bool CommentsEnabled = true, int Revision = 0);
public sealed record SocialAction(string Action, int Revision, string Reason = "", long? CommentId = null);
public sealed record SocialLikeInput(bool Liked);
public sealed record SocialCommentInput(string Text, int Revision = 0);
public sealed record SocialReportInput(string Reason, string Details = "", long? CommentId = null);
public sealed record SocialSettingsInput(bool RequireApproval, int Revision, Guid[] Reviewers);
