using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Api.Errors;
using Mdaresna.Platform.Application.Billing.ReviewSchoolPlatformPayment;
using Mdaresna.Platform.Application.Billing.Read;
using Mdaresna.Platform.Application.Billing.SubmitSchoolPlatformPayment;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.Api.Controllers.Billing;

[ApiController]
[Route("api/platform/v1/payments")]
public sealed class PlatformPaymentsController(
    SubmitSchoolPlatformPaymentCommandHandler submit,
    ReviewSchoolPlatformPaymentCommandHandler review,
    IPlatformPaymentReadStore readStore) : ControllerBase
{
    [PlatformPermission("platform.billing.read")]
    [HttpGet]
    public async Task<IResult> List(
        [FromQuery] Guid? tenantId,
        [FromQuery] Guid? schoolId,
        [FromQuery] string? status,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        PlatformPaymentStatus? parsedStatus = null;
        if (status is not null)
        {
            if (!Enum.GetNames<PlatformPaymentStatus>()
                    .Contains(status, StringComparer.OrdinalIgnoreCase))
            {
                return ApiResponseWriter.ToResult(ApiResponse<object?>.Failure(
                    400, "request.invalid_status", "Invalid payment status.",
                    correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
            }

            parsedStatus = Enum.Parse<PlatformPaymentStatus>(status, true);
        }

        var page = await readStore.ListAsync(
            tenantId.HasValue ? TenantId.From(tenantId.Value) : null,
            schoolId.HasValue ? SchoolId.From(schoolId.Value) : null,
            parsedStatus, pageNumber, pageSize, cancellationToken);

        return ApiResponseWriter.ToResult(PagedApiResponse<PlatformPaymentDetail>.Success(
            page.Items, page.TotalCount, page.PageNumber, page.PageSize,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [PlatformPermission("platform.billing.read")]
    [HttpGet("{requestId:guid}")]
    public async Task<IResult> Get(Guid requestId, CancellationToken cancellationToken)
    {
        var payment = await readStore.GetAsync(requestId, cancellationToken);
        return payment is null
            ? ApiResponseWriter.ToResult(ApiResponse<object?>.Failure(
                404, "platform_payment.not_found", "Platform payment request was not found.",
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)))
            : ApiResponseWriter.ToResult(ApiResponse<PlatformPaymentDetail>.Success(
                payment, correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [PlatformPermission("platform.billing.manage")]
    [HttpPost]
    public async Task<IResult> Submit(
        [FromBody] SubmitPlatformPaymentRequest request,
        CancellationToken cancellationToken)
    {
        var result = await submit.HandleAsync(new SubmitSchoolPlatformPaymentCommand(
            request.RequestId,
            TenantId.From(request.TenantId),
            SchoolId.From(request.SchoolId),
            request.Amount,
            request.Currency,
            request.TransferReference,
            CurrentAccountId(),
            CorrelationId()), cancellationToken);

        return ApiResponseWriter.ToResult(ApiResponse<object>.Success(
            new { result.RequestId, Status = result.Status.ToString(), result.WasCreated },
            statusCode: result.WasCreated ? 201 : 200,
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    [PlatformPermission("platform.payments.approve")]
    [HttpPost("{requestId:guid}/review")]
    public async Task<IResult> Review(
        Guid requestId,
        [FromBody] ReviewPlatformPaymentRequest request,
        CancellationToken cancellationToken)
    {
        if (!Enum.GetNames<PlatformPaymentReviewDecision>()
                .Contains(request.Decision, StringComparer.OrdinalIgnoreCase))
        {
            return ApiResponseWriter.ToResult(ApiResponse<object?>.Failure(
                400, "request.invalid_decision", "Decision must be Approve or Reject.",
                correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
        }

        var decision = Enum.Parse<PlatformPaymentReviewDecision>(request.Decision, true);

        var result = await review.HandleAsync(new ReviewSchoolPlatformPaymentCommand(
            requestId,
            decision,
            CurrentAccountId(),
            request.ReviewNote,
            CorrelationId()), cancellationToken);

        return ApiResponseWriter.ToResult(ApiResponse<object>.Success(
            new { result.RequestId, Status = result.Status.ToString(), result.LedgerEntryId, result.UnitGrantId },
            correlationId: ApiResponseWriter.GetCorrelationId(HttpContext)));
    }

    private IdentityAccountId CurrentAccountId() =>
        Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var accountId) &&
        accountId != Guid.Empty
            ? IdentityAccountId.From(accountId)
            : throw new UnauthorizedAccessException("Platform account identity is missing.");

    private Guid CorrelationId() =>
        Guid.TryParse(ApiResponseWriter.GetCorrelationId(HttpContext), out var correlationId)
            ? correlationId
            : throw new InvalidOperationException("Correlation ID middleware is required.");
}

public sealed record SubmitPlatformPaymentRequest(
    Guid RequestId,
    Guid TenantId,
    Guid SchoolId,
    decimal Amount,
    [Required, MaxLength(3)] string Currency,
    [Required, MaxLength(100)] string TransferReference);

public sealed record ReviewPlatformPaymentRequest(
    [Required] string Decision,
    [MaxLength(1000)] string? ReviewNote);
