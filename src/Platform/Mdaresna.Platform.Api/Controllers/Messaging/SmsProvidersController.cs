using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Infrastructure.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.Api.Controllers.Messaging;

[ApiController]
[Route("api/platform/v1/sms-providers")]
public sealed class SmsProvidersController(PlatformSmsProviderService providers) : ControllerBase
{
    [HttpGet]
    [PlatformPermission("platform.deployments.manage")]
    public async Task<IActionResult> List(
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var page = await providers.ListAsync(isActive, pageNumber, pageSize, cancellationToken);
        return Ok(PagedApiResponse<PlatformSmsProviderReadModel>.Success(
            page.Items, page.TotalCount, page.PageNumber, page.PageSize,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpGet("{providerId:guid}")]
    [PlatformPermission("platform.deployments.manage")]
    public async Task<IActionResult> Get(Guid providerId, CancellationToken cancellationToken)
    {
        var provider = await providers.GetAsync(providerId, cancellationToken);
        return Ok(ApiResponse<PlatformSmsProviderReadModel>.Success(
            provider, correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPost]
    [PlatformPermission("platform.deployments.manage")]
    public async Task<IActionResult> Create(
        [FromBody] CreateSmsProviderRequest request,
        CancellationToken cancellationToken)
    {
        var provider = await providers.CreateAsync(request.ToValues(), Actor(),
            ApiResponseWriter.GetCorrelationId(HttpContext), cancellationToken);
        return CreatedAtAction(nameof(Get), new { providerId = provider.Id },
            ApiResponse<PlatformSmsProviderReadModel>.Success(provider, statusCode: 201,
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPut("{providerId:guid}")]
    [PlatformPermission("platform.deployments.manage")]
    public async Task<IActionResult> Update(
        Guid providerId,
        [FromBody] UpdateSmsProviderRequest request,
        CancellationToken cancellationToken)
    {
        var provider = await providers.UpdateAsync(providerId, request.Version,
            request.ToValues(), Actor(), ApiResponseWriter.GetCorrelationId(HttpContext),
            cancellationToken);
        return Ok(ApiResponse<PlatformSmsProviderReadModel>.Success(
            provider, correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPost("{providerId:guid}/activate")]
    [PlatformPermission("platform.deployments.manage")]
    public Task<IActionResult> Activate(
        Guid providerId,
        [FromBody] SmsProviderVersionRequest request,
        CancellationToken cancellationToken) =>
        SetActive(providerId, request.Version, true, cancellationToken);

    [HttpPost("{providerId:guid}/deactivate")]
    [PlatformPermission("platform.deployments.manage")]
    public Task<IActionResult> Deactivate(
        Guid providerId,
        [FromBody] SmsProviderVersionRequest request,
        CancellationToken cancellationToken) =>
        SetActive(providerId, request.Version, false, cancellationToken);

    [HttpDelete("{providerId:guid}")]
    [PlatformPermission("platform.deployments.manage")]
    public async Task<IActionResult> Delete(
        Guid providerId,
        [FromQuery, Required] string version,
        CancellationToken cancellationToken)
    {
        await providers.DeleteAsync(providerId, version, Actor(),
            ApiResponseWriter.GetCorrelationId(HttpContext), cancellationToken);
        return Ok(ApiResponse<object?>.Success(null,
            message: "SMS provider deleted.",
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    private async Task<IActionResult> SetActive(
        Guid providerId,
        string version,
        bool active,
        CancellationToken cancellationToken)
    {
        var provider = await providers.SetActiveAsync(providerId, version, active,
            Actor(), ApiResponseWriter.GetCorrelationId(HttpContext), cancellationToken);
        return Ok(ApiResponse<PlatformSmsProviderReadModel>.Success(provider,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    private Guid Actor()
    {
        if (User.FindFirst(PlatformTokenClaims.Purpose)?.Value !=
                PlatformTokenClaims.TokenPurpose ||
            !Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var id) ||
            id == Guid.Empty)
        {
            throw new InvalidOperationException("A validated Platform operator is required.");
        }

        return id;
    }
}

public sealed class CreateSmsProviderRequest
{
    [Required, MaxLength(300)] public string ProviderUserName { get; init; } = string.Empty;
    [Required, MaxLength(300)] public string ProviderPassword { get; init; } = string.Empty;
    [Required, MaxLength(300)] public string SenderName { get; init; } = string.Empty;
    [Required, MaxLength(2000)] public string ApiUrlTemplate { get; init; } = string.Empty;
    [Range(1, 1000)] public int MessageCharactersLength { get; init; }
    [Range(1, 1000)] public int Priority { get; init; }
    [Required, MaxLength(200)] public string SuccessResponsePrefix { get; init; } = string.Empty;

    public PlatformSmsProviderValues ToValues() => new()
    {
        ProviderUserName = ProviderUserName,
        ProviderPassword = ProviderPassword,
        SenderName = SenderName,
        ApiUrlTemplate = ApiUrlTemplate,
        MessageCharactersLength = MessageCharactersLength,
        Priority = Priority,
        SuccessResponsePrefix = SuccessResponsePrefix
    };
}

public sealed class UpdateSmsProviderRequest
{
    [Required] public string Version { get; init; } = string.Empty;
    [Required, MaxLength(300)] public string ProviderUserName { get; init; } = string.Empty;
    [MaxLength(300)] public string? ProviderPassword { get; init; }
    [Required, MaxLength(300)] public string SenderName { get; init; } = string.Empty;
    [Required, MaxLength(2000)] public string ApiUrlTemplate { get; init; } = string.Empty;
    [Range(1, 1000)] public int MessageCharactersLength { get; init; }
    [Range(1, 1000)] public int Priority { get; init; }
    [Required, MaxLength(200)] public string SuccessResponsePrefix { get; init; } = string.Empty;

    public PlatformSmsProviderValues ToValues() => new()
    {
        ProviderUserName = ProviderUserName,
        ProviderPassword = ProviderPassword,
        SenderName = SenderName,
        ApiUrlTemplate = ApiUrlTemplate,
        MessageCharactersLength = MessageCharactersLength,
        Priority = Priority,
        SuccessResponsePrefix = SuccessResponsePrefix
    };
}

public sealed class SmsProviderVersionRequest
{
    [Required] public string Version { get; init; } = string.Empty;
}
