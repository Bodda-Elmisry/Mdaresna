using System.ComponentModel.DataAnnotations;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Application.Registry.BeginSchoolProvisioning;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using Mdaresna.Platform.Application.Registry.Read;
using Mdaresna.Platform.Application.Registry.RegisterSchool;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.Api.Controllers.Registry;

[ApiController]
[Route("api/platform/v1")]
public sealed class SchoolsController(
    RegistryReadService readService,
    RegisterSchoolCommandHandler registerSchool,
    TransitionSchoolCommandHandler transitionSchool,
    BeginSchoolProvisioningCommandHandler beginProvisioning,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet("schools")]
    [PlatformPermission("platform.schools.read")]
    public Task<IActionResult> List(
        [FromQuery] string? search = null,
        [FromQuery] SchoolLifecycleStatus? status = null,
        [FromQuery] SchoolType? schoolType = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ListCore(null, search, status, schoolType, pageNumber, pageSize, cancellationToken);

    [HttpGet("tenants/{tenantId:guid}/schools")]
    [PlatformPermission("platform.schools.read")]
    public Task<IActionResult> ListTenantSchools(
        Guid tenantId,
        [FromQuery] string? search = null,
        [FromQuery] SchoolLifecycleStatus? status = null,
        [FromQuery] SchoolType? schoolType = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        RegistryRequestContext.RequireId(tenantId, nameof(tenantId));
        return ListCore(TenantId.From(tenantId), search, status, schoolType,
            pageNumber, pageSize, cancellationToken);
    }

    [HttpGet("schools/{schoolId:guid}")]
    [PlatformPermission("platform.schools.read")]
    public async Task<IActionResult> Get(
        Guid schoolId,
        CancellationToken cancellationToken)
    {
        RegistryRequestContext.RequireId(schoolId, nameof(schoolId));
        var school = await readService.GetSchoolAsync(SchoolId.From(schoolId), cancellationToken);
        return Ok(ApiResponse<SchoolReadModel>.Success(
            school,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpGet("tenants/{tenantId:guid}/schools/{schoolId:guid}")]
    [PlatformPermission("platform.schools.read")]
    public async Task<IActionResult> GetTenantSchool(
        Guid tenantId,
        Guid schoolId,
        CancellationToken cancellationToken)
    {
        RegistryRequestContext.RequireId(tenantId, nameof(tenantId));
        RegistryRequestContext.RequireId(schoolId, nameof(schoolId));
        var school = await readService.GetSchoolAsync(SchoolId.From(schoolId), cancellationToken);
        if (school.TenantId != TenantId.From(tenantId))
        {
            throw new PlatformResourceNotFoundException(
                "school.not_found",
                "School was not found in this tenant.");
        }

        return Ok(ApiResponse<SchoolReadModel>.Success(
            school,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPost("tenants/{tenantId:guid}/schools")]
    [PlatformPermission("platform.schools.manage")]
    public async Task<IActionResult> Register(
        Guid tenantId,
        [FromBody] RegisterSchoolRequest request,
        CancellationToken cancellationToken)
    {
        RegistryRequestContext.RequireId(tenantId, nameof(tenantId));
        RegistryRequestContext.RequireId(
            request.RegistrationRequestId, nameof(request.RegistrationRequestId));
        if (string.IsNullOrWhiteSpace(request.SchoolCode) ||
            string.IsNullOrWhiteSpace(request.DisplayName) ||
            request.DisplayName.Trim().Length > 200 ||
            !Enum.IsDefined(request.SchoolType) ||
            !Enum.IsDefined(request.DeploymentMode))
        {
            throw new ValidationException("School registration data is invalid.");
        }

        try
        {
            SchoolCode.Create(request.SchoolCode);
        }
        catch (ArgumentException exception)
        {
            throw new ValidationException(exception.Message, exception);
        }

        var result = await registerSchool.HandleAsync(new RegisterSchoolCommand(
            request.RegistrationRequestId,
            TenantId.From(tenantId),
            request.SchoolCode,
            request.DisplayName,
            request.SchoolType,
            request.DeploymentMode,
            RegistryRequestContext.Actor(HttpContext),
            RegistryRequestContext.CorrelationId(HttpContext),
            TraceParent: RegistryRequestContext.TraceParent), cancellationToken);
        var response = ApiResponse<RegisterSchoolResult>.Success(
            result,
            statusCode: result.WasCreated ? StatusCodes.Status201Created : StatusCodes.Status200OK,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext));
        return result.WasCreated
            ? CreatedAtAction(nameof(GetTenantSchool), new
            {
                tenantId = result.TenantId.Value,
                schoolId = result.SchoolId.Value
            }, response)
            : Ok(response);
    }

    [HttpPost("tenants/{tenantId:guid}/schools/{schoolId:guid}/submit-for-verification")]
    [PlatformPermission("platform.schools.manage")]
    public Task<IActionResult> SubmitForVerification(
        Guid tenantId, Guid schoolId, [FromBody] TransitionSchoolRequest request,
        CancellationToken cancellationToken) =>
        Transition(tenantId, schoolId, SchoolLifecycleAction.SubmitForVerification,
            request, cancellationToken);

    [HttpPost("tenants/{tenantId:guid}/schools/{schoolId:guid}/approve")]
    [PlatformPermission("platform.schools.activate")]
    public Task<IActionResult> Approve(
        Guid tenantId, Guid schoolId, [FromBody] TransitionSchoolRequest request,
        CancellationToken cancellationToken) =>
        Transition(tenantId, schoolId, SchoolLifecycleAction.Approve, request, cancellationToken);

    [HttpPost("tenants/{tenantId:guid}/schools/{schoolId:guid}/suspend")]
    [PlatformPermission("platform.schools.activate")]
    public Task<IActionResult> Suspend(
        Guid tenantId, Guid schoolId, [FromBody] TransitionSchoolRequest request,
        CancellationToken cancellationToken) =>
        Transition(tenantId, schoolId, SchoolLifecycleAction.Suspend, request, cancellationToken);

    [HttpPost("tenants/{tenantId:guid}/schools/{schoolId:guid}/reinstate")]
    [PlatformPermission("platform.schools.activate")]
    public Task<IActionResult> Reinstate(
        Guid tenantId, Guid schoolId, [FromBody] TransitionSchoolRequest request,
        CancellationToken cancellationToken) =>
        Transition(tenantId, schoolId, SchoolLifecycleAction.Reinstate, request, cancellationToken);

    [HttpPost("tenants/{tenantId:guid}/schools/{schoolId:guid}/close")]
    [PlatformPermission("platform.schools.activate")]
    public Task<IActionResult> Close(
        Guid tenantId, Guid schoolId, [FromBody] TransitionSchoolRequest request,
        CancellationToken cancellationToken) =>
        Transition(tenantId, schoolId, SchoolLifecycleAction.Close, request, cancellationToken);

    [HttpPost("tenants/{tenantId:guid}/schools/{schoolId:guid}/provisioning")]
    [PlatformPermission("platform.deployments.manage")]
    public async Task<IActionResult> BeginProvisioning(
        Guid tenantId,
        Guid schoolId,
        [FromBody] BeginProvisioningRequest request,
        CancellationToken cancellationToken)
    {
        if (!configuration.GetValue<bool>("PlatformFeatures:SchoolProvisioningEnabled"))
        {
            return StatusCode(StatusCodes.Status503ServiceUnavailable,
                ApiResponse<object?>.Failure(
                    StatusCodes.Status503ServiceUnavailable,
                    "provisioning.not_configured",
                    "School provisioning is not configured for this environment.",
                    correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
        }

        RegistryRequestContext.RequireId(tenantId, nameof(tenantId));
        RegistryRequestContext.RequireId(schoolId, nameof(schoolId));
        RegistryRequestContext.RequireId(request.OperationId, nameof(request.OperationId));
        await beginProvisioning.HandleAsync(new BeginSchoolProvisioningCommand(
            TenantId.From(tenantId),
            SchoolId.From(schoolId),
            request.OperationId,
            RegistryRequestContext.Actor(HttpContext),
            RegistryRequestContext.CorrelationId(HttpContext),
            TraceParent: RegistryRequestContext.TraceParent), cancellationToken);
        var school = await readService.GetSchoolAsync(SchoolId.From(schoolId), cancellationToken);
        var statusCode = school.Status == SchoolLifecycleStatus.Provisioning
            ? StatusCodes.Status202Accepted
            : StatusCodes.Status200OK;
        return StatusCode(statusCode,
            ApiResponse<SchoolReadModel>.Success(
                school,
                statusCode,
                statusCode == StatusCodes.Status202Accepted
                    ? "Provisioning request accepted."
                    : "Provisioning operation was already recorded.",
                ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    private async Task<IActionResult> ListCore(
        TenantId? tenantId,
        string? search,
        SchoolLifecycleStatus? status,
        SchoolType? schoolType,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        RegistryRequestContext.ValidateList(pageNumber, pageSize, search);
        if (status.HasValue && !Enum.IsDefined(status.Value) ||
            schoolType.HasValue && !Enum.IsDefined(schoolType.Value))
        {
            throw new ValidationException("School filters are invalid.");
        }

        var page = await readService.ListSchoolsAsync(
            new ListSchoolsQuery(tenantId, search, status, schoolType, pageNumber, pageSize),
            cancellationToken);
        return Ok(PagedApiResponse<SchoolReadModel>.Success(
            page.Items,
            page.TotalCount,
            page.PageNumber,
            page.PageSize,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    private async Task<IActionResult> Transition(
        Guid tenantId,
        Guid schoolId,
        SchoolLifecycleAction action,
        TransitionSchoolRequest request,
        CancellationToken cancellationToken)
    {
        RegistryRequestContext.RequireId(tenantId, nameof(tenantId));
        RegistryRequestContext.RequireId(schoolId, nameof(schoolId));
        if (request.ExpectedVersion is null or < 0 ||
            request.Reason?.Trim().Length > 1000)
        {
            throw new ValidationException("Lifecycle request data is invalid.");
        }

        if (action is SchoolLifecycleAction.Suspend or SchoolLifecycleAction.Close)
        {
            if (string.IsNullOrWhiteSpace(request.Reason))
            {
                throw new ValidationException("A reason is required for this action.");
            }
        }
        else if (!string.IsNullOrWhiteSpace(request.Reason))
        {
            throw new ValidationException("Reason is not accepted for this action.");
        }

        var result = await transitionSchool.HandleAsync(new TransitionSchoolCommand(
            TenantId.From(tenantId),
            SchoolId.From(schoolId),
            action,
            RegistryRequestContext.Actor(HttpContext),
            request.ExpectedVersion.Value,
            RegistryRequestContext.CorrelationId(HttpContext),
            request.Reason,
            TraceParent: RegistryRequestContext.TraceParent), cancellationToken);
        return Ok(ApiResponse<TransitionSchoolResult>.Success(
            result,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }
}

public sealed record RegisterSchoolRequest(
    Guid RegistrationRequestId,
    string SchoolCode,
    string DisplayName,
    SchoolType SchoolType,
    DeploymentMode DeploymentMode);

public sealed record TransitionSchoolRequest(long? ExpectedVersion, string? Reason = null);

public sealed record BeginProvisioningRequest(Guid OperationId);
