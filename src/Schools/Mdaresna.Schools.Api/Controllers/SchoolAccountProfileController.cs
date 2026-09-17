using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Infrastructure.Identity;
using Mdaresna.Schools.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/schools/v1/me/profile")]
public sealed class SchoolAccountProfileController(ISchoolDbContextFactory dbFactory) : ControllerBase
{
    private const int MaximumImageBytes = 2 * 1024 * 1024;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken);
        if (db is null) return Unauthorized();
        var personId = CurrentPersonId();
        var person = await db.Persons.AsNoTracking().Where(x => x.Id == personId)
            .Select(x => new { x.DisplayName, x.DateOfBirth, x.GenderCode }).SingleOrDefaultAsync(cancellationToken);
        if (person is null) return NotFoundResponse();
        var imageUpdatedAtUtc = await db.PersonProfileImages.AsNoTracking().Where(x => x.PersonId == personId)
            .Select(x => (DateTimeOffset?)x.UpdatedAtUtc).SingleOrDefaultAsync(cancellationToken);
        return Ok(ApiResponse<SchoolPersonProfileResponse>.Success(new(person.DisplayName, person.DateOfBirth,
            person.GenderCode, imageUpdatedAtUtc is not null, imageUpdatedAtUtc), correlationId: HttpContext.TraceIdentifier));
    }

    [HttpPut]
    public async Task<IActionResult> Update([FromBody] UpdateSchoolPersonProfileRequest request,
        CancellationToken cancellationToken)
    {
        var displayName = request.DisplayName?.Trim();
        var gender = request.GenderCode?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(displayName) || displayName.Length > 200 ||
            gender is not null and not ("male" or "female") || request.DateOfBirth is { } birthDate &&
            (birthDate < new DateOnly(1900, 1, 1) || birthDate > DateOnly.FromDateTime(DateTime.UtcNow)))
            return InvalidProfile();
        await using var db = await RequireDb(cancellationToken);
        if (db is null) return Unauthorized();
        var person = await db.Persons.SingleOrDefaultAsync(x => x.Id == CurrentPersonId(), cancellationToken);
        if (person is null) return NotFoundResponse();
        person.DisplayName = displayName;
        person.DateOfBirth = request.DateOfBirth;
        person.GenderCode = gender;
        person.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        var imageUpdatedAtUtc = await db.PersonProfileImages.AsNoTracking().Where(x => x.PersonId == person.Id)
            .Select(x => (DateTimeOffset?)x.UpdatedAtUtc).SingleOrDefaultAsync(cancellationToken);
        return Ok(ApiResponse<SchoolPersonProfileResponse>.Success(new(person.DisplayName, person.DateOfBirth,
            person.GenderCode, imageUpdatedAtUtc is not null, imageUpdatedAtUtc), correlationId: HttpContext.TraceIdentifier));
    }

    [HttpGet("image")]
    public async Task<IActionResult> GetImage(CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken);
        if (db is null) return Unauthorized();
        var image = await db.PersonProfileImages.AsNoTracking().Where(x => x.PersonId == CurrentPersonId())
            .Select(x => new { x.Content, x.ContentType }).SingleOrDefaultAsync(cancellationToken);
        if (image is null) return NotFoundResponse();
        Response.Headers.CacheControl = "private, no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(image.Content, image.ContentType);
    }

    [HttpPost("image")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length is <= 0 or > MaximumImageBytes) return InvalidImage();
        await using var input = file.OpenReadStream();
        using var buffer = new MemoryStream();
        await input.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length is <= 0 or > MaximumImageBytes) return InvalidImage();
        var content = buffer.ToArray();
        var contentType = DetectImageType(content);
        if (contentType is null) return InvalidImage();

        await using var db = await RequireDb(cancellationToken);
        if (db is null) return Unauthorized();
        var personId = CurrentPersonId();
        if (!await db.Persons.AsNoTracking().AnyAsync(x => x.Id == personId, cancellationToken)) return NotFoundResponse();
        var image = await db.PersonProfileImages.SingleOrDefaultAsync(x => x.PersonId == personId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        if (image is null)
            db.PersonProfileImages.Add(new PersonProfileImage { PersonId = personId, Content = content,
                ContentType = contentType, UpdatedAtUtc = now });
        else
        {
            image.Content = content; image.ContentType = contentType; image.UpdatedAtUtc = now;
        }
        await db.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<SchoolProfileImageResponse>.Success(new(now), correlationId: HttpContext.TraceIdentifier));
    }

    [HttpDelete("image")]
    public async Task<IActionResult> DeleteImage(CancellationToken cancellationToken)
    {
        await using var db = await RequireDb(cancellationToken);
        if (db is null) return Unauthorized();
        var image = await db.PersonProfileImages.SingleOrDefaultAsync(x => x.PersonId == CurrentPersonId(), cancellationToken);
        if (image is not null) { db.PersonProfileImages.Remove(image); await db.SaveChangesAsync(cancellationToken); }
        return Ok(ApiResponse<object?>.Success(null, correlationId: HttpContext.TraceIdentifier));
    }

    private Task<SchoolsDbContext?> RequireDb(CancellationToken cancellationToken) =>
        dbFactory.CreateAsync(User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty, cancellationToken);
    private Guid CurrentPersonId() => Guid.TryParse(User.FindFirst(SchoolClaimTypes.PersonId)?.Value, out var id) && id != Guid.Empty
        ? id : throw new InvalidOperationException("A validated school person is required.");
    private IActionResult InvalidProfile() => BadRequest(ApiResponse<object?>.Failure(400, "profile.invalid",
        "Enter a valid display name, birth date and gender.", correlationId: HttpContext.TraceIdentifier));
    private IActionResult InvalidImage() => BadRequest(ApiResponse<object?>.Failure(400, "profile.image_invalid",
        "Upload a JPEG, PNG, or WebP image of up to 2 MB.", correlationId: HttpContext.TraceIdentifier));
    private IActionResult NotFoundResponse() => NotFound(ApiResponse<object?>.Failure(404, "profile.not_found",
        "Profile was not found.", correlationId: HttpContext.TraceIdentifier));

    private static string? DetectImageType(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF) return "image/jpeg";
        if (bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A })) return "image/png";
        if (bytes.Length >= 12 && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) && bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8)) return "image/webp";
        return null;
    }
}

public sealed record UpdateSchoolPersonProfileRequest(string DisplayName, DateOnly? DateOfBirth, string? GenderCode);
public sealed record SchoolPersonProfileResponse(string DisplayName, DateOnly? DateOfBirth, string? GenderCode,
    bool HasImage, DateTimeOffset? ImageUpdatedAtUtc);
public sealed record SchoolProfileImageResponse(DateTimeOffset ImageUpdatedAtUtc);
