using Mdaresna.Platform.Domain.Billing.Subscriptions;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;

internal static class PlatformInvoiceConfiguration
{
    public static void Apply(ModelBuilder model)
    {
        var invoice = model.Entity<PlatformInvoice>();
        invoice.ToTable("invoices", "billing", t => t.HasCheckConstraint("ck_invoice_amount", "[Amount] > 0"));
        invoice.HasKey(x => x.Id); invoice.Property(x => x.Id).ValueGeneratedNever();
        invoice.Property(x => x.Number).HasMaxLength(40).IsRequired(); invoice.HasIndex(x => x.Number).IsUnique();
        invoice.HasIndex(x => x.RegistrationRequestId).IsUnique().HasFilter("[RegistrationRequestId] IS NOT NULL");
        invoice.HasIndex(x => new { x.SchoolId, x.CreatedAtUtc });
        invoice.Property(x => x.Amount).HasPrecision(18, 2); invoice.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        invoice.Property(x => x.Version).IsConcurrencyToken();
        var line = model.Entity<PlatformInvoiceLine>();
        line.ToTable("invoice_lines", "billing"); line.HasKey(x => x.Id); line.Property(x => x.Id).ValueGeneratedNever();
        line.HasIndex(x => x.QuoteId).IsUnique();
        line.HasOne<PlatformInvoice>().WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        line.HasOne<SubscriptionQuote>().WithMany().HasForeignKey(x => x.QuoteId).OnDelete(DeleteBehavior.Restrict);
        line.Property(x => x.SystemNameAr).HasMaxLength(200).IsRequired(); line.Property(x => x.SystemNameEn).HasMaxLength(200).IsRequired();
        line.Property(x => x.PlanNameAr).HasMaxLength(200).IsRequired(); line.Property(x => x.PlanNameEn).HasMaxLength(200).IsRequired();
        var receipt = model.Entity<PlatformInvoiceReceipt>();
        receipt.ToTable("invoice_receipts", "billing"); receipt.HasKey(x => x.PaymentRequestId); receipt.Property(x => x.PaymentRequestId).ValueGeneratedNever();
        receipt.HasOne<PlatformInvoice>().WithMany().HasForeignKey(x => x.InvoiceId).OnDelete(DeleteBehavior.Restrict);
        receipt.HasOne<Mdaresna.Platform.Domain.Billing.PlatformPaymentRequest>().WithMany().HasForeignKey(x => x.PaymentRequestId).OnDelete(DeleteBehavior.Restrict);
        receipt.Property(x => x.Reference).HasMaxLength(100).IsRequired(); receipt.Property(x => x.RepresentativeName).HasMaxLength(200);
        var offer = model.Entity<SchoolRegistrationOffer>();
        offer.ToTable("registration_subscription_offers", "billing"); offer.HasKey(x => x.Id); offer.Property(x => x.Id).ValueGeneratedNever();
        offer.Property(x => x.RequestHash).HasMaxLength(64).IsRequired(); offer.Property(x => x.QuoteIdsJson).HasMaxLength(4000).IsRequired();
    }
}
