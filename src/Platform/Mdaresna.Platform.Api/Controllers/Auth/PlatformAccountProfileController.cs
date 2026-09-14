using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Api.Controllers.Auth;

/// <summary>The person profile is owned by the shared Identity database, not a school or Platform tenant.</summary>
[ApiController]
[Authorize]
[Route("api/platform/v1/me/profile")]
public sealed class PlatformAccountProfileController(IdentityDbContext identityDb) : ControllerBase
{
    private const int MaximumImageBytes = 2 * 1024 * 1024;

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var accountId = CurrentAccountId();
        var account = await identityDb.Accounts.AsNoTracking()
            .Where(x => x.Id == accountId)
            .Select(x => new { x.DisplayName, x.DateOfBirth, x.GenderCode })
            .SingleOrDefaultAsync(cancellationToken);
        if (account is null) return NotFoundResponse();

        var imageUpdatedAtUtc = await identityDb.AccountProfileImages.AsNoTracking()
            .Where(x => x.AccountId == accountId)
            .Select(x => (DateTimeOffset?)x.UpdatedAtUtc)
            .SingleOrDefaultAsync(cancellationToken);
        return Ok(ApiResponse<PersonProfileResponse>.Success(
            new(account.DisplayName, account.DateOfBirth, account.GenderCode,
                imageUpdatedAtUtc is not null, imageUpdatedAtUtc),
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPut]
    public async Task<IActionResult> Update(
        [FromBody] UpdatePersonProfileRequest request, CancellationToken cancellationToken)
    {
        var gender = request.GenderCode?.Trim().ToLowerInvariant();
        if (gender is not null and not ("male" or "female") ||
            request.DateOfBirth is { } birthDate &&
            (birthDate < new DateOnly(1900, 1, 1) || birthDate > DateOnly.FromDateTime(DateTime.UtcNow)))
        {
            return BadRequest(ApiResponse<object?>.Failure(400, "profile.invalid",
                "Enter a valid birth date and gender.",
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
        }

        var accountId = CurrentAccountId();
        var account = await identityDb.Accounts.SingleOrDefaultAsync(
            x => x.Id == accountId, cancellationToken);
        if (account is null) return NotFoundResponse();

        account.DateOfBirth = request.DateOfBirth;
        account.GenderCode = gender;
        account.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await identityDb.SaveChangesAsync(cancellationToken);

        var imageUpdatedAtUtc = await identityDb.AccountProfileImages.AsNoTracking()
            .Where(x => x.AccountId == accountId)
            .Select(x => (DateTimeOffset?)x.UpdatedAtUtc)
            .SingleOrDefaultAsync(cancellationToken);
        return Ok(ApiResponse<PersonProfileResponse>.Success(
            new(account.DisplayName, account.DateOfBirth, account.GenderCode,
                imageUpdatedAtUtc is not null, imageUpdatedAtUtc),
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpGet("image")]
    public async Task<IActionResult> GetImage(CancellationToken cancellationToken)
    {
        var accountId = CurrentAccountId();
        var image = await identityDb.AccountProfileImages.AsNoTracking()
            .Where(x => x.AccountId == accountId)
            .Select(x => new { x.Content, x.ContentType })
            .SingleOrDefaultAsync(cancellationToken);
        if (image is null) return NotFoundResponse();
        Response.Headers.CacheControl = "private, no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(image.Content, image.ContentType);
    }

    [HttpPost("image")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(3 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(
        IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length is <= 0 or > MaximumImageBytes)
            return InvalidImage();

        await using var stream = file.OpenReadStream();
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer, cancellationToken);
        if (buffer.Length is <= 0 or > MaximumImageBytes)
            return InvalidImage();
        var content = buffer.ToArray();
        var contentType = DetectImageType(content);
        if (contentType is null) return InvalidImage();

        var accountId = CurrentAccountId();
        if (!await identityDb.Accounts.AsNoTracking().AnyAsync(x => x.Id == accountId, cancellationToken))
            return NotFoundResponse();
        var image = await identityDb.AccountProfileImages.SingleOrDefaultAsync(
            x => x.AccountId == accountId, cancellationToken);
        var now = DateTimeOffset.UtcNow;
        if (image is null)
        {
            identityDb.AccountProfileImages.Add(new AccountProfileImage
            {
                AccountId = accountId, Content = content, ContentType = contentType,
                UpdatedAtUtc = now
            });
        }
        else
        {
            image.Content = content;
            image.ContentType = contentType;
            image.UpdatedAtUtc = now;
        }
        await identityDb.SaveChangesAsync(cancellationToken);
        return Ok(ApiResponse<ProfileImageResponse>.Success(
            new(now), correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpDelete("image")]
    public async Task<IActionResult> DeleteImage(CancellationToken cancellationToken)
    {
        var image = await identityDb.AccountProfileImages.SingleOrDefaultAsync(
            x => x.AccountId == CurrentAccountId(), cancellationToken);
        if (image is not null)
        {
            identityDb.AccountProfileImages.Remove(image);
            await identityDb.SaveChangesAsync(cancellationToken);
        }
        return Ok(ApiResponse<object?>.Success(null,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    private IActionResult InvalidImage() => BadRequest(ApiResponse<object?>.Failure(
        400, "profile.image_invalid", "Upload a JPEG, PNG, or WebP image of up to 2 MB.",
        correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));

    private IActionResult NotFoundResponse() => NotFound(ApiResponse<object?>.Failure(
        404, "profile.not_found", "Profile was not found.",
        correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));

    private Guid CurrentAccountId() =>
        Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) && id != Guid.Empty
            ? id : throw new InvalidOperationException("A validated Platform account is required.");

    private static string? DetectImageType(byte[] bytes)
    {
        if (bytes.Length >= 3 && bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF)
            return "image/jpeg";
        if (bytes.Length >= 8 && bytes.AsSpan(0, 8).SequenceEqual(
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }))
            return "image/png";
        if (bytes.Length >= 12 && bytes.AsSpan(0, 4).SequenceEqual("RIFF"u8) &&
            bytes.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            return "image/webp";
        return null;
    }
}

public sealed record UpdatePersonProfileRequest(DateOnly? DateOfBirth, string? GenderCode);
public sealed record PersonProfileResponse(
    string? DisplayName, DateOnly? DateOfBirth, string? GenderCode,
    bool HasImage, DateTimeOffset? ImageUpdatedAtUtc);
public sealed record ProfileImageResponse(DateTimeOffset ImageUpdatedAtUtc);
