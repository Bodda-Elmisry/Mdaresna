using Mdaresna.Platform.Domain.Billing.Subscriptions;

namespace Mdaresna.Platform.UnitTests.Billing;

public sealed class SchoolSubscriptionTests
{
    [Theory]
    [InlineData(0, 30000)]
    [InlineData(12, 31380)]
    public void Initial_subscription_prices_additional_capacity_and_keeps_base_price_separate(int quantity, decimal total)
    {
        var now = DateTimeOffset.UtcNow;
        var system = SchoolSystemDefinition.Create(Guid.NewGuid(), "ACADEMIC", "أكاديمي", "Academic", CapacityMetric.Student, ["academic.students"], now);
        var plan = SystemPlan.Create(Guid.NewGuid(), system.Id, "PLAN-300", "باقة", "Plan", 1, now);
        var offer = SystemPlanRevision.Create(plan.Id, plan.NextRevision(), 300, 100, 115, "EGP", now); plan.Publish(1);
        var quote = SubscriptionQuote.Create(Guid.NewGuid(), Guid.NewGuid(), system, plan, offer, SubscriptionChangeKind.New, null, quantity, now, now);
        Assert.Equal(total, quote.AmountDue); Assert.Equal(30000m, quote.BaseAmount);
        quote.Submit(Guid.NewGuid(), now); var contract = SchoolSubscription.ApplyNew(quote);
        Assert.Equal(quantity, contract.AddOnCapacity); Assert.Equal(now.AddYears(1), contract.ValidUntilUtc);
        Assert.Throws<ArgumentException>(() => SubscriptionQuote.Create(Guid.NewGuid(), Guid.NewGuid(), system, plan, offer, SubscriptionChangeKind.New, null, -1, now, now));
        Assert.Throws<ArgumentException>(() => SubscriptionQuote.Create(Guid.NewGuid(), Guid.NewGuid(), system, plan, offer, SubscriptionChangeKind.New, null, 1000001, now, now));
    }
    [Fact]
    public void Renewal_uses_selected_addons_and_new_capabilities_while_existing_terms_remain_frozen()
    {
        var now = DateTimeOffset.UtcNow;
        var school = Guid.NewGuid();
        var system = SchoolSystemDefinition.Create(Guid.NewGuid(), "ACADEMIC", "أكاديمي", "Academic", CapacityMetric.Student, ["academic.students"], now);
        var plan = SystemPlan.Create(Guid.NewGuid(), system.Id, "PLAN-300", "باقة", "Plan", 1, now);
        var offer = SystemPlanRevision.Create(plan.Id, plan.NextRevision(), 300, 100, 100, "EGP", now); plan.Publish(1);
        var initial = SubscriptionQuote.Create(Guid.NewGuid(), school, system, plan, offer, SubscriptionChangeKind.New, null, 0, now, now);
        initial.Submit(Guid.NewGuid(), now); var current = SchoolSubscription.ApplyNew(initial);
        system.Update("أكاديمي", "Academic", ["academic.students", "academic.homework"], true, now.AddDays(1));
        var next = SystemPlanRevision.Create(plan.Id, plan.NextRevision(), 300, 110, 120, "EGP", now); plan.Publish(2);
        var addon = SubscriptionQuote.Create(Guid.NewGuid(), school, system, plan, offer, SubscriptionChangeKind.AddOn, current, 50, now, now);
        Assert.Equal(5000m, addon.AmountDue);
        Assert.DoesNotContain("academic.homework", addon.CapabilityCodes);
        addon.Submit(Guid.NewGuid(), now); current.Apply(addon);
        var renew = SubscriptionQuote.Create(Guid.NewGuid(), school, system, plan, next, SubscriptionChangeKind.Renew, current, 12, current.ValidUntilUtc, current.ValidUntilUtc);
        Assert.Equal(34440m, renew.AmountDue);
        Assert.Equal(12, renew.ResultAddOnCapacity);
        Assert.Contains("academic.homework", renew.CapabilityCodes);
        Assert.Equal("academic.students", current.CapabilityCodes);
        renew.Submit(Guid.NewGuid(), renew.ValidFromUtc); current.Apply(renew);
        Assert.Equal(12, current.AddOnCapacity);
        Assert.Equal(now.AddYears(2), current.ValidUntilUtc);
        Assert.Contains("academic.homework", current.CapabilityCodes);
    }

