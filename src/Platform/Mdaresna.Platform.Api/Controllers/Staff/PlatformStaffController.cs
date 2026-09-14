using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Application.Access.Staff;
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.Api.Controllers.Staff;

[ApiController]
[Route("api/platform/v1/staff")]
[PlatformPermission("platform.access.manage")]
public sealed class PlatformStaffController(
    IPlatformStaffDirectory directory,
    IPlatformStaffRoleManager roles,
    IPlatformRoleCatalog roleCatalog,
    IPlatformStaffManagement management,
    IdentityDbContext identityDb,
    PlatformDbContext platformDb) : ControllerBase
{
    [HttpGet("roles")]
    public async Task<IActionResult> ListRoles(
        [FromQuery] bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var catalog = await roleCatalog.ListAsync(includeInactive, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PlatformRoleCatalogItem>>.Success(
            catalog, correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] Guid? roleId = null,
        CancellationToken cancellationToken = default)
    {
        var page = await directory.ListAsync(pageNumber, pageSize, cancellationToken, search, roleId);
        return Ok(PagedApiResponse<PlatformStaffDirectoryItem>.Success(
            page.Items, page.TotalCount, page.PageNumber, page.PageSize,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpGet("lookup")]
    public async Task<IActionResult> Lookup([FromQuery] string phone, CancellationToken cancellationToken)
    {
        var result = await management.LookupAsync(CurrentAccountId(), phone, cancellationToken);
        return Ok(ApiResponse<PlatformStaffLookupResult>.Success(result,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPost("invitations")]
    public async Task<IActionResult> Invite([FromBody] InvitePlatformStaffRequest request,
        CancellationToken cancellationToken)
    {
        var accountId = await management.InviteAsync(CurrentAccountId(), request, cancellationToken,
            ApiResponseWriter.GetCorrelationId(HttpContext));
        return StatusCode(201, ApiResponse<object>.Success(new { accountId }, statusCode: 201,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPut("{accountId:guid}")]
    public async Task<IActionResult> Update(Guid accountId, [FromBody] UpdatePlatformStaffRequest request,
        CancellationToken cancellationToken)
    {
        await management.UpdateAsync(CurrentAccountId(), accountId, request, cancellationToken,
            ApiResponseWriter.GetCorrelationId(HttpContext));
        return Ok(ApiResponse<object?>.Success(null,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPut("{accountId:guid}/active")]
    public async Task<IActionResult> SetActive(Guid accountId, [FromBody] SetStaffActiveRequest request,
        CancellationToken cancellationToken)
    {
        await management.SetActiveAsync(CurrentAccountId(), accountId, request.IsActive, cancellationToken,
            ApiResponseWriter.GetCorrelationId(HttpContext));
        return Ok(ApiResponse<object?>.Success(null,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpGet("{accountId:guid}/image")]
    public async Task<IActionResult> Image(Guid accountId, CancellationToken cancellationToken)
    {
        if (!await platformDb.LocalUsers.AsNoTracking().AnyAsync(x => x.PersonId == accountId, cancellationToken) ||
            !await identityDb.AccountProfileImages.AsNoTracking().AnyAsync(x => x.AccountId == accountId, cancellationToken))
            return NotFound();
        var image = await identityDb.AccountProfileImages.AsNoTracking()
            .Where(x => x.AccountId == accountId)
            .Select(x => new { x.Content, x.ContentType })
            .SingleAsync(cancellationToken);
        Response.Headers.CacheControl = "private, no-store";
        Response.Headers["X-Content-Type-Options"] = "nosniff";
        return File(image.Content, image.ContentType);
    }

    [HttpPost("{accountId:guid}/roles")]
    public async Task<IActionResult> AssignRole(
        Guid accountId,
        [FromBody] AssignStaffRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await roles.AssignAsync(
            CurrentAccountId(), accountId, request.RoleId, cancellationToken,
            ApiResponseWriter.GetCorrelationId(HttpContext));
        var response = ApiResponse<PlatformStaffRoleAssignmentResult>.Success(
            result,
            statusCode: result.Changed ? 201 : 200,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext));
        return StatusCode(response.StatusCode, response);
    }

    [HttpDelete("roles/{assignmentId:guid}")]
    public async Task<IActionResult> RevokeRole(
        Guid assignmentId,
        CancellationToken cancellationToken)
    {
        var result = await roles.RevokeAsync(
            CurrentAccountId(), assignmentId, cancellationToken,
            ApiResponseWriter.GetCorrelationId(HttpContext));
        return Ok(ApiResponse<PlatformStaffRoleAssignmentResult>.Success(
            result,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    private Guid CurrentAccountId() =>
        Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var accountId) &&
        accountId != Guid.Empty
            ? accountId
            : throw new InvalidOperationException("A validated Platform account is required.");
}

public sealed record AssignStaffRoleRequest(Guid RoleId);
public sealed record SetStaffActiveRequest(bool IsActive);
