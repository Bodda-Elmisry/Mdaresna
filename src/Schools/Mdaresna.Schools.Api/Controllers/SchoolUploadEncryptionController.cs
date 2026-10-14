using System.Data;
using Mdaresna.Api.Contracts;
using Mdaresna.SharedKernel;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Api.Documents;
using Mdaresna.Schools.Infrastructure.Persistence;
using Mdaresna.Schools.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

/// <summary>Explicit maintenance operation, never automatic at startup. Take a backup and stop uploads before running batches.</summary>
[ApiController, Authorize, Route("api/schools/v1/uploads/encryption")]
public sealed class SchoolUploadEncryptionController(ISchoolDbContextFactory factory, ISchoolDocumentStorage storage, IConfiguration config) : ControllerBase
{
    [HttpPost("migrate")]
    public async Task<IActionResult> Migrate([FromQuery] int profileOffset = 0, CancellationToken ct = default)
    {
        if (!User.HasClaim(SchoolClaimTypes.Permission, "school.academics.manage") || !User.HasClaim(SchoolClaimTypes.Permission, "school.profile.manage")) return Forbid();
        if (profileOffset < 0) return BadRequest();
        var school = User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? "";
        await using var db = await factory.CreateAsync(school, ct); if (db is null) return Unauthorized();
        var cipher = UploadEncryptionConfiguration.Create(config); var files = 0; var images = 0;
        // Includes historical, deleted and rejected document versions: their bytes need encryption too.
        var versions = await db.DocumentVersions.Where(x => !x.StorageKey.EndsWith(".enc")).OrderBy(x => x.Id).Take(16).ToArrayAsync(ct);
        foreach (var version in versions)
        {
            var previous = version.StorageKey;
            await using var input = await storage.OpenReadAsync(previous, ct); if (input is null) return Conflict(new { code = "encryption.content_missing", documentVersionId = version.Id });
            var stored = await storage.SaveAsync(school, version.DocumentId, version.Id, input, Path.GetExtension(previous), ct);
            if (stored.Sha256 != version.Sha256 || stored.SizeBytes != version.SizeBytes) { await storage.DeleteAsync(stored.StorageKey, CancellationToken.None); return Conflict(new { code = "encryption.hash_mismatch", documentVersionId = version.Id }); }
            version.StorageKey = stored.StorageKey;
            db.DocumentAudits.Add(new() { Id = Guid.NewGuid(), DocumentId = version.DocumentId, Action = "StoredFileEncrypted", ActorUserId = Guid.TryParse(User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value, out var actor) ? actor : Guid.Empty, CreatedAtUtc = DateTimeOffset.UtcNow, Details = version.Id.ToString() });
            try { await db.SaveChangesAsync(ct); } catch { await storage.DeleteAsync(stored.StorageKey, CancellationToken.None); throw; }
            await input.DisposeAsync(); await storage.DeleteAsync(previous, ct); files++;
        }
        var profiles = await db.PersonProfileImages.OrderBy(x => x.PersonId).Skip(profileOffset).Take(16).ToArrayAsync(ct);
        foreach (var image in profiles) if (!UploadedFileCipher.IsEncrypted(image.Content)) { image.Content = cipher.EncryptBytes(image.Content, $"schools/{school}/profile/{image.PersonId:N}"); images++; }
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Success(new { documentsEncrypted = files, profilesEncrypted = images, nextProfileOffset = profileOffset + profiles.Length, profilesComplete = profiles.Length < 16, remainingDocuments = await db.DocumentVersions.CountAsync(x => !x.StorageKey.EndsWith(".enc"), ct) }));
    }
}
