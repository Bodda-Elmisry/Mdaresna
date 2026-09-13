using System.ComponentModel.DataAnnotations;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Application.Billing.Read;
using Mdaresna.Platform.Application.Billing.Units;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.Api.Controllers.Billing;

[ApiController]
[Route("api/platform/v1/unit-purchases")]
public sealed class UnitPurchasesController(
    SubmitUnitPurchaseCommandHandler submit,
    IUnitPurchaseRepository purchases,
    IPlatformPaymentReadStore payments) : ControllerBase
{
    [HttpGet("{requestId:guid}")]
    [PlatformPermission("platform.billing.read")]
    public async Task<IActionResult> Get(Guid requestId, CancellationToken cancellationToken)
    {
        UnitCommerceRequestContext.RequireId(requestId, nameof(requestId));
        var payment = await payments.GetAsync(requestId, cancellationToken);
        var intent = await purchases.FindIntentAsync(requestId, cancellationToken);
        if (payment is null || intent is null)
        {
            throw new PlatformResourceNotFoundException(
                "unit_purchase.not_found", "Unit purchase was not found.");
        }

        var grant = await purchases.FindGrantAsync(requestId, cancellationToken);
        var detail = new UnitPurchaseDetail(
            payment,
            intent.UnitTypeId,
            intent.UnitTypeVersion,
            intent.UnitTypeCode,
            intent.UnitTypeName,
            intent.Quantity,
            intent.UnitPrice,
            intent.Amount,
            intent.Currency,
            intent.PaymentMethodCode,
            intent.TransferOccurredAtUtc,
            intent.CreatedAtUtc,
            grant?.Id,
            grant?.GrantedAtUtc);
        return Ok(ApiResponse<UnitPurchaseDetail>.Success(
            detail, correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [HttpPost]
    [PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Submit(
        [FromBody] SubmitUnitPurchaseRequest request,
        CancellationToken cancellationToken)
    {
        UnitCommerceRequestContext.RequireId(request.RequestId, nameof(request.RequestId));
        UnitCommerceRequestContext.RequireId(request.TenantId, nameof(request.TenantId));
        UnitCommerceRequestContext.RequireId(request.SchoolId, nameof(request.SchoolId));
        UnitCommerceRequestContext.RequireId(request.UnitTypeId, nameof(request.UnitTypeId));
        if (request.TenantId != request.SchoolId ||
            request.ExpectedUnitTypeVersion is null or < 0 ||
            request.Quantity is < 1 or > 1_000_000 ||
            request.TransferOccurredAtUtc == default ||
            request.TransferOccurredAtUtc.Offset != TimeSpan.Zero)
        {
            throw new ValidationException("Unit purchase request data is invalid.");
        }

        var result = await submit.HandleAsync(new SubmitUnitPurchaseCommand(
            request.RequestId,
            TenantId.From(request.TenantId),
            SchoolId.From(request.SchoolId),
            request.UnitTypeId,
            request.ExpectedUnitTypeVersion.Value,
            request.Quantity,
            request.TransferReference,
            request.PaymentMethodCode,
            request.TransferOccurredAtUtc,
            UnitCommerceRequestContext.Actor(HttpContext),
            UnitCommerceRequestContext.CorrelationId(HttpContext)), cancellationToken);
        var statusCode = result.WasCreated
            ? StatusCodes.Status201Created
            : StatusCodes.Status200OK;
        var response = ApiResponse<SubmitUnitPurchaseResult>.Success(
            result, statusCode: statusCode,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext));
        return result.WasCreated
            ? CreatedAtAction(nameof(Get), new { requestId = result.RequestId }, response)
            : Ok(response);
    }
}

public sealed record SubmitUnitPurchaseRequest(
    Guid RequestId,
    Guid TenantId,
    Guid SchoolId,
    Guid UnitTypeId,
    long? ExpectedUnitTypeVersion,
    int Quantity,
    [Required, MaxLength(100)] string TransferReference,
    [Required, MaxLength(40)] string PaymentMethodCode,
    DateTimeOffset TransferOccurredAtUtc);

public sealed record UnitPurchaseDetail(
    PlatformPaymentDetail Payment,
    Guid UnitTypeId,
    long UnitTypeVersion,
    string UnitTypeCode,
    string UnitTypeName,
    int Quantity,
    decimal UnitPrice,
    decimal Amount,
    string Currency,
    string PaymentMethodCode,
    DateTimeOffset TransferOccurredAtUtc,
    DateTimeOffset SubmittedAtUtc,
    Guid? GrantId,
    DateTimeOffset? GrantedAtUtc);
