using System.ComponentModel.DataAnnotations;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Application.Registry.CreateTenant;
using Mdaresna.Platform.Application.Registry.Read;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.Api.Controllers.Registry;

[ApiController]
[Route("api/platform/v1/tenants")]
public sealed class TenantsController(
    RegistryReadService readService,
    CreateTenantCommandHandler createTenant) : ControllerBase
{
    [HttpGet]
    [PlatformPermission("platform.schools.read")]
    public async Task<IActionResult> List(
        [FromQuery] string? search = null,
        [FromQuery] TenantStatus? status = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        RegistryRequestContext.ValidateList(pageNumber, pageSize, search);
        if (status.HasValue && !Enum.IsDefined(status.Value))
        {
            throw new ValidationException("Tenant status is invalid.");
        }

        var page = await readService.ListTenantsAsync(
            new ListTenantsQuery(search, status, pageNumber, pageSize),
            cancellationToken);
        return Ok(PagedApiResponse<TenantReadModel>.Success(
            page.Items,
            page.TotalCount,
            page.PageNumber,
            page.PageSize,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpGet("{tenantId:guid}")]
    [PlatformPermission("platform.schools.read")]
    public async Task<IActionResult> Get(
        Guid tenantId,
        CancellationToken cancellationToken)
    {
        RegistryRequestContext.RequireId(tenantId, nameof(tenantId));
        var tenant = await readService.GetTenantAsync(TenantId.From(tenantId), cancellationToken);
        return Ok(ApiResponse<TenantReadModel>.Success(
            tenant,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPost]
    [PlatformPermission("platform.schools.manage")]
    public async Task<IActionResult> Create(
        [FromBody] CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        RegistryRequestContext.RequireId(request.TenantId, nameof(request.TenantId));
        if (string.IsNullOrWhiteSpace(request.DisplayName) ||
            request.DisplayName.Trim().Length > 200 ||
            request.LegalName?.Trim().Length > 250)
        {
            throw new ValidationException("Tenant name is required and must fit the allowed length.");
        }

        var result = await createTenant.HandleAsync(new CreateTenantCommand(
            TenantId.From(request.TenantId),
            request.DisplayName,
            request.LegalName,
            RegistryRequestContext.Actor(HttpContext),
            RegistryRequestContext.CorrelationId(HttpContext),
            TraceParent: RegistryRequestContext.TraceParent), cancellationToken);
        var response = ApiResponse<CreateTenantResult>.Success(
            result,
            statusCode: result.WasCreated ? StatusCodes.Status201Created : StatusCodes.Status200OK,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext));
        return result.WasCreated
            ? CreatedAtAction(nameof(Get), new { tenantId = result.TenantId.Value }, response)
            : Ok(response);
    }
}

public sealed record CreateTenantRequest(Guid TenantId, string DisplayName, string? LegalName);
