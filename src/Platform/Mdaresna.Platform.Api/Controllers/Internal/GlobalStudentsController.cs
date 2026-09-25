using System.Security.Cryptography;
using System.Text;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Api.Controllers.Internal;

[ApiController, AllowAnonymous, PlatformInternalService]
[Route("api/platform/v1/internal/global-students")]
public sealed class GlobalStudentsController(PlatformDbContext db, IConfiguration configuration) : ControllerBase
{
    [HttpPost("resolve-or-create")]
    public async Task<IActionResult> ResolveOrCreate([FromBody] ResolveGlobalStudentRequest request, CancellationToken ct)
    {
        if (!Authorized()) return Unauthorized();
        if (request.PlatformSchoolId == Guid.Empty || string.IsNullOrWhiteSpace(request.FullName) ||
            request.FullName.Trim().Length > 200 || request.DateOfBirth > DateOnly.FromDateTime(DateTime.UtcNow))
            return BadRequest(ApiResponse<object?>.Failure(400, "students.global_invalid", "Enter valid student identity data.", correlationId: HttpContext.TraceIdentifier));
        var schoolExists = await db.Schools.AnyAsync(x => x.Id == SchoolId.From(request.PlatformSchoolId) && x.Status == SchoolLifecycleStatus.Active, ct);
        if (!schoolExists) return NotFound();
        var code = request.StudentCode?.Trim().ToUpperInvariant();
        var nationalId = Clean(request.NationalId, 40); var birthCertificate = Clean(request.BirthCertificateNumber, 80);
        GlobalStudentRegistry? student = null;
        if (!string.IsNullOrWhiteSpace(code)) student = await db.GlobalStudents.SingleOrDefaultAsync(x => x.StudentCode == code && x.IsActive, ct);
        if (student is null && nationalId is not null) student = await db.GlobalStudents.SingleOrDefaultAsync(x => x.NationalId == nationalId && x.IsActive, ct);
        if (student is null && birthCertificate is not null) student = await db.GlobalStudents.SingleOrDefaultAsync(x => x.BirthCertificateNumber == birthCertificate && x.IsActive, ct);
        if (!string.IsNullOrWhiteSpace(code) && student is null)
            return NotFound(ApiResponse<object?>.Failure(404, "students.code_not_found", "Student code was not found.", correlationId: HttpContext.TraceIdentifier));
        if (student is not null)
        {
            if (student.DateOfBirth != request.DateOfBirth)
                return Conflict(ApiResponse<object?>.Failure(409, "students.identity_mismatch", "Student identity does not match the global code.", correlationId: HttpContext.TraceIdentifier));
            return Ok(ApiResponse<GlobalStudentResponse>.Success(new(student.Id, student.StudentCode), correlationId: HttpContext.TraceIdentifier));
        }
        var now = DateTimeOffset.UtcNow;
        for (var attempt = 0; attempt < 5; attempt++)
        {
            student = new GlobalStudentRegistry { Id = Guid.NewGuid(), StudentCode = GenerateCode(), FullName = request.FullName.Trim(),
                NormalizedName = request.FullName.Trim().ToUpperInvariant(), DateOfBirth = request.DateOfBirth,
                NationalId = nationalId, BirthCertificateNumber = birthCertificate, CreatedAtUtc = now, UpdatedAtUtc = now };
            db.GlobalStudents.Add(student);
            try { await db.SaveChangesAsync(ct); return Ok(ApiResponse<GlobalStudentResponse>.Success(new(student.Id, student.StudentCode), correlationId: HttpContext.TraceIdentifier)); }
            catch (DbUpdateException) when (attempt < 4) { db.Entry(student).State = EntityState.Detached; }
        }
        return Conflict(ApiResponse<object?>.Failure(409, "students.global_conflict", "Unable to allocate a student code.", correlationId: HttpContext.TraceIdentifier));
    }

    private bool Authorized()
    {
        var configured = configuration["InternalServices:ApiKey"]; var supplied = Request.Headers["X-Mdaresna-Internal-Key"].ToString();
        if (string.IsNullOrWhiteSpace(configured)) return false;
        var left = Encoding.UTF8.GetBytes(configured); var right = Encoding.UTF8.GetBytes(supplied);
        return left.Length == right.Length && CryptographicOperations.FixedTimeEquals(left, right);
    }
    private static string GenerateCode() => $"STU-{Convert.ToHexString(RandomNumberGenerator.GetBytes(6))}";
    private static string? Clean(string? value, int max) => string.IsNullOrWhiteSpace(value) ? null : value.Trim().Length <= max ? value.Trim() : null;
}

public sealed record ResolveGlobalStudentRequest(Guid PlatformSchoolId, string? StudentCode, string FullName,
    DateOnly DateOfBirth, string? NationalId, string? BirthCertificateNumber);
public sealed record GlobalStudentResponse(Guid GlobalStudentId, string StudentCode);
