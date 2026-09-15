using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Application.Billing.Units;
using Mdaresna.Platform.Domain.Access;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.Api.Controllers.Billing;

[ApiController]
[Route("api/platform/v1/unit-types")]
public sealed class UnitTypesController(
    ListUnitTypesQueryHandler list,
    CreateUnitTypeCommandHandler create,
    UpdateUnitTypeCommandHandler update,
    DeactivateUnitTypeCommandHandler deactivate,
    ActivateUnitTypeCommandHandler activate,
    DeleteUnitTypeCommandHandler delete) : ControllerBase
{
    [HttpGet]
    [PlatformPermission("platform.billing.read")]
    public async Task<IActionResult> List(
        [FromQuery] string? search = null,
        [FromQuery] string? code = null,
        [FromQuery] string? displayName = null,
        [FromQuery] decimal? unitPrice = null,
        [FromQuery] string? currency = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        UnitCommerceRequestContext.ValidateList(
            pageNumber, pageSize, search, code, displayName, unitPrice, currency);
        var page = await list.HandleAsync(
            new UnitTypeListQuery(search, isActive, pageNumber, pageSize,
                code, displayName, unitPrice, currency),
            cancellationToken);
        return Ok(PagedApiResponse<UnitTypeReadModel>.Success(
            page.Items, page.TotalCount, page.PageNumber, page.PageSize,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpGet("{unitTypeId:guid}")]
    [PlatformPermission("platform.billing.read")]
    public async Task<IActionResult> Get(Guid unitTypeId, CancellationToken cancellationToken)
    {
        UnitCommerceRequestContext.RequireId(unitTypeId, nameof(unitTypeId));
        var unitType = await list.GetAsync(unitTypeId, cancellationToken);
        return Ok(ApiResponse<UnitTypeReadModel>.Success(
            unitType, correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPost]
    [PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Create(
        [FromBody] CreateUnitTypeRequest request,
        CancellationToken cancellationToken)
    {
        UnitCommerceRequestContext.RequireId(request.UnitTypeId, nameof(request.UnitTypeId));
        var result = await create.HandleAsync(new CreateUnitTypeCommand(
            request.UnitTypeId, request.Code, request.DisplayName,
            request.UnitPrice, request.Currency,
            UnitCommerceRequestContext.Actor(HttpContext),
            UnitCommerceRequestContext.CorrelationId(HttpContext)), cancellationToken);
        var statusCode = result.Changed
            ? StatusCodes.Status201Created
            : StatusCodes.Status200OK;
        var response = ApiResponse<UnitTypeMutationResult>.Success(
            result, statusCode: statusCode,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext));
        return result.Changed
            ? CreatedAtAction(nameof(Get), new { unitTypeId = result.UnitTypeId }, response)
            : Ok(response);
    }

    [HttpPut("{unitTypeId:guid}")]
    [PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Update(
        Guid unitTypeId,
        [FromBody] UpdateUnitTypeRequest request,
        CancellationToken cancellationToken)
    {
        UnitCommerceRequestContext.RequireId(unitTypeId, nameof(unitTypeId));
        if (request.ExpectedVersion is null or < 0)
        {
            throw new ValidationException("ExpectedVersion must be supplied and non-negative.");
        }

        var result = await update.HandleAsync(new UpdateUnitTypeCommand(
            unitTypeId, request.ExpectedVersion.Value,
            request.DisplayName, request.UnitPrice, request.Currency,
            UnitCommerceRequestContext.Actor(HttpContext),
            UnitCommerceRequestContext.CorrelationId(HttpContext)), cancellationToken);
        return Ok(ApiResponse<UnitTypeMutationResult>.Success(
            result, correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPost("{unitTypeId:guid}/deactivate")]
    [PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Deactivate(
        Guid unitTypeId,
        [FromBody] DeactivateUnitTypeRequest request,
        CancellationToken cancellationToken)
    {
        UnitCommerceRequestContext.RequireId(unitTypeId, nameof(unitTypeId));
        if (request.ExpectedVersion is null or < 0)
        {
            throw new ValidationException("ExpectedVersion must be supplied and non-negative.");
        }

        var result = await deactivate.HandleAsync(new DeactivateUnitTypeCommand(
            unitTypeId, request.ExpectedVersion.Value,
            UnitCommerceRequestContext.Actor(HttpContext),
            UnitCommerceRequestContext.CorrelationId(HttpContext)), cancellationToken);
        return Ok(ApiResponse<UnitTypeMutationResult>.Success(
            result, correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPost("{unitTypeId:guid}/activate")]
    [PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Activate(
        Guid unitTypeId,
        [FromBody] DeactivateUnitTypeRequest request,
        CancellationToken cancellationToken)
    {
        UnitCommerceRequestContext.RequireId(unitTypeId, nameof(unitTypeId));
        if (request.ExpectedVersion is null or < 0)
        {
            throw new ValidationException("ExpectedVersion must be supplied and non-negative.");
        }

        var result = await activate.HandleAsync(new ActivateUnitTypeCommand(
            unitTypeId, request.ExpectedVersion.Value,
            UnitCommerceRequestContext.Actor(HttpContext),
            UnitCommerceRequestContext.CorrelationId(HttpContext)), cancellationToken);
        return Ok(ApiResponse<UnitTypeMutationResult>.Success(
            result, correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpDelete("{unitTypeId:guid}")]
    [PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Delete(
        Guid unitTypeId,
        [FromQuery] long? expectedVersion,
        CancellationToken cancellationToken)
    {
        UnitCommerceRequestContext.RequireId(unitTypeId, nameof(unitTypeId));
        if (expectedVersion is null or < 0)
        {
            throw new ValidationException("ExpectedVersion must be supplied and non-negative.");
        }

        await delete.HandleAsync(new DeleteUnitTypeCommand(
            unitTypeId, expectedVersion.Value,
            UnitCommerceRequestContext.Actor(HttpContext),
            UnitCommerceRequestContext.CorrelationId(HttpContext)), cancellationToken);
        return Ok(ApiResponse<object?>.Success(
            null, correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }
}

public sealed record CreateUnitTypeRequest(
    Guid UnitTypeId,
    [Required, MaxLength(32)] string Code,
    [Required, MaxLength(200)] string DisplayName,
    decimal UnitPrice,
    [Required, StringLength(3, MinimumLength = 3)] string Currency);

public sealed record UpdateUnitTypeRequest(
    long? ExpectedVersion,
    [Required, MaxLength(200)] string DisplayName,
    decimal UnitPrice,
    [Required, StringLength(3, MinimumLength = 3)] string Currency);

public sealed record DeactivateUnitTypeRequest(long? ExpectedVersion);

internal static class UnitCommerceRequestContext
{
    public static IdentityAccountId Actor(HttpContext context)
    {
        if (context.User.FindFirst(PlatformTokenClaims.Purpose)?.Value !=
                PlatformTokenClaims.TokenPurpose ||
            !Guid.TryParse(context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
                out var accountId) ||
            accountId == Guid.Empty)
        {
            throw new InvalidOperationException("A validated platform operator is required.");
        }

        return IdentityAccountId.From(accountId);
    }

    public static Guid CorrelationId(HttpContext context) =>
        Guid.TryParse(ApiResponseWriter.GetCorrelationId(context), out var correlationId) &&
        correlationId != Guid.Empty
            ? correlationId
            : throw new InvalidOperationException(
                "Correlation middleware did not provide a valid ID.");

    public static void RequireId(Guid id, string field)
    {
        if (id == Guid.Empty)
        {
            throw new ValidationException($"{field} must not be empty.");
        }
    }

    public static void ValidateList(
        int pageNumber,
        int pageSize,
        string? search,
        string? code,
        string? displayName,
        decimal? unitPrice,
        string? currency)
    {
        if (pageNumber < 1 || pageSize is < 1 or > 100 ||
            ((long)pageNumber - 1) * pageSize > int.MaxValue ||
            search?.Trim().Length > 200 || code?.Trim().Length > 32 ||
            displayName?.Trim().Length > 200 || currency?.Trim().Length > 3 ||
            unitPrice is <= 0 or > 9999999999999999.99m)
        {
            throw new ValidationException("Invalid unit type listing request.");
        }
    }
}
