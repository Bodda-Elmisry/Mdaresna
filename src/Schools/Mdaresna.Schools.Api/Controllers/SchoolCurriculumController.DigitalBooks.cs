using System.Data;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Documents;
using Mdaresna.Schools.Domain.Documents;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Mdaresna.Schools.Api.Controllers;

public sealed partial class SchoolCurriculumController
{
    [HttpGet("book-versions/{id:guid}/digital")]
    public async Task<IActionResult> DigitalBook(Guid id, CancellationToken ct)
    {
        if (!Can("view")) return Forbid();
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await db.BookVersions.AnyAsync(x => x.Id == id && x.IsActive && x.Book.IsActive, ct)) return Missing();
        var link = await DigitalQuery(db, id).SingleOrDefaultAsync(ct);
        return Ok(ApiResponse<DigitalBookResponse?>.Success(link is null ? null : DigitalResponse(link)));
    }

    [HttpPost("book-versions/{id:guid}/digital")]
    [Consumes("multipart/form-data"), RequestSizeLimit(54 * 1024 * 1024)]
    public async Task<IActionResult> UploadDigitalBook(Guid id, IFormFile file, [FromForm] int expectedRevision,
        [FromForm] bool shareWithFamily, [FromForm] bool allowDownload, [FromServices] ISchoolDocumentStorage storage,
        [FromServices] IBookPdfRenderer renderer, CancellationToken ct)
    {
        if (!Can("manage")) return Forbid();
        if (file is null || file.Length is < 5 or > 50 * 1024 * 1024) return DigitalFailure(400, "books.invalid_pdf", "Use a PDF up to 50 MiB.");
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        if (!await db.BookVersions.AnyAsync(x => x.Id == id && x.IsActive && x.Book.IsActive, ct)) return Missing();
        await using var input = file.OpenReadStream(); var prefix = new byte[5]; await input.ReadExactlyAsync(prefix, ct);
        if (!prefix.AsSpan().SequenceEqual("%PDF-"u8) || !input.CanSeek) return DigitalFailure(400, "books.invalid_pdf", "Use a valid PDF.");
        input.Position = 0; int pages;
        try { pages = await renderer.PageCountAsync(input, ct); }
        catch (InvalidDataException) { return DigitalFailure(400, "books.invalid_pdf", "Use a valid unpassworded PDF."); }
        catch (System.ComponentModel.Win32Exception) { return DigitalFailure(503, "books.renderer_unavailable", "Book reader is not configured."); }
        await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct) : null;
        var link = await db.BookVersionDigitalDocuments.SingleOrDefaultAsync(x => x.BookVersionId == id, ct);
        if ((link?.Revision ?? 0) != expectedRevision) return DigitalFailure(409, "books.changed", "Reload the digital book before modifying it.");
        var actor = CurrentUserId() ?? Guid.Empty; var now = DateTimeOffset.UtcNow; var documentId = Guid.NewGuid(); var versionId = Guid.NewGuid();
        input.Position = 0;
        var stored = await storage.SaveAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? "", documentId, versionId, input, ".pdf", ct);
        var stem = Path.GetFileNameWithoutExtension(Path.GetFileName(file.FileName.Replace('\\', '/')));
        var name = new string(stem.Where(c => !char.IsControl(c)).Take(150).ToArray()); if (string.IsNullOrWhiteSpace(name)) name = "book"; name += ".pdf";
        var document = new SchoolDocument { Id = documentId, Title = name, CreatedByUserId = actor, CreatedAtUtc = now, UpdatedAtUtc = now };
        var version = new SchoolDocumentVersion { Id = versionId, DocumentId = documentId, Document = document, VersionNumber = 1, OriginalFileName = name, ContentType = "application/pdf", SizeBytes = stored.SizeBytes, Sha256 = stored.Sha256, StorageKey = stored.StorageKey, ValidationStatus = SchoolDocumentValidationStatus.Pending, CreatedByUserId = actor, CreatedAtUtc = now };
        db.DocumentVersions.Add(version);
        if (link is null) { link = new() { BookVersionId = id, Revision = 0 }; db.BookVersionDigitalDocuments.Add(link); }
        link.DocumentVersionId = versionId; link.Version = version; link.ShareWithFamily = shareWithFamily; link.AllowDownload = allowDownload;
        link.PageCount = pages; link.Revision++; link.UpdatedAtUtc = now; link.UpdatedByUserId = actor;
        DigitalAudit(db, link, "BookDigitalUploaded", actor);
        try { await db.SaveChangesAsync(ct); if (tx is not null) await tx.CommitAsync(ct); }
        catch (DbUpdateException) { await storage.DeleteAsync(stored.StorageKey, CancellationToken.None); return DigitalFailure(409, "books.changed", "Reload and retry."); }
        catch { await storage.DeleteAsync(stored.StorageKey, CancellationToken.None); throw; }
        return Ok(ApiResponse<DigitalBookResponse>.Success(DigitalResponse(link)));
    }

    [HttpPut("book-versions/{id:guid}/digital/policy")]
    public async Task<IActionResult> DigitalBookPolicy(Guid id, DigitalBookPolicy request, CancellationToken ct)
    {
        if (!Can("manage")) return Forbid();
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var link = await DigitalQuery(db, id).SingleOrDefaultAsync(ct); if (link is null) return Missing();
        if (link.Revision != request.ExpectedRevision) return DigitalFailure(409, "books.changed", "Reload and retry.");
        link.ShareWithFamily = request.ShareWithFamily; link.AllowDownload = request.AllowDownload; link.Revision++;
        link.UpdatedAtUtc = DateTimeOffset.UtcNow; link.UpdatedByUserId = CurrentUserId() ?? Guid.Empty;
        DigitalAudit(db, link, "BookDigitalPolicyChanged", link.UpdatedByUserId);
        try { await db.SaveChangesAsync(ct); } catch (DbUpdateConcurrencyException) { return DigitalFailure(409, "books.changed", "Reload and retry."); }
        return Ok(ApiResponse<DigitalBookResponse>.Success(DigitalResponse(link)));
    }

    [HttpGet("book-versions/{id:guid}/digital/download")]
    public async Task<IActionResult> DownloadDigitalBook(Guid id, [FromServices] ISchoolDocumentStorage storage, CancellationToken ct)
    {
        if (!Can("view")) return Forbid();
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var link = await DigitalQuery(db, id).SingleOrDefaultAsync(ct); if (link is null) return Missing();
        if (!link.AllowDownload) return Forbid();
        var stream = await storage.OpenReadAsync(link.Version.StorageKey, ct); if (stream is null) return Missing();
        try { DigitalAudit(db, link, "BookDigitalDownloaded", CurrentUserId() ?? Guid.Empty); await db.SaveChangesAsync(ct); }
        catch { await stream.DisposeAsync(); throw; }
        DigitalHeaders(); return File(stream, "application/pdf", link.Version.OriginalFileName, false);
    }

    [HttpGet("book-versions/{id:guid}/digital/pages/{page:int}")]
    public async Task<IActionResult> DigitalBookPage(Guid id, int page, [FromQuery] int revision, [FromServices] ISchoolDocumentStorage storage,
        [FromServices] IBookPdfRenderer renderer, CancellationToken ct)
    {
        if (!Can("view")) return Forbid();
        await using var db = await RequireDb(ct); if (db is null) return Unauthorized();
        var link = await DigitalQuery(db, id).AsNoTracking().SingleOrDefaultAsync(ct); if (link is null) return Missing();
        if (revision != link.Revision) return DigitalFailure(409, "books.changed", "Reload the reader.");
        if (page < 1 || page > link.PageCount) return Missing();
        await using var stream = await storage.OpenReadAsync(link.Version.StorageKey, ct); if (stream is null) return Missing();
        byte[] png;
        var scope = $"{User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value} {CurrentUserId():N} {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm}";
        try { png = await renderer.RenderAsync(stream, page, scope, ct); }
        catch (System.ComponentModel.Win32Exception) { return DigitalFailure(503, "books.renderer_unavailable", "Book reader is not configured."); }
        catch (InvalidDataException) { return DigitalFailure(422, "books.render_failed", "This page cannot be displayed."); }
        // Recheck after rendering: a revoked or replaced edition must not complete an old request.
        if (!await DigitalQuery(db, id).AnyAsync(x => x.Revision == revision, ct)) return DigitalFailure(409, "books.changed", "Reload the reader.");
        DigitalHeaders(); return File(png, "image/png");
    }
    private void DigitalHeaders() { Response.Headers.CacheControl = "private, no-store"; Response.Headers["X-Content-Type-Options"] = "nosniff"; }
    private IActionResult DigitalFailure(int code, string key, string message) => StatusCode(code, ApiResponse<object?>.Failure(code, key, message));
    private static IQueryable<BookVersionDigitalDocument> DigitalQuery(Mdaresna.Schools.Infrastructure.Persistence.SchoolsDbContext db, Guid id) =>
        db.BookVersionDigitalDocuments.Include(x => x.Version).ThenInclude(x => x.Document).Where(x => x.BookVersionId == id && x.BookVersion.IsActive && !x.BookVersion.IsDeleted && x.BookVersion.Book.IsActive && !x.BookVersion.Book.IsDeleted && x.Version.Document.Status == SchoolDocumentStatus.Active && x.Version.ValidationStatus != SchoolDocumentValidationStatus.Rejected);
    private static DigitalBookResponse DigitalResponse(BookVersionDigitalDocument link) => new(link.Version.OriginalFileName, link.Version.SizeBytes, link.PageCount, link.ShareWithFamily, link.AllowDownload, link.Revision, link.UpdatedAtUtc);
    private static void DigitalAudit(Mdaresna.Schools.Infrastructure.Persistence.SchoolsDbContext db, BookVersionDigitalDocument link, string action, Guid actor) => db.DocumentAudits.Add(new() { Id = Guid.NewGuid(), DocumentId = link.Version.DocumentId, Action = action, ActorUserId = actor, CreatedAtUtc = DateTimeOffset.UtcNow, Details = $"bookVersion={link.BookVersionId};revision={link.Revision};family={link.ShareWithFamily};download={link.AllowDownload}" });
}
public sealed record DigitalBookResponse(string FileName, long SizeBytes, int PageCount, bool ShareWithFamily, bool AllowDownload, int Revision, DateTimeOffset UpdatedAtUtc);
public sealed record DigitalBookPolicy(int ExpectedRevision, bool ShareWithFamily, bool AllowDownload);
