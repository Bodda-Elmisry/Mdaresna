using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;

internal sealed class PlatformPaymentRequestConfiguration : IEntityTypeConfiguration<PlatformPaymentRequest>
{
    public void Configure(EntityTypeBuilder<PlatformPaymentRequest> builder)
    {
        builder.ToTable("school_platform_payment_requests", "billing", table =>
        {
            table.HasTrigger("tr_billing_payment_request_approved_guard");
            table.HasCheckConstraint("ck_billing_payment_request_amount", "[Amount] > 0");
            table.HasCheckConstraint(
                "ck_billing_payment_request_currency",
                "[Currency] COLLATE Latin1_General_BIN2 LIKE '[A-Z][A-Z][A-Z]'");
            table.HasCheckConstraint(
                "ck_billing_payment_request_status",
                "[Status] IN (N'Pending', N'Approved', N'Rejected')");
            table.HasCheckConstraint(
                "ck_billing_payment_request_review",
                "([Status] = N'Pending' AND [ReviewedByAccountId] IS NULL " +
                "AND [ReviewedAtUtc] IS NULL AND [ReviewNote] IS NULL) OR " +
                "([Status] IN (N'Approved', N'Rejected') " +
                "AND [ReviewedByAccountId] IS NOT NULL AND [ReviewedAtUtc] IS NOT NULL " +
                "AND [ReviewedAtUtc] >= [RequestedAtUtc] " +
                "AND [ReviewedByAccountId] <> [RequestedByAccountId] " +
                "AND ([Status] = N'Approved' OR " +
                "([ReviewNote] IS NOT NULL AND LEN(LTRIM(RTRIM([ReviewNote]))) > 0)))");
            table.HasCheckConstraint(
                "ck_billing_payment_request_timestamps",
                "DATEPART(TZOFFSET, [RequestedAtUtc]) = 0 AND " +
                "([ReviewedAtUtc] IS NULL OR DATEPART(TZOFFSET, [ReviewedAtUtc]) = 0)");
            table.HasCheckConstraint("ck_billing_payment_request_version", "[Version] >= 0");
            table.HasCheckConstraint(
                "ck_billing_payment_request_identifiers",
                "[Id] <> '00000000-0000-0000-0000-000000000000' AND " +
                "[RequestedByAccountId] <> '00000000-0000-0000-0000-000000000000' AND " +
                "([ReviewedByAccountId] IS NULL OR " +
                "[ReviewedByAccountId] <> '00000000-0000-0000-0000-000000000000')");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value));
        builder.Property(x => x.SchoolId)
            .HasConversion(id => id.Value, value => SchoolId.From(value));
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.TransferReference).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.RequestedByAccountId)
            .HasConversion(id => id.Value, value => IdentityAccountId.From(value));
        builder.Property(x => x.ReviewedByAccountId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? IdentityAccountId.From(value.Value) : (IdentityAccountId?)null);
        builder.Property(x => x.RequestedAtUtc).HasPrecision(3);
        builder.Property(x => x.ReviewedAtUtc).HasPrecision(3);
        builder.Property(x => x.ReviewNote).HasMaxLength(1000);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Property<byte[]>("RowVersion").IsRequired().IsRowVersion();
        builder.Ignore(x => x.DomainEvents);

        builder.HasOne<SchoolRegistration>()
            .WithMany()
            .HasForeignKey(x => new { x.TenantId, x.SchoolId })
            .HasPrincipalKey(x => new { x.TenantId, x.Id })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.SchoolId, x.TransferReference }).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Status, x.RequestedAtUtc });
    }
}

internal sealed class PlatformPaymentLedgerEntryConfiguration : IEntityTypeConfiguration<PlatformPaymentLedgerEntry>
{
    public void Configure(EntityTypeBuilder<PlatformPaymentLedgerEntry> builder)
    {
        builder.ToTable("school_platform_payment_ledger", "billing", table =>
        {
            table.HasTrigger("tr_billing_payment_ledger_approved_insert");
            table.HasTrigger("tr_billing_payment_ledger_append_only");
            table.HasCheckConstraint("ck_billing_payment_ledger_amount", "[Amount] > 0");
            table.HasCheckConstraint(
                "ck_billing_payment_ledger_currency",
                "[Currency] COLLATE Latin1_General_BIN2 LIKE '[A-Z][A-Z][A-Z]'");
            table.HasCheckConstraint(
                "ck_billing_payment_ledger_posted_utc",
                "DATEPART(TZOFFSET, [PostedAtUtc]) = 0");
            table.HasCheckConstraint(
                "ck_billing_payment_ledger_identifiers",
                "[Id] <> '00000000-0000-0000-0000-000000000000' AND " +
                "[PaymentRequestId] <> '00000000-0000-0000-0000-000000000000' AND " +
                "[PostedByAccountId] <> '00000000-0000-0000-0000-000000000000'");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.PaymentRequestId).ValueGeneratedNever();
        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value));
        builder.Property(x => x.SchoolId)
            .HasConversion(id => id.Value, value => SchoolId.From(value));
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.TransferReference).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PostedByAccountId)
            .HasConversion(id => id.Value, value => IdentityAccountId.From(value));
        builder.Property(x => x.PostedAtUtc).HasPrecision(3);

        builder.HasOne<PlatformPaymentRequest>()
            .WithMany()
            .HasForeignKey(x => new
            {
                x.PaymentRequestId,
                x.TenantId,
                x.SchoolId,
                x.Amount,
                x.Currency,
                x.TransferReference
            })
            .HasPrincipalKey(x => new
            {
                x.Id,
                x.TenantId,
                x.SchoolId,
                x.Amount,
                x.Currency,
                x.TransferReference
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PaymentRequestId).IsUnique();
        builder.HasIndex(x => new { x.SchoolId, x.PostedAtUtc });
    }
}
