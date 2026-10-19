using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Billing.Subscriptions;
using Mdaresna.Platform.Application.Billing.Subscriptions;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mdaresna.Platform.UnitTests.Billing;

public sealed class SystemCatalogTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.UtcNow;
    [Fact]
    public void Custom_prices_are_calculated_without_fixed_tiers_and_old_offers_stay_unchanged()
    {
        var plan = SystemPlan.Create(Guid.NewGuid(), Guid.NewGuid(), "CUSTOM-425", "مخصصة", "Custom", 20, Now);
        var first = SystemPlanRevision.Create(plan.Id, plan.NextRevision(), 425, 87.5m, 115m, "EGP", Now);
        plan.Publish(1);
        var second = SystemPlanRevision.Create(plan.Id, plan.NextRevision(), 913, 21.4321m, 69.8765m, "EGP", Now);
        Assert.Equal(37187.50m, first.BaseAmount); Assert.Equal(115m, first.AddOnUnitPrice);
        Assert.Equal(425, first.Capacity); Assert.Equal(1, plan.PublishedRevision);
        Assert.Equal(decimal.Round(913 * 21.4321m, 2, MidpointRounding.AwayFromZero), second.BaseAmount);
        plan.Publish(2); Assert.Equal(2, plan.PublishedRevision);
    }
    [Theory]
    [InlineData(0, 10, 20)] [InlineData(1000001, 10, 20)] [InlineData(3, 0, 2)] [InlineData(3, 10, -2)]
    public void Invalid_offer_is_rejected(int capacity, decimal basePrice, decimal extraPrice) =>
        Assert.Throws<ArgumentException>(() => SystemPlanRevision.Create(Guid.NewGuid(), 1, capacity, basePrice, extraPrice, "EGP", Now));
    [Fact]
    public void Unknown_and_wrong_metric_capabilities_are_rejected()
    {
        Assert.Throws<ArgumentException>(() => SystemPlanRevision.Create(Guid.NewGuid(), 1, 1, .0001m, 1m, "EGP", Now));
        Assert.Throws<ArgumentException>(() => SchoolSystemDefinition.Create(Guid.NewGuid(), "TEST-SYSTEM", "اختبار", "Test", CapacityMetric.Student, ["unknown"], Now));
        Assert.Throws<ArgumentException>(() => SchoolSystemDefinition.Create(Guid.NewGuid(), "TEST-SYSTEM", "اختبار", "Test", CapacityMetric.Student, ["administration.payroll"], Now));
    }
    [Fact]
    public async Task Catalog_publishes_reorders_and_filters_next_by_rank_capacity_and_status()
    {
        await using var db = new PlatformDbContext(new DbContextOptionsBuilder<PlatformDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var serviceType = typeof(PlatformDbContext).Assembly.GetType("Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing.SystemCatalogService")!;
        var service = (ISystemCatalogService)Activator.CreateInstance(serviceType, db)!;
        var actor = new CatalogActor(IdentityAccountId.From(Guid.NewGuid()), Guid.NewGuid());
        var systemId = Guid.NewGuid();
        await service.CreateSystemAsync(systemId, "ACADEMIC-CUSTOM", CapacityMetric.Student,
            new("أكاديمي", "Academic", ["academic.students", "academic.staff"], true, 0), actor, default);
        async Task<SystemPlanItem> Create(int capacity, int rank)
        {
            var system = (await service.ListSystemsAsync(default)).Single();
            return await service.CreatePlanAsync(system.Id, new(Guid.NewGuid(), "PLAN-" + rank, "باقة", "Plan", rank, system.Version, new(capacity, 12.5m, 99.9m, "EGP")), actor, default);
        }
        async Task<SystemPlanItem> Publish(SystemPlanItem item)
        {
            var system = (await service.ListSystemsAsync(default)).Single();
            return await service.PublishAsync(item.Id, 1, item.Version, system.Version, actor, default);
        }
        var small = await Publish(await Create(123, 10));
        var large = await Publish(await Create(456, 20));
        var draft = await Create(789, 30);
        Assert.Equal(large.Id, Assert.Single(await service.NextPlansAsync(small.Id, default)).Id);
        var version = (await service.ListSystemsAsync(default)).Single().Version;
        await service.ReorderAsync(systemId, version, [large.Id, small.Id, draft.Id], actor, default);
        Assert.Empty(await service.NextPlansAsync(small.Id, default));
        await Assert.ThrowsAsync<PlatformConflictException>(() => service.ReorderAsync(systemId, version, [small.Id, large.Id, draft.Id], actor, default));
        Assert.Equal(7, await db.AuditEntries.CountAsync());
    }
    [Fact]
    public void Both_provider_models_include_unique_plan_order_and_decimal_prices()
    {
        foreach (var postgres in new[] { false, true })
        {
            var builder = new DbContextOptionsBuilder<PlatformDbContext>();
            if (postgres) builder.UseNpgsql("Host=localhost;Database=model_only;Username=fixture;Password=fixture");
            else builder.UseSqlServer("Server=localhost;Database=model_only;User Id=fixture;Password=fixture;TrustServerCertificate=true");
            using var db = new PlatformDbContext(builder.Options);
            var plan = db.Model.FindEntityType(typeof(SystemPlan))!;
            Assert.Contains(plan.GetIndexes(), x => x.IsUnique && x.Properties.Select(p => p.Name).SequenceEqual(new[] { "SystemId", "SortOrder" }));
            Assert.Equal(4, db.Model.FindEntityType(typeof(SystemPlanRevision))!.FindProperty("BaseUnitPrice")!.GetScale());
        }
    }
}
