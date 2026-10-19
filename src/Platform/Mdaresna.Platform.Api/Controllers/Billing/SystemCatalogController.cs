using Mdaresna.Api.Contracts;
using Mdaresna.Platform.Api.Auth;
using Mdaresna.Platform.Application.Billing.Subscriptions;
using Mdaresna.Platform.Domain.Billing.Subscriptions;
using Microsoft.AspNetCore.Mvc;

namespace Mdaresna.Platform.Api.Controllers.Billing;

[ApiController, Route("api/platform/v1")]
public sealed class SystemCatalogController(ISystemCatalogService catalog) : ControllerBase
{
    private CatalogActor Actor() => new(UnitCommerceRequestContext.Actor(HttpContext), UnitCommerceRequestContext.CorrelationId(HttpContext));
    private IActionResult Reply<T>(T value) => Ok(ApiResponse<T>.Success(value));
    [HttpGet("capabilities"), PlatformPermission("platform.billing.read")]
    public IActionResult Capabilities() => Reply(SystemCapabilityCatalog.All);
    [HttpGet("systems"), PlatformPermission("platform.billing.read")]
    public async Task<IActionResult> Systems(CancellationToken ct) => Reply(await catalog.ListSystemsAsync(ct));
    [HttpPost("systems"), PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Create(CreateSystemRequest input, CancellationToken ct) => Reply(await catalog.CreateSystemAsync(input.Id, input.Code, input.CapacityMetric,
        new(input.NameAr, input.NameEn, input.Capabilities, input.IsActive, 0), Actor(), ct));
    [HttpPut("systems/{id:guid}"), PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Update(Guid id, SystemWrite input, CancellationToken ct) => Reply(await catalog.UpdateSystemAsync(id, input, Actor(), ct));
    [HttpGet("systems/{id:guid}/plans"), PlatformPermission("platform.billing.read")]
    public async Task<IActionResult> Plans(Guid id, CancellationToken ct) => Reply(await catalog.ListPlansAsync(id, ct));
    [HttpPost("systems/{id:guid}/plans"), PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> CreatePlan(Guid id, PlanCreate input, CancellationToken ct) => Reply(await catalog.CreatePlanAsync(id, input, Actor(), ct));
    [HttpPut("plans/{id:guid}"), PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Rename(Guid id, PlanWrite input, CancellationToken ct) => Reply(await catalog.UpdatePlanAsync(id, input, Actor(), ct));
    [HttpPost("plans/{id:guid}/revisions"), PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Revision(Guid id, RevisionRequest input, CancellationToken ct) => Reply(await catalog.AddRevisionAsync(id, input.ExpectedVersion, input.ExpectedSystemVersion, input.Offer, Actor(), ct));
    [HttpPost("plans/{id:guid}/revisions/{revision:int}/publish"), PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Publish(Guid id, int revision, CatalogVersionRequest input, CancellationToken ct) => Reply(await catalog.PublishAsync(id, revision, input.ExpectedVersion, input.ExpectedSystemVersion, Actor(), ct));
    [HttpPost("plans/{id:guid}/archive"), PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Archive(Guid id, CatalogVersionRequest input, CancellationToken ct) => Reply(await catalog.ArchiveAsync(id, input.ExpectedVersion, input.ExpectedSystemVersion, Actor(), ct));
    [HttpPut("systems/{id:guid}/plan-order"), PlatformPermission("platform.billing.manage")]
    public async Task<IActionResult> Order(Guid id, ReorderPlansRequest input, CancellationToken ct) { await catalog.ReorderAsync(id, input.ExpectedVersion, input.OrderedPlanIds, Actor(), ct); return Reply(new { changed = true }); }
    [HttpGet("plans/{id:guid}/next-plans"), PlatformPermission("platform.billing.read")]
    public async Task<IActionResult> Next(Guid id, CancellationToken ct) => Reply(await catalog.NextPlansAsync(id, ct));
}

public sealed record CreateSystemRequest(Guid Id, string Code, string NameAr, string NameEn, CapacityMetric CapacityMetric, string[] Capabilities, bool IsActive = true);
public sealed record RevisionRequest(long ExpectedVersion, long ExpectedSystemVersion, PlanOfferWrite Offer);
public sealed record CatalogVersionRequest(long ExpectedVersion, long ExpectedSystemVersion);
public sealed record ReorderPlansRequest(long ExpectedVersion, Guid[] OrderedPlanIds);
