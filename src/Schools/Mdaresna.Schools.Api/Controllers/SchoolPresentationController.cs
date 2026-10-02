using System.IdentityModel.Tokens.Jwt;
using System.Net.Mail;
using System.Text.Json;
using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Documents;
using Mdaresna.Schools.Domain.Documents;
using Mdaresna.Schools.Domain.School;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController, Authorize, Route("api/schools/v1/school-profile/presentation")]
public sealed class SchoolPresentationController(ISchoolDbContextFactory factory, ISchoolDocumentStorage storage) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var school = await db.SchoolInformation.AsNoTracking().SingleOrDefaultAsync(ct);
        if (school is null) return NotFound(Failure("school.profile_not_found"));
        var profile = await db.SchoolPresentations.AsNoTracking().SingleOrDefaultAsync(ct) ?? new SchoolPresentation { SchoolInformationId = school.Id, Revision = 0 };
        var images = await db.SchoolProfileImages.AsNoTracking().OrderBy(x => x.SortOrder).ThenBy(x => x.Id)
            .Select(x => new PresentationImage(x.Id, x.IsLogo, x.CaptionAr, x.CaptionEn, x.SortOrder)).ToArrayAsync(ct);
        return Ok(ApiResponse<object>.Success(new {
            school.Id, school.DisplayName, school.Code, school.SchoolType, school.Status, school.UnitTypeName,
            address = profile.ContactAddress ?? school.Address, primaryPhone = profile.ContactPhone ?? school.PrimaryPhone,
            profile.TaglineAr, profile.TaglineEn, profile.AboutAr, profile.AboutEn, profile.VisionAr, profile.VisionEn,
            profile.MissionAr, profile.MissionEn, goals = JsonSerializer.Deserialize<PresentationGoal[]>(profile.GoalsJson),
            profile.Email, profile.Website, profile.CoverImageId, profile.CoverPosition, profile.Revision, images
        }, correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut, Authorize(Policy = SchoolPermissionPolicies.ProfileManage)]
    public async Task<IActionResult> Save(SavePresentation r, CancellationToken ct)
    {
        if (!Valid(r)) return BadRequest(Failure("school.presentation_invalid"));
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var school = await db.SchoolInformation.SingleOrDefaultAsync(ct);
        if (school is null) return NotFound(Failure("school.profile_not_found"));
        var profile = await db.SchoolPresentations.SingleOrDefaultAsync(ct);
        if ((profile?.Revision ?? 0) != r.Revision) return Conflict(Failure("school.presentation_conflict", 409));
        var images = await db.SchoolProfileImages.ToArrayAsync(ct);
        if (r.Images.Select(x => x.Id).Distinct().Count() != r.Images.Count ||
            !images.Select(x => x.Id).Order().SequenceEqual(r.Images.Select(x => x.Id).Order()) ||
            r.CoverImageId.HasValue && !images.Any(x => x.Id == r.CoverImageId && !x.IsLogo))
            return BadRequest(Failure("school.presentation_invalid"));
        profile ??= new SchoolPresentation { SchoolInformationId = school.Id, Revision = 0 };
        if (profile.Revision == 0) db.SchoolPresentations.Add(profile);
        profile.TaglineAr = r.TaglineAr.Trim(); profile.TaglineEn = r.TaglineEn.Trim();
        profile.AboutAr = r.AboutAr.Trim(); profile.AboutEn = r.AboutEn.Trim();
        profile.VisionAr = r.VisionAr.Trim(); profile.VisionEn = r.VisionEn.Trim();
        profile.MissionAr = r.MissionAr.Trim(); profile.MissionEn = r.MissionEn.Trim();
        profile.GoalsJson = JsonSerializer.Serialize(r.Goals.Select(x => new PresentationGoal(x.Ar.Trim(), x.En.Trim())));
        profile.ContactAddress = Clean(r.Address); profile.ContactPhone = Clean(r.PrimaryPhone);
        profile.Email = Clean(r.Email); profile.Website = Clean(r.Website);
        profile.CoverImageId = r.CoverImageId; profile.CoverPosition = r.CoverPosition;
        profile.Revision++; profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        foreach (var image in images) {
            var input = r.Images.Single(x => x.Id == image.Id);
            image.CaptionAr = input.CaptionAr.Trim(); image.CaptionEn = input.CaptionEn.Trim(); image.SortOrder = input.SortOrder;
            Audit(db, image.DocumentId, "SchoolPresentationUpdated");
        }
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return Conflict(Failure("school.presentation_conflict", 409)); }
        return Ok(ApiResponse<object>.Success(new { profile.Revision }));
    }

    [HttpPost("images"), Authorize(Policy = SchoolPermissionPolicies.ProfileManage), RequestSizeLimit(9_000_000)]
    public async Task<IActionResult> Upload(IFormFile file, [FromForm] bool isLogo = false, CancellationToken ct = default)
    {
        if (file.Length is < 1 or > 8_388_608) return BadRequest(Failure("school.image_invalid"));
        await using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, ct);
        var bytes = buffer.ToArray();
        var type = SchoolPresentationRules.ImageType(bytes);
        if (type is null) return BadRequest(Failure("school.image_invalid"));
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var school = await db.SchoolInformation.SingleOrDefaultAsync(ct);
        if (school is null) return NotFound(Failure("school.profile_not_found"));
        if (!isLogo && await db.SchoolProfileImages.CountAsync(x => !x.IsLogo, ct) >= 30)
            return BadRequest(Failure("school.gallery_limit"));
        var profile = await db.SchoolPresentations.SingleOrDefaultAsync(ct);
        profile ??= new SchoolPresentation { SchoolInformationId = school.Id, Revision = 0 };
        if (profile.Revision == 0) db.SchoolPresentations.Add(profile);
        var now = DateTimeOffset.UtcNow; var documentId = Guid.NewGuid(); var versionId = Guid.NewGuid();
        buffer.Position = 0;
        var stored = await storage.SaveAsync(Code(), documentId, versionId, buffer, type.Value.Extension, ct);
        var document = new SchoolDocument { Id = documentId, Title = (isLogo ? "School logo" : "School photo"),
            CreatedByUserId = Actor(), CreatedAtUtc = now, UpdatedAtUtc = now };
        document.Versions.Add(new SchoolDocumentVersion { Id = versionId, VersionNumber = 1,
            OriginalFileName = $"{versionId:N}{type.Value.Extension}", ContentType = type.Value.ContentType,
            SizeBytes = stored.SizeBytes, Sha256 = stored.Sha256, StorageKey = stored.StorageKey,
            ValidationStatus = SchoolDocumentValidationStatus.Pending, CreatedByUserId = Actor(), CreatedAtUtc = now });
        db.Documents.Add(document);
        if (isLogo) {
            var old = await db.SchoolProfileImages.Where(x => x.IsLogo).ToArrayAsync(ct);
            foreach (var item in old) Audit(db, item.DocumentId, "SchoolLogoReplaced");
            db.SchoolProfileImages.RemoveRange(old);
        }
        var image = new SchoolProfileImage { Id = Guid.NewGuid(), SchoolInformationId = school.Id, DocumentId = documentId,
            IsLogo = isLogo, SortOrder = (await db.SchoolProfileImages.MaxAsync(x => (int?)x.SortOrder, ct) ?? -1) + 1, CreatedAtUtc = now };
        db.SchoolProfileImages.Add(image); Audit(db, documentId, isLogo ? "SchoolLogoUploaded" : "SchoolPhotoUploaded");
        profile.Revision++; profile.UpdatedAtUtc = now;
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return Conflict(Failure("school.presentation_conflict", 409)); }
        return Ok(ApiResponse<object>.Success(new { image.Id, profile.Revision }));
    }

    [HttpDelete("images/{id:guid}"), Authorize(Policy = SchoolPermissionPolicies.ProfileManage)]
    public async Task<IActionResult> Remove(Guid id, CancellationToken ct)
    {
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var image = await db.SchoolProfileImages.SingleOrDefaultAsync(x => x.Id == id, ct);
        if (image is null) return NotFound(Failure("school.image_not_found"));
        var profile = await db.SchoolPresentations.SingleAsync(ct);
        if (profile.CoverImageId == id) profile.CoverImageId = null;
        profile.Revision++; profile.UpdatedAtUtc = DateTimeOffset.UtcNow;
        db.SchoolProfileImages.Remove(image); Audit(db, image.DocumentId, "SchoolImageRemoved");
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { return Conflict(Failure("school.presentation_conflict", 409)); }
        return Ok(ApiResponse<object?>.Success(null));
    }

    [HttpGet("images/{id:guid}/content")]
    public async Task<IActionResult> Content(Guid id, CancellationToken ct)
    {
        await using var db = await Db(ct); if (db is null) return Unauthorized();
        var documentId = await db.SchoolProfileImages.Where(x => x.Id == id).Select(x => (Guid?)x.DocumentId).SingleOrDefaultAsync(ct);
        if (!documentId.HasValue) return NotFound(Failure("school.image_not_found"));
        var version = await db.DocumentVersions.AsNoTracking().Where(x => x.DocumentId == documentId &&
            x.ValidationStatus != SchoolDocumentValidationStatus.Rejected).OrderByDescending(x => x.VersionNumber).FirstOrDefaultAsync(ct);
        if (version is null) return NotFound(Failure("school.image_not_found"));
        var stream = await storage.OpenReadAsync(version.StorageKey, ct);
        if (stream is null) return NotFound(Failure("school.image_not_found"));
        Response.Headers.CacheControl = "private, max-age=300";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(stream, version.ContentType);
    }

    private static bool Valid(SavePresentation r) =>
        r.TaglineAr is not null && r.TaglineEn is not null && r.AboutAr is not null && r.AboutEn is not null &&
        r.VisionAr is not null && r.VisionEn is not null && r.MissionAr is not null && r.MissionEn is not null &&
        r.Goals is not null && r.Images is not null &&
        r.Revision >= 0 && r.CoverPosition is >= 0 and <= 100 &&
        r.TaglineAr.Length <= 250 && r.TaglineEn.Length <= 250 &&
        r.AboutAr.Length <= 10000 && r.AboutEn.Length <= 10000 &&
        r.VisionAr.Length <= 5000 && r.VisionEn.Length <= 5000 &&
        r.MissionAr.Length <= 5000 && r.MissionEn.Length <= 5000 &&
        r.Goals.Count <= 30 && r.Goals.All(x => x is not null && x.Ar is not null && x.En is not null && x.Ar.Length <= 500 && x.En.Length <= 500 && (!string.IsNullOrWhiteSpace(x.Ar) || !string.IsNullOrWhiteSpace(x.En))) &&
        r.Images.Count <= 31 && r.Images.All(x => x is not null && x.CaptionAr is not null && x.CaptionEn is not null && x.CaptionAr.Length <= 500 && x.CaptionEn.Length <= 500 && x.SortOrder >= 0) &&
        (r.Address?.Length ?? 0) <= 500 && (r.PrimaryPhone?.Length ?? 0) <= 50 &&
        (string.IsNullOrWhiteSpace(r.Email) || r.Email.Length <= 250 && MailAddress.TryCreate(r.Email, out _)) &&
        (string.IsNullOrWhiteSpace(r.Website) || r.Website.Length <= 1000 && Uri.TryCreate(r.Website, UriKind.Absolute, out var uri) && uri.Scheme is "https" or "http");
    private Task<SchoolsDbContext?> Db(CancellationToken ct) => factory.CreateAsync(Code(), ct);
    private string Code() => User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? "";
    private Guid Actor() => Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ? id : Guid.Empty;
    private static string? Clean(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    private ApiResponse<object?> Failure(string code, int status = 400) => ApiResponse<object?>.Failure(status, code, code, correlationId: HttpContext.TraceIdentifier);
    private void Audit(SchoolsDbContext db, Guid documentId, string action) => db.DocumentAudits.Add(new SchoolDocumentAudit {
        Id = Guid.NewGuid(), DocumentId = documentId, Action = action, ActorUserId = Actor(), CreatedAtUtc = DateTimeOffset.UtcNow });
}

public sealed record PresentationGoal(string Ar, string En);
public sealed record PresentationImage(Guid Id, bool IsLogo, string CaptionAr, string CaptionEn, int SortOrder);
public sealed class SavePresentation
{
    public int Revision { get; init; }
    public string TaglineAr { get; init; } = "";
    public string TaglineEn { get; init; } = "";
    public string AboutAr { get; init; } = "";
    public string AboutEn { get; init; } = "";
    public string VisionAr { get; init; } = "";
    public string VisionEn { get; init; } = "";
    public string MissionAr { get; init; } = "";
    public string MissionEn { get; init; } = "";
    public IReadOnlyList<PresentationGoal> Goals { get; init; } = [];
    public string? Address { get; init; }
    public string? PrimaryPhone { get; init; }
    public string? Email { get; init; }
    public string? Website { get; init; }
    public Guid? CoverImageId { get; init; }
    public decimal CoverPosition { get; init; } = 50;
    public IReadOnlyList<PresentationImage> Images { get; init; } = [];
}
