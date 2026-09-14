using System.IdentityModel.Tokens.Jwt;
using System.ComponentModel.DataAnnotations;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Application.Access.Staff;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.Api.Controllers.Staff;

[ApiController]
[Route("api/platform/v1/roles")]
[PlatformPermission("platform.access.manage")]
public sealed class PlatformRolesController(IPlatformRoleManager manager, IPlatformRoleCatalog catalog) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var roles = await catalog.ListAsync(true, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PlatformRoleCatalogItem>>.Success(
            roles, correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpGet("permissions")]
    public async Task<IActionResult> Permissions(CancellationToken cancellationToken)
    {
        var codes = await manager.ListPermissionsAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<string>>.Success(
            codes, correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] SaveRoleRequest request, CancellationToken cancellationToken)
    {
        var result = await manager.CreateAsync(CurrentAccountId(), request.ToCommand(), cancellationToken,
            ApiResponseWriter.GetCorrelationId(HttpContext));
        return StatusCode(201, ApiResponse<PlatformRoleCatalogItem>.Success(result, statusCode: 201,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPut("{roleId:guid}")]
    public async Task<IActionResult> Update(Guid roleId, [FromBody] SaveRoleRequest request,
        CancellationToken cancellationToken)
    {
        var result = await manager.UpdateAsync(CurrentAccountId(), roleId, request.ToCommand(), cancellationToken,
            ApiResponseWriter.GetCorrelationId(HttpContext));
        return Ok(ApiResponse<PlatformRoleCatalogItem>.Success(result,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPut("{roleId:guid}/active")]
    public async Task<IActionResult> SetActive(Guid roleId, [FromBody] SetRoleActiveRequest request,
        CancellationToken cancellationToken)
    {
        var result = await manager.SetActiveAsync(CurrentAccountId(), roleId, request.IsActive, cancellationToken,
            ApiResponseWriter.GetCorrelationId(HttpContext));
        return Ok(ApiResponse<PlatformRoleCatalogItem>.Success(result,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpDelete("{roleId:guid}")]
    public async Task<IActionResult> Delete(Guid roleId, CancellationToken cancellationToken)
    {
        await manager.DeleteAsync(CurrentAccountId(), roleId, cancellationToken,
            ApiResponseWriter.GetCorrelationId(HttpContext));
        return Ok(ApiResponse<object?>.Success(null,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    private Guid CurrentAccountId() =>
        Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var accountId) && accountId != Guid.Empty
            ? accountId
            : throw new InvalidOperationException("A validated Platform account is required.");
}

public sealed record SaveRoleRequest(
    [Required, MaxLength(64)] string Key,
    [Required, MaxLength(100)] string DisplayName,
    [Required] IReadOnlyList<string> PermissionCodes)
{
    public PlatformRoleSaveRequest ToCommand() => new(Key, DisplayName, PermissionCodes);
}

public sealed record SetRoleActiveRequest(bool IsActive);
