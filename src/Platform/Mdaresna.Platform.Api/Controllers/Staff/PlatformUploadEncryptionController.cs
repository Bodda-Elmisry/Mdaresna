using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.SharedKernel;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace Mdaresna.Platform.Api.Controllers.Staff;
[ApiController, PlatformPermission("platform.access.manage"), Route("api/platform/v1/uploads/encryption")]
public sealed class PlatformUploadEncryptionController(IdentityDbContext db, IConfiguration config) : ControllerBase
{
    [HttpPost("migrate")]
    public async Task<IActionResult> Migrate([FromQuery] int offset = 0, CancellationToken ct = default)
    {
        if (offset < 0) return BadRequest();
        var cipher = UploadEncryptionConfiguration.Create(config); var count = 0;
        var images = await db.AccountProfileImages.OrderBy(x => x.AccountId).Skip(offset).Take(16).ToArrayAsync(ct);
        foreach (var image in images) if (!UploadedFileCipher.IsEncrypted(image.Content)) { image.Content = cipher.EncryptBytes(image.Content, $"platform/profile/{image.AccountId:N}"); count++; }
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Success(new { profilesEncrypted = count, nextOffset = offset + images.Length, complete = images.Length < 16 }));
    }
}
