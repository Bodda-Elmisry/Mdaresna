using System.Data;
using System.Text.Json;
using Mdaresna.Platform.Application.Billing.Subscriptions;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Billing.Subscriptions;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;

internal sealed class SystemCatalogService(PlatformDbContext db) : ISystemCatalogService
{
    public async Task<SystemCatalogItem[]> ListSystemsAsync(CancellationToken ct) => (await db.SchoolSystems.AsNoTracking().OrderBy(x => x.Code).ToArrayAsync(ct)).Select(Map).ToArray();
    public async Task<SystemCatalogItem> CreateSystemAsync(Guid id, string code, CapacityMetric metric, SystemWrite input, CatalogActor actor, CancellationToken ct)
    {
        if (input.ExpectedVersion != 0) throw new ArgumentException("A new system requires version zero.");
        var item = SchoolSystemDefinition.Create(id, code, input.NameAr, input.NameEn, metric, input.Capabilities, DateTimeOffset.UtcNow);
        if (!input.IsActive) item.Update(input.NameAr, input.NameEn, input.Capabilities, false, DateTimeOffset.UtcNow);
        if (await db.SchoolSystems.AnyAsync(x => x.Id == id || x.Code == item.Code, ct)) throw Conflict("catalog.system_exists");
        db.SchoolSystems.Add(item); Audit(actor, "catalog.system.created", id, new { item.Code });
        await Save(ct); return Map(item);
    }
    public async Task<SystemCatalogItem> UpdateSystemAsync(Guid id, SystemWrite input, CatalogActor actor, CancellationToken ct)
    {
        var item = await System(id, input.ExpectedVersion, ct);
        item.Update(input.NameAr, input.NameEn, input.Capabilities, input.IsActive, DateTimeOffset.UtcNow);
        Audit(actor, "catalog.system.updated", id, new { item.Version, item.IsActive, item.CapabilityCodes });
        await Save(ct); return Map(item);
    }
    public async Task<SystemPlanItem[]> ListPlansAsync(Guid systemId, CancellationToken ct)
    {
        if (!await db.SchoolSystems.AnyAsync(x => x.Id == systemId, ct)) throw Missing();
        var plans = await db.SystemPlans.AsNoTracking().Where(x => x.SystemId == systemId).OrderBy(x => x.SortOrder).ToArrayAsync(ct);
        var ids = plans.Select(x => x.Id).ToArray();
        var offers = await db.SystemPlanRevisions.AsNoTracking().Where(x => ids.Contains(x.PlanId)).OrderBy(x => x.Revision).ToArrayAsync(ct);
        return plans.Select(x => Map(x, offers.Where(o => o.PlanId == x.Id))).ToArray();
    }
    public async Task<SystemPlanItem> CreatePlanAsync(Guid systemId, PlanCreate input, CatalogActor actor, CancellationToken ct)
    {
        if (input.SortOrder > 1_000_000) throw new ArgumentException("Order exceeds the supported range.");
        var system = await System(systemId, input.ExpectedSystemVersion, ct);
        if (await db.SystemPlans.CountAsync(x => x.SystemId == systemId, ct) >= 1000) throw new ArgumentException("A system supports at most 1000 plans, including archived plans.");
        var plan = SystemPlan.Create(input.Id, systemId, input.Code, input.NameAr, input.NameEn, input.SortOrder, DateTimeOffset.UtcNow);
        var offer = NewOffer(plan, input.Offer);
        if (await db.SystemPlans.AnyAsync(x => x.Id == plan.Id || x.SystemId == systemId && (x.Code == plan.Code || x.SortOrder == plan.SortOrder), ct)) throw Conflict("catalog.plan_exists");
        system.Touch(DateTimeOffset.UtcNow); db.SystemPlans.Add(plan); db.SystemPlanRevisions.Add(offer);
        Audit(actor, "catalog.plan.created", plan.Id, new { plan.SystemId, plan.SortOrder, offer.Capacity, offer.BaseUnitPrice, offer.AddOnUnitPrice, offer.BaseAmount });
        await Save(ct); return Map(plan, [offer]);
    }
    public async Task<SystemPlanItem> UpdatePlanAsync(Guid id, PlanWrite input, CatalogActor actor, CancellationToken ct)
    {
        var (plan, system) = await Plan(id, input.ExpectedVersion, input.ExpectedSystemVersion, ct);
        plan.Rename(input.NameAr, input.NameEn); system.Touch(DateTimeOffset.UtcNow);
        Audit(actor, "catalog.plan.renamed", id, new { plan.NameAr, plan.NameEn }); await Save(ct); return await ReadPlan(plan, ct);
    }
    public async Task<SystemPlanItem> AddRevisionAsync(Guid id, long expectedVersion, long expectedSystemVersion, PlanOfferWrite input, CatalogActor actor, CancellationToken ct)
    {
        var (plan, system) = await Plan(id, expectedVersion, expectedSystemVersion, ct);
        var offer = NewOffer(plan, input); db.SystemPlanRevisions.Add(offer); system.Touch(DateTimeOffset.UtcNow);
        Audit(actor, "catalog.plan.revision.created", id, new { offer.Revision, offer.Capacity, offer.BaseUnitPrice, offer.AddOnUnitPrice, offer.Currency });
        await Save(ct); return await ReadPlan(plan, ct);
    }
    public async Task<SystemPlanItem> PublishAsync(Guid id, int revision, long expectedVersion, long expectedSystemVersion, CatalogActor actor, CancellationToken ct)
    {
        var (plan, system) = await Plan(id, expectedVersion, expectedSystemVersion, ct);
        if (!await db.SystemPlanRevisions.AnyAsync(x => x.PlanId == id && x.Revision == revision, ct)) throw Missing();
        plan.Publish(revision); system.Touch(DateTimeOffset.UtcNow);
        Audit(actor, "catalog.plan.published", id, new { revision }); await Save(ct); return await ReadPlan(plan, ct);
    }
    public async Task<SystemPlanItem> ArchiveAsync(Guid id, long expectedVersion, long expectedSystemVersion, CatalogActor actor, CancellationToken ct)
    {
        var (plan, system) = await Plan(id, expectedVersion, expectedSystemVersion, ct);
        plan.Archive(); system.Touch(DateTimeOffset.UtcNow);
        Audit(actor, "catalog.plan.archived", id, new { plan.Code }); await Save(ct); return await ReadPlan(plan, ct);
    }
    public async Task ReorderAsync(Guid systemId, long expectedVersion, Guid[] orderedPlanIds, CatalogActor actor, CancellationToken ct)
    {
        if (orderedPlanIds is null || orderedPlanIds.Length > 1000 || orderedPlanIds.Distinct().Count() != orderedPlanIds.Length)
            throw new ArgumentException("Supply every plan exactly once.");
        // Two flushes are one transaction: temporary ranks cannot leak, even under concurrent updates.
        var strategy = db.Database.CreateExecutionStrategy();
        var firstAttempt = true;
        await strategy.ExecuteAsync(async () => {
            if (!firstAttempt) db.ChangeTracker.Clear();
            firstAttempt = false;
            await using var tx = db.Database.IsRelational() ? await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct) : null;
            var system = await System(systemId, expectedVersion, ct);
            var plans = await db.SystemPlans.Where(x => x.SystemId == systemId).ToArrayAsync(ct);
            if (plans.Length != orderedPlanIds.Length || plans.Any(x => !orderedPlanIds.Contains(x.Id))) throw new ArgumentException("Supply every system plan exactly once, including archived plans.");
            system.Touch(DateTimeOffset.UtcNow);
            for (var i = 0; i < plans.Length; i++) plans[i].Move(int.MaxValue - i);
            await Save(ct);
            for (var i = 0; i < orderedPlanIds.Length; i++) plans.Single(x => x.Id == orderedPlanIds[i]).Move(i + 1);
            Audit(actor, "catalog.plans.reordered", systemId, new { orderedPlanIds }); await Save(ct);
            if (tx is not null) await tx.CommitAsync(ct);
        });
    }
    public async Task<SystemPlanItem[]> NextPlansAsync(Guid planId, CancellationToken ct)
    {
        var plan = await db.SystemPlans.AsNoTracking().SingleOrDefaultAsync(x => x.Id == planId, ct) ?? throw Missing();
        var system = await db.SchoolSystems.AsNoTracking().SingleAsync(x => x.Id == plan.SystemId, ct);
        if (!system.IsActive) return [];
        var plans = await ListPlansAsync(plan.SystemId, ct);
        var current = plans.Single(x => x.Id == planId);
        var capacity = current.Offers.SingleOrDefault(x => x.Revision == current.PublishedRevision)?.Capacity;
        return plans.Where(x => x.SortOrder > plan.SortOrder && !x.IsArchived && x.PublishedRevision.HasValue &&
            x.Offers.Any(o => o.Revision == x.PublishedRevision && (!capacity.HasValue || o.Capacity > capacity.Value))).ToArray();
    }
    private async Task<SchoolSystemDefinition> System(Guid id, long version, CancellationToken ct)
    {
        var item = await db.SchoolSystems.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw Missing();
        if (item.Version != version) throw Conflict("catalog.version_conflict"); return item;
    }
    private async Task<(SystemPlan, SchoolSystemDefinition)> Plan(Guid id, long version, long systemVersion, CancellationToken ct)
    {
        var plan = await db.SystemPlans.SingleOrDefaultAsync(x => x.Id == id, ct) ?? throw Missing();
        if (plan.Version != version) throw Conflict("catalog.version_conflict");
        return (plan, await System(plan.SystemId, systemVersion, ct));
    }
    private static SystemPlanRevision NewOffer(SystemPlan plan, PlanOfferWrite input)
    {
        var offer = SystemPlanRevision.Create(plan.Id, checked(plan.LatestRevision + 1), input.Capacity, input.BaseUnitPrice, input.AddOnUnitPrice, input.Currency, DateTimeOffset.UtcNow);
        plan.NextRevision(); return offer;
    }
    private async Task<SystemPlanItem> ReadPlan(SystemPlan plan, CancellationToken ct) => Map(plan, await db.SystemPlanRevisions.AsNoTracking().Where(x => x.PlanId == plan.Id).OrderBy(x => x.Revision).ToArrayAsync(ct));
    private static SystemCatalogItem Map(SchoolSystemDefinition x) => new(x.Id, x.Code, x.NameAr, x.NameEn, x.CapacityMetric, x.CapabilityCodes.Split('\n'), x.IsActive, x.Version);
    private static SystemPlanItem Map(SystemPlan x, IEnumerable<SystemPlanRevision> offers) => new(x.Id, x.SystemId, x.Code, x.NameAr, x.NameEn, x.SortOrder, x.IsArchived, x.Version, x.PublishedRevision,
        offers.Select(o => new PlanOffer(o.Revision, o.Capacity, o.BaseUnitPrice, o.AddOnUnitPrice, o.Currency, o.BaseAmount)).ToArray());
    private static PlatformConflictException Conflict(string code) => new(code, "Catalog changed or the requested item already exists. Refresh and retry.");
    private static PlatformResourceNotFoundException Missing() => new("catalog.not_found", "Catalog item was not found.");
    private async Task Save(CancellationToken ct)
    {
        try { await db.SaveChangesAsync(ct); }
        catch (DbUpdateConcurrencyException) { throw Conflict("catalog.version_conflict"); }
        catch (DbUpdateException ex) when (ex.InnerException is Npgsql.PostgresException { SqlState: "23505" } || ex.InnerException is Microsoft.Data.SqlClient.SqlException { Number: 2601 or 2627 })
        { throw Conflict("catalog.duplicate"); }
    }
    private void Audit(CatalogActor actor, string action, Guid id, object metadata)
    {
        if (actor.AccountId.IsEmpty || actor.CorrelationId == Guid.Empty) throw new ArgumentException("A validated actor and correlation are required.");
        db.AuditEntries.Add(new PlatformAuditEntry { Id = Guid.NewGuid(), AccountId = actor.AccountId, Action = action,
            ResourceType = "system-catalog", ResourceId = id.ToString("D"), OccurredAtUtc = DateTimeOffset.UtcNow,
            CorrelationId = actor.CorrelationId.ToString("D"), MetadataJson = JsonSerializer.Serialize(metadata) });
    }
}
