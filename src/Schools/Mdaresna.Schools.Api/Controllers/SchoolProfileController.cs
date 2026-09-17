using Mdaresna.Api.Contracts;
using Mdaresna.Schools.Api.Auth;
using Mdaresna.Schools.Infrastructure.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Schools.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/schools/v1/school-profile")]
public sealed class SchoolProfileController(ISchoolDbContextFactory dbFactory) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var schoolCode = User.FindFirst(SchoolClaimTypes.SchoolCode)?.Value ?? string.Empty;
        await using var db = await dbFactory.CreateAsync(schoolCode, cancellationToken);
        if (db is null) return Unauthorized();

        var school = await db.SchoolInformation.AsNoTracking()
            .Select(x => new SchoolProfileResponse(
                x.Id,
                x.PlatformSchoolReferenceId,
                x.Code,
                x.DisplayName,
                x.SchoolType,
                x.Status,
                x.Address,
                x.PrimaryPhone,
                x.UnitTypeCode,
                x.UnitTypeName,
                x.Currency,
                x.ActivatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

        return school is null
            ? NotFound(ApiResponse<object?>.Failure(404, "school.profile_not_found",
                "School information was not found.", correlationId: HttpContext.TraceIdentifier))
            : Ok(ApiResponse<SchoolProfileResponse>.Success(
                school, correlationId: HttpContext.TraceIdentifier));
    }
}

public sealed record SchoolProfileResponse(
    Guid Id,
    Guid PlatformSchoolReferenceId,
    string Code,
    string DisplayName,
    string SchoolType,
    string Status,
    string? Address,
    string? PrimaryPhone,
    string UnitTypeCode,
    string UnitTypeName,
    string Currency,
    DateTimeOffset ActivatedAtUtc);
