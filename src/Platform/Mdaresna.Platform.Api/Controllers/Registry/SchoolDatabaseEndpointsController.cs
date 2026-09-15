using System.ComponentModel.DataAnnotations;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Application.Registry.DatabaseEndpoints;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.Api.Controllers.Registry;

[ApiController]
[Route("api/platform/v1/schools/{schoolId:guid}/database-endpoints")]
public sealed class SchoolDatabaseEndpointsController(
    SchoolDatabaseEndpointRegistry registry) : ControllerBase
{
    [HttpGet]
    [PlatformPermission("platform.deployments.manage")]
    public async Task<IActionResult> List(
        Guid schoolId,
        CancellationToken cancellationToken)
    {
        RegistryRequestContext.RequireId(schoolId, nameof(schoolId));
        var endpoints = await registry.ListAsync(SchoolId.From(schoolId), cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SchoolDatabaseEndpointReadModel>>.Success(
            endpoints,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPost]
    [PlatformPermission("platform.deployments.manage")]
    public async Task<IActionResult> Register(
        Guid schoolId,
        [FromBody] RegisterSchoolDatabaseEndpointRequest request,
        CancellationToken cancellationToken)
    {
        ValidateEndpointRequest(
            schoolId,
            request.EndpointId,
            request.Purpose,
            request.Provider,
            request.Host,
            request.Port,
            request.DatabaseName,
            request.CredentialSecretReference,
            request.RequireTls,
            request.Region,
            request.SchemaVersion);
        var result = await registry.RegisterAsync(
            new RegisterSchoolDatabaseEndpointCommand(
                request.EndpointId,
                SchoolId.From(schoolId),
                request.Purpose,
                request.Provider,
                request.Host,
                request.Port,
                request.DatabaseName,
                request.CredentialSecretReference,
                request.RequireTls!.Value,
                request.IsPrimary,
                request.Region,
                request.SchemaVersion,
                RegistryRequestContext.Actor(HttpContext),
                RegistryRequestContext.CorrelationId(HttpContext)),
            cancellationToken);
        return CreatedAtAction(
            nameof(List),
            new { schoolId },
            ApiResponse<SchoolDatabaseEndpointReadModel>.Success(
                result,
                StatusCodes.Status201Created,
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPut("{endpointId:guid}")]
    [PlatformPermission("platform.deployments.manage")]
    public async Task<IActionResult> Update(
        Guid schoolId,
        Guid endpointId,
        [FromBody] UpdateSchoolDatabaseEndpointRequest request,
        CancellationToken cancellationToken)
    {
        ValidateEndpointRequest(
            schoolId,
            endpointId,
            SchoolDatabasePurpose.Operational,
            SchoolDatabaseProvider.PostgreSql,
            request.Host,
            request.Port,
            request.DatabaseName,
            request.CredentialSecretReference,
            request.RequireTls,
            request.Region,
            request.SchemaVersion,
            validateEnums: false);
        RequireVersion(request.ExpectedVersion);
        var result = await registry.UpdateAsync(
            new UpdateSchoolDatabaseEndpointCommand(
                endpointId,
                SchoolId.From(schoolId),
                request.ExpectedVersion!.Value,
                request.Host,
                request.Port,
                request.DatabaseName,
                request.CredentialSecretReference,
                request.RequireTls!.Value,
                request.Region,
                request.SchemaVersion,
                RegistryRequestContext.Actor(HttpContext),
                RegistryRequestContext.CorrelationId(HttpContext)),
            cancellationToken);
        return Ok(ApiResponse<SchoolDatabaseEndpointReadModel>.Success(
            result,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPost("{endpointId:guid}/status")]
    [PlatformPermission("platform.deployments.manage")]
    public async Task<IActionResult> ChangeStatus(
        Guid schoolId,
        Guid endpointId,
        [FromBody] ChangeSchoolDatabaseEndpointStatusRequest request,
        CancellationToken cancellationToken)
    {
        RegistryRequestContext.RequireId(schoolId, nameof(schoolId));
        RegistryRequestContext.RequireId(endpointId, nameof(endpointId));
        RequireVersion(request.ExpectedVersion);
        if (!Enum.IsDefined(request.Status))
        {
            throw new ValidationException("Database endpoint status is invalid.");
        }

        var result = await registry.ChangeStatusAsync(
            new ChangeSchoolDatabaseEndpointStatusCommand(
                endpointId,
                SchoolId.From(schoolId),
                request.ExpectedVersion!.Value,
                request.Status,
                RegistryRequestContext.Actor(HttpContext),
                RegistryRequestContext.CorrelationId(HttpContext)),
            cancellationToken);
        return Ok(ApiResponse<SchoolDatabaseEndpointReadModel>.Success(
            result,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPost("{endpointId:guid}/make-primary")]
    [PlatformPermission("platform.deployments.manage")]
    public async Task<IActionResult> MakePrimary(
        Guid schoolId,
        Guid endpointId,
        [FromBody] MakeSchoolDatabaseEndpointPrimaryRequest request,
        CancellationToken cancellationToken)
    {
        RegistryRequestContext.RequireId(schoolId, nameof(schoolId));
        RegistryRequestContext.RequireId(endpointId, nameof(endpointId));
        RequireVersion(request.ExpectedVersion);
        var result = await registry.MakePrimaryAsync(
            new MakeSchoolDatabaseEndpointPrimaryCommand(
                endpointId,
                SchoolId.From(schoolId),
                request.ExpectedVersion!.Value,
                RegistryRequestContext.Actor(HttpContext),
                RegistryRequestContext.CorrelationId(HttpContext)),
            cancellationToken);
        return Ok(ApiResponse<SchoolDatabaseEndpointReadModel>.Success(
            result,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    private static void ValidateEndpointRequest(
        Guid schoolId,
        Guid endpointId,
        SchoolDatabasePurpose purpose,
        SchoolDatabaseProvider provider,
        string? host,
        int port,
        string? databaseName,
        string? credentialSecretReference,
        bool? requireTls,
        string? region,
        string? schemaVersion,
        bool validateEnums = true)
    {
        RegistryRequestContext.RequireId(schoolId, nameof(schoolId));
        RegistryRequestContext.RequireId(endpointId, nameof(endpointId));
        if ((validateEnums && (!Enum.IsDefined(purpose) || !Enum.IsDefined(provider))) ||
            string.IsNullOrWhiteSpace(host) || host.Trim().Length > 253 ||
            port is < 1 or > 65535 ||
            string.IsNullOrWhiteSpace(databaseName) || databaseName.Trim().Length > 128 ||
            string.IsNullOrWhiteSpace(credentialSecretReference) ||
            credentialSecretReference.Trim().Length > 500 ||
            requireTls is null ||
            region?.Trim().Length > 100 || schemaVersion?.Trim().Length > 64)
        {
            throw new ValidationException("School database endpoint data is invalid.");
        }
    }

    private static void RequireVersion(long? expectedVersion)
    {
        if (expectedVersion is null or < 0)
        {
            throw new ValidationException("ExpectedVersion must be supplied and non-negative.");
        }
    }
}

public sealed record RegisterSchoolDatabaseEndpointRequest(
    Guid EndpointId,
    SchoolDatabasePurpose Purpose,
    SchoolDatabaseProvider Provider,
    [Required, MaxLength(253)] string Host,
    [Range(1, 65535)] int Port,
    [Required, MaxLength(128)] string DatabaseName,
    [Required, MaxLength(500)] string CredentialSecretReference,
    [Required] bool? RequireTls,
    bool IsPrimary,
    [MaxLength(100)] string? Region,
    [MaxLength(64)] string? SchemaVersion);

public sealed record UpdateSchoolDatabaseEndpointRequest(
    long? ExpectedVersion,
    [Required, MaxLength(253)] string Host,
    [Range(1, 65535)] int Port,
    [Required, MaxLength(128)] string DatabaseName,
    [Required, MaxLength(500)] string CredentialSecretReference,
    [Required] bool? RequireTls,
    [MaxLength(100)] string? Region,
    [MaxLength(64)] string? SchemaVersion);

public sealed record ChangeSchoolDatabaseEndpointStatusRequest(
    long? ExpectedVersion,
    SchoolDatabaseEndpointStatus Status);

public sealed record MakeSchoolDatabaseEndpointPrimaryRequest(long? ExpectedVersion);
