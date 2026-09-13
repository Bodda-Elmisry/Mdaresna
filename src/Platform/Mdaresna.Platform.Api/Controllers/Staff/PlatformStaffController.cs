using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Application.Access.Staff;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.Api.Controllers.Staff;

[ApiController]
[Route("api/platform/v1/staff")]
[PlatformPermission("platform.access.manage")]
public sealed class PlatformStaffController(
    IPlatformStaffDirectory directory,
    IPlatformStaffRoleManager roles,
    IPlatformRoleCatalog roleCatalog) : ControllerBase
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
        CancellationToken cancellationToken = default)
    {
        var page = await directory.ListAsync(pageNumber, pageSize, cancellationToken);
        return Ok(PagedApiResponse<PlatformStaffDirectoryItem>.Success(
            page.Items, page.TotalCount, page.PageNumber, page.PageSize,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
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
