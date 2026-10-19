using Mdaresna.Platform.Domain.Billing.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;

internal static class SystemCatalogConfiguration
{
    internal static void Apply(ModelBuilder model)
    {
        var system = model.Entity<SchoolSystemDefinition>();
        system.ToTable("school_systems", "billing"); system.HasKey(x => x.Id);
        system.Property(x => x.Id).ValueGeneratedNever();
        system.Property(x => x.Code).HasMaxLength(32).IsRequired(); system.HasIndex(x => x.Code).IsUnique();
        system.Property(x => x.NameAr).HasMaxLength(200).IsRequired(); system.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        system.Property(x => x.CapabilityCodes).HasMaxLength(4000).IsRequired(); system.Property(x => x.Version).IsConcurrencyToken();

        var plan = model.Entity<SystemPlan>();
        plan.ToTable("system_plans", "billing", t => t.HasCheckConstraint("ck_system_plan_order", "[SortOrder] > 0"));
        plan.HasKey(x => x.Id); plan.Property(x => x.Id).ValueGeneratedNever();
        plan.Property(x => x.Code).HasMaxLength(32).IsRequired();
        plan.Property(x => x.NameAr).HasMaxLength(200).IsRequired(); plan.Property(x => x.NameEn).HasMaxLength(200).IsRequired();
        plan.Property(x => x.Version).IsConcurrencyToken();
        plan.HasIndex(x => new { x.SystemId, x.Code }).IsUnique(); plan.HasIndex(x => new { x.SystemId, x.SortOrder }).IsUnique();
        plan.HasOne<SchoolSystemDefinition>().WithMany().HasForeignKey(x => x.SystemId).OnDelete(DeleteBehavior.Restrict);

        var offer = model.Entity<SystemPlanRevision>();
        offer.ToTable("system_plan_revisions", "billing", t => {
            t.HasCheckConstraint("ck_system_plan_revision_capacity", "[Capacity] BETWEEN 1 AND 1000000");
            t.HasCheckConstraint("ck_system_plan_revision_prices", "[BaseUnitPrice] > 0 AND [AddOnUnitPrice] > 0 AND [BaseAmount] > 0");
        });
        offer.HasKey(x => new { x.PlanId, x.Revision });
        offer.Property(x => x.BaseUnitPrice).HasPrecision(18, 4); offer.Property(x => x.AddOnUnitPrice).HasPrecision(18, 4);
        offer.Property(x => x.BaseAmount).HasPrecision(18, 2); offer.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        offer.HasOne<SystemPlan>().WithMany().HasForeignKey(x => x.PlanId).OnDelete(DeleteBehavior.Restrict);

        var subscription = model.Entity<SchoolSubscription>();
        subscription.ToTable("school_system_subscriptions", "billing", t => {
            t.HasCheckConstraint("ck_school_subscription_capacity", "[BaseCapacity] > 0 AND [AddOnCapacity] >= 0");
            t.HasCheckConstraint("ck_school_subscription_period", "[ValidUntilUtc] > [ValidFromUtc]");
        });
        subscription.HasKey(x => x.Id); subscription.Property(x => x.Id).ValueGeneratedNever();
        subscription.HasIndex(x => new { x.SchoolId, x.SystemId }).IsUnique();
        subscription.Property(x => x.Version).IsConcurrencyToken();
        subscription.Property(x => x.BaseAmount).HasPrecision(18, 2); subscription.Property(x => x.AddOnUnitPrice).HasPrecision(18, 4);
        subscription.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        subscription.Property(x => x.CapabilityCodes).HasMaxLength(4000).IsRequired();
        subscription.HasOne<SchoolSystemDefinition>().WithMany().HasForeignKey(x => x.SystemId).OnDelete(DeleteBehavior.Restrict);
        subscription.HasOne<SystemPlanRevision>().WithMany().HasForeignKey(x => new { x.PlanId, Revision = x.PlanRevision }).OnDelete(DeleteBehavior.Restrict);

        var delivery = model.Entity<SchoolSubscriptionDelivery>();
        delivery.ToTable("subscription_delivery_receipts", "billing"); delivery.HasKey(x => x.SubscriptionId);
        delivery.Property(x => x.SubscriptionId).ValueGeneratedNever(); delivery.Property(x => x.AppliedRevision).IsConcurrencyToken();
        delivery.HasOne<SchoolSubscription>().WithOne().HasForeignKey<SchoolSubscriptionDelivery>(x => x.SubscriptionId).OnDelete(DeleteBehavior.Restrict);

        var quote = model.Entity<SubscriptionQuote>();
        quote.ToTable("subscription_quotes", "billing", t => {
            t.HasCheckConstraint("ck_subscription_quote_amount", "[AmountDue] >= 0 AND [BaseAmount] > 0");
            t.HasCheckConstraint("ck_subscription_quote_capacity", "[BaseCapacity] > 0 AND [ResultAddOnCapacity] >= 0");
        });
        quote.HasKey(x => x.Id); quote.Property(x => x.Id).ValueGeneratedNever(); quote.Property(x => x.Version).IsConcurrencyToken();
        quote.Property(x => x.AmountDue).HasPrecision(18, 2); quote.Property(x => x.BaseAmount).HasPrecision(18, 2);
        quote.Property(x => x.AddOnUnitPrice).HasPrecision(18, 4); quote.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        quote.Property(x => x.CapabilityCodes).HasMaxLength(4000).IsRequired();
        quote.HasIndex(x => new { x.SchoolId, x.ExpiresAtUtc });
        quote.HasIndex(x => x.PaymentRequestId).HasFilter("[PaymentRequestId] IS NOT NULL");
        quote.HasOne<PlatformInvoice>().WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        quote.HasOne<Mdaresna.Platform.Domain.Billing.PlatformPaymentRequest>().WithMany().HasForeignKey(x => x.PaymentRequestId).OnDelete(DeleteBehavior.Restrict);
        quote.HasOne<SystemPlanRevision>().WithMany().HasForeignKey(x => new { x.PlanId, Revision = x.PlanRevision }).OnDelete(DeleteBehavior.Restrict);
    }
}
