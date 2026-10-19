using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Application.Billing.Subscriptions;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.Api.Controllers.Billing;

[ApiController, Route("api/platform/v1")]
public sealed class SchoolSubscriptionsController(ISubscriptionService subscriptions) : ControllerBase
{
    private CatalogActor Actor() => new(UnitCommerceRequestContext.Actor(HttpContext), UnitCommerceRequestContext.CorrelationId(HttpContext));
    private IActionResult Reply<T>(T value) => Ok(ApiResponse<T>.Success(value));
    [HttpGet("schools/{schoolId:guid}/subscriptions"), PlatformPermission("platform.billing.read")]
    public async Task<IActionResult> List(Guid schoolId, CancellationToken ct) => Reply(await subscriptions.ListAsync(schoolId, ct));
    [HttpGet("schools/{schoolId:guid}/subscription-quotes"), PlatformPermission("platform.billing.read")]
    public async Task<IActionResult> Quotes(Guid schoolId, CancellationToken ct) => Reply(await subscriptions.QuotesAsync(schoolId, ct));
    [HttpGet("subscriptions/{id:guid}/delivery"), PlatformPermission("platform.billing.read")]
    public async Task<IActionResult> Delivery(Guid id, CancellationToken ct) => Reply(await subscriptions.DeliveryAsync(id, ct));
    [HttpPost("subscriptions/{id:guid}/resynchronize"), PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Resynchronize(Guid id, ResynchronizeSubscriptionRequest input, CancellationToken ct)
    { await subscriptions.ResynchronizeAsync(id, input.ExpectedVersion, Actor(), ct); return Reply(new { queued = true }); }
    [HttpPost("schools/{schoolId:guid}/subscription-quotes"), PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Create(Guid schoolId, CreateSubscriptionQuote input, CancellationToken ct)
    {
        if (input.SchoolId != schoolId) return BadRequest();
        return Reply(await subscriptions.QuoteAsync(input, Actor(), ct));
    }
    [HttpPost("subscription-quotes/{id:guid}/submit"), PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Submit(Guid id, SubmitSubscriptionQuote input, CancellationToken ct) => Reply(await subscriptions.SubmitAsync(id, input, Actor(), ct));
    [HttpPost("subscription-quotes/{id:guid}/approve-free"), PlatformPermission("platform.payments.approve")]
    public async Task<IActionResult> Free(Guid id, CancellationToken ct) => Reply(await subscriptions.ApplyFreeAsync(id, Actor(), ct));
    [HttpPost("subscriptions/{id:guid}/status"), PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Status(Guid id, SubscriptionStatusRequest input, CancellationToken ct)
    { await subscriptions.SetSuspendedAsync(id, input.ExpectedVersion, input.IsSuspended, Actor(), ct); return Reply(new { changed = true }); }
}
public sealed record SubscriptionStatusRequest(long ExpectedVersion, bool IsSuspended);
public sealed record ResynchronizeSubscriptionRequest(long ExpectedVersion);
