using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Application.Billing.Subscriptions;
using Microsoft.AspNetCore.Mvc;
namespace Mdaresna.Platform.Api.Controllers.Billing;

[ApiController, Route("api/platform/v1/invoices")]
public sealed class PlatformInvoicesController(IPlatformInvoiceService invoices) : ControllerBase
{
    [HttpPost("from-quote/{quoteId:guid}"), PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Issue(Guid quoteId, CancellationToken ct) => Ok(ApiResponse<object>.Success(new {
        invoiceId = await invoices.IssueQuoteAsync(quoteId, new(UnitCommerceRequestContext.Actor(HttpContext), UnitCommerceRequestContext.CorrelationId(HttpContext)), ct)
    }));
    [HttpGet, PlatformPermission("platform.billing.read")]
    public async Task<IActionResult> List([FromQuery] Guid? schoolId, [FromQuery] int page = 1, CancellationToken ct = default) => Ok(ApiResponse<InvoiceView[]>.Success(await invoices.ListAsync(schoolId, page, ct)));
    [HttpPost("{id:guid}/receipts"), PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Receipt(Guid id, RecordInvoiceReceipt input, CancellationToken ct)
    {
        await invoices.RecordAsync(id, input, new(UnitCommerceRequestContext.Actor(HttpContext), UnitCommerceRequestContext.CorrelationId(HttpContext)), ct);
        return Ok(ApiResponse<object>.Success(new { recorded = true }));
    }
}