    [Fact]
    public void Equal_value_upgrade_is_free_and_keeps_capability_snapshot_and_addons()
    {
        var now = DateTimeOffset.UtcNow; var school = Guid.NewGuid();
        var system = SchoolSystemDefinition.Create(Guid.NewGuid(), "ACADEMIC", "أكاديمي", "Academic", CapacityMetric.Student, ["academic.students"], now);
        var plan = SystemPlan.Create(Guid.NewGuid(), system.Id, "PLAN-600", "باقة", "Plan", 1, now);
        var offer = SystemPlanRevision.Create(plan.Id, plan.NextRevision(), 600, 80, 100, "EGP", now); plan.Publish(1);
        var first = SubscriptionQuote.Create(Guid.NewGuid(), school, system, plan, offer, SubscriptionChangeKind.New, null, 0, now, now);
        first.Submit(Guid.NewGuid(), now); var current = SchoolSubscription.ApplyNew(first);
        system.Update("أكاديمي", "Academic", ["academic.students", "academic.homework"], true, now);
        var target = SystemPlan.Create(Guid.NewGuid(), system.Id, "PLAN-800", "باقة أكبر", "Larger", 2, now);
        var larger = SystemPlanRevision.Create(target.Id, target.NextRevision(), 800, 60, 100, "EGP", now); target.Publish(1);
        var upgrade = SubscriptionQuote.Create(Guid.NewGuid(), school, system, target, larger, SubscriptionChangeKind.Upgrade, current, 0, now, now);
        Assert.Equal(0m, upgrade.AmountDue);
        Assert.Throws<ArgumentException>(() => upgrade.Submit(Guid.NewGuid(), now));
        Assert.Throws<ArgumentException>(() => current.Apply(upgrade));
        upgrade.Submit(null, now); current.Apply(upgrade);
        Assert.Equal(800, current.BaseCapacity);
        Assert.Equal("academic.students", current.CapabilityCodes);
    }
    [Fact]
    public void Annual_subscription_addons_and_upgrade_preserve_purchased_capacity()
    {
        var now = DateTimeOffset.UtcNow; var schoolId = Guid.NewGuid();
        var system = SchoolSystemDefinition.Create(Guid.NewGuid(), "ACADEMIC", "أكاديمي", "Academic", CapacityMetric.Student, ["academic.students"], now);
        var first = SystemPlan.Create(Guid.NewGuid(), system.Id, "FIRST-PLAN", "الأولى", "First", 1, now);
        var firstOffer = SystemPlanRevision.Create(first.Id, first.NextRevision(), 300, 100, 115, "EGP", now); first.Publish(1);
        var quote = SubscriptionQuote.Create(Guid.NewGuid(), schoolId, system, first, firstOffer, SubscriptionChangeKind.New, null, 0, now, now);
        quote.Submit(Guid.NewGuid(), now); var subscription = SchoolSubscription.ApplyNew(quote); quote.Complete(true, now);
        Assert.Equal(now.AddYears(1), subscription.ValidUntilUtc); Assert.Equal(30000m, quote.AmountDue);
        var add = SubscriptionQuote.Create(Guid.NewGuid(), schoolId, system, first, firstOffer, SubscriptionChangeKind.AddOn, subscription, 50, now, now);
        Assert.Equal(5750m, add.AmountDue); add.Submit(Guid.NewGuid(), now); subscription.Apply(add); add.Complete(true, now);
        var second = SystemPlan.Create(Guid.NewGuid(), system.Id, "SECOND-PLAN", "الثانية", "Second", 2, now);
        var secondOffer = SystemPlanRevision.Create(second.Id, second.NextRevision(), 600, 80, 120, "EGP", now); second.Publish(1);
        var upgrade = SubscriptionQuote.Create(Guid.NewGuid(), schoolId, system, second, secondOffer, SubscriptionChangeKind.Upgrade, subscription, 0, now, now);
        Assert.Equal(18000m, upgrade.AmountDue); Assert.Equal(50, upgrade.ResultAddOnCapacity);
        upgrade.Submit(Guid.NewGuid(), now); subscription.Apply(upgrade); upgrade.Complete(true, now);
        Assert.Equal(650, subscription.BaseCapacity + subscription.AddOnCapacity); Assert.Equal(now.AddYears(1), subscription.ValidUntilUtc);
        Assert.Throws<ArgumentException>(() => subscription.Apply(upgrade));
    }
    [Fact]
    public void Quotes_expire_and_zero_cost_upgrade_needs_separate_approval()
    {
        var now = DateTimeOffset.UtcNow;
        var system = SchoolSystemDefinition.Create(Guid.NewGuid(), "CUSTOM", "نظام", "System", CapacityMetric.Student, ["academic.students"], now);
        var plan = SystemPlan.Create(Guid.NewGuid(), system.Id, "CUSTOM-PLAN", "باقة", "Plan", 1, now);
        var offer = SystemPlanRevision.Create(plan.Id, plan.NextRevision(), 425, 87.5m, 115m, "EGP", now); plan.Publish(1);
        var quote = SubscriptionQuote.Create(Guid.NewGuid(), Guid.NewGuid(), system, plan, offer, SubscriptionChangeKind.New, null, 0, now, now);
        Assert.Throws<ArgumentException>(() => quote.Submit(Guid.NewGuid(), now.AddHours(1)));
        Assert.Throws<ArgumentException>(() => quote.Submit(null, now));
        Assert.Throws<ArgumentException>(() => quote.Complete(true, now));
    }
}
