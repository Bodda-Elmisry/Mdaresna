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
using Mdaresna.Platform.Infrastructure.Persistence.Identity;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.Api.Controllers.Registry;

[ApiController]
[Route("api/platform/v1")]
public sealed class SchoolsController(
    RegistryReadService readService,
    RegisterSchoolCommandHandler registerSchool,
    TransitionSchoolCommandHandler transitionSchool,
    BeginSchoolProvisioningCommandHandler beginProvisioning,
    IdentityDbContext identityDb,
    IConfiguration configuration) : ControllerBase
{
    [HttpGet("schools")]
    [PlatformPermission("platform.schools.read")]
    public Task<IActionResult> List(
        [FromQuery] string? search = null,
        [FromQuery] SchoolLifecycleStatus? status = null,
        [FromQuery] SchoolType? schoolType = null,
        [FromQuery] string? displayName = null,
        [FromQuery] string? address = null,
        [FromQuery] DateOnly? createdFrom = null,
        [FromQuery] DateOnly? createdTo = null,
        [FromQuery] DateOnly? activatedFrom = null,
        [FromQuery] DateOnly? activatedTo = null,
        [FromQuery] Guid? unitTypeId = null,
        [FromQuery] string? unitType = null,
        [FromQuery] string? owner = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default) =>
        ListCore(null, search, status, schoolType, displayName, address,
            createdFrom, createdTo, activatedFrom, activatedTo, unitTypeId,
            unitType, owner, pageNumber, pageSize, cancellationToken);

    [HttpGet("schools/summary")]
    [PlatformPermission("platform.schools.read")]
    public async Task<IActionResult> Summary(CancellationToken cancellationToken = default)
    {
        var summary = await readService.GetSchoolSummaryAsync(cancellationToken);
        return Ok(ApiResponse<SchoolDirectorySummary>.Success(
            summary, correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpGet("tenants/{tenantId:guid}/schools")]
    [PlatformPermission("platform.schools.read")]
    public Task<IActionResult> ListTenantSchools(
        Guid tenantId,
        [FromQuery] string? search = null,
        [FromQuery] SchoolLifecycleStatus? status = null,
        [FromQuery] SchoolType? schoolType = null,
        [FromQuery] string? displayName = null,
        [FromQuery] string? address = null,
        [FromQuery] DateOnly? createdFrom = null,
        [FromQuery] DateOnly? createdTo = null,
        [FromQuery] DateOnly? activatedFrom = null,
        [FromQuery] DateOnly? activatedTo = null,
        [FromQuery] Guid? unitTypeId = null,
        [FromQuery] string? unitType = null,
        [FromQuery] string? owner = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        RegistryRequestContext.RequireId(tenantId, nameof(tenantId));
        return ListCore(TenantId.From(tenantId), search, status, schoolType,
            displayName, address, createdFrom, createdTo, activatedFrom,
            activatedTo, unitTypeId, unitType, owner, pageNumber, pageSize, cancellationToken);
    }

    [HttpGet("schools/{schoolId:guid}")]
    [PlatformPermission("platform.schools.read")]
    public async Task<IActionResult> Get(
        Guid schoolId,
        CancellationToken cancellationToken)
    {
        RegistryRequestContext.RequireId(schoolId, nameof(schoolId));
        var school = await EnrichOwnerAsync(
            await readService.GetSchoolAsync(SchoolId.From(schoolId), cancellationToken), cancellationToken);
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
        var school = await EnrichOwnerAsync(
            await readService.GetSchoolAsync(SchoolId.From(schoolId), cancellationToken), cancellationToken);
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
        string? displayName,
        string? address,
        DateOnly? createdFrom,
        DateOnly? createdTo,
        DateOnly? activatedFrom,
        DateOnly? activatedTo,
        Guid? unitTypeId,
        string? unitType,
        string? owner,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        try
        {
            RegistryRequestContext.ValidateList(pageNumber, pageSize, search);
            if (displayName?.Trim().Length > 200 || address?.Trim().Length > 500 ||
                unitType?.Trim().Length > 200 ||
                owner?.Trim().Length > 200 || unitTypeId == Guid.Empty ||
                createdFrom > createdTo || activatedFrom > activatedTo ||
                status.HasValue && !Enum.IsDefined(status.Value) ||
                schoolType.HasValue && !Enum.IsDefined(schoolType.Value))
            {
                throw new ValidationException("School filters are invalid.");
            }

            var ownerIds = await FindOwnerIdsAsync(owner, cancellationToken);

            var page = await readService.ListSchoolsAsync(
                new ListSchoolsQuery(tenantId, search, status, schoolType, pageNumber, pageSize,
                    displayName, address, createdFrom, createdTo, activatedFrom, activatedTo,
                    unitTypeId, unitType, owner, ownerIds),
                cancellationToken);
            var items = await EnrichOwnersAsync(page.Items, cancellationToken);
            return Ok(PagedApiResponse<SchoolReadModel>.Success(
                items,
                page.TotalCount,
                page.PageNumber,
                page.PageSize,
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
        }
        catch(Exception ex)
        {
            throw;
        }
    }

    private async Task<IReadOnlyCollection<Guid>?> FindOwnerIdsAsync(
        string? owner,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(owner)) return null;
        var normalized = owner.Trim();
        var accounts = identityDb.Accounts.AsNoTracking()
            .Where(account => account.DisplayName != null);
        accounts = identityDb.Database.IsNpgsql()
            ? accounts.Where(account => EF.Functions.ILike(account.DisplayName!,
                $"%{EscapeLike(normalized)}%", "\\"))
            : accounts.Where(account => account.DisplayName!.Contains(normalized));
        return await accounts.Select(account => account.Id).ToArrayAsync(cancellationToken);
    }

    private async Task<SchoolReadModel> EnrichOwnerAsync(
        SchoolReadModel school,
        CancellationToken cancellationToken)
    {
        var name = await identityDb.Accounts.AsNoTracking()
            .Where(account => account.Id == school.RequestedByAccountId)
            .Select(account => account.DisplayName)
            .SingleOrDefaultAsync(cancellationToken);
        return school with { OwnerDisplayName = name };
    }

    private async Task<IReadOnlyList<SchoolReadModel>> EnrichOwnersAsync(
        IReadOnlyList<SchoolReadModel> schools,
        CancellationToken cancellationToken)
    {
        if (schools.Count == 0) return schools;
        var ids = schools.Select(school => school.RequestedByAccountId).Distinct().ToArray();
        var owners = await identityDb.Accounts.AsNoTracking()
            .Where(account => ids.Contains(account.Id))
            .Select(account => new { account.Id, account.DisplayName })
            .ToDictionaryAsync(account => account.Id, account => account.DisplayName, cancellationToken);
        return schools.Select(school => school with {
            OwnerDisplayName = owners.GetValueOrDefault(school.RequestedByAccountId)
        }).ToArray();
    }

    private static string EscapeLike(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("%", "\\%", StringComparison.Ordinal)
            .Replace("_", "\\_", StringComparison.Ordinal);

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
