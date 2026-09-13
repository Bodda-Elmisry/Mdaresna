using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Billing.Units;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;

internal sealed class UnitTypeConfiguration : IEntityTypeConfiguration<UnitType>
{
    public void Configure(EntityTypeBuilder<UnitType> builder)
    {
        builder.ToTable("unit_types", "billing", table =>
        {
            table.HasCheckConstraint("ck_billing_unit_type_price", "[UnitPrice] > 0");
            table.HasCheckConstraint("ck_billing_unit_type_code",
                "LEN([Code]) BETWEEN 3 AND 32 AND [Code] COLLATE Latin1_General_BIN2 = UPPER([Code])");
            table.HasCheckConstraint("ck_billing_unit_type_currency",
                "[Currency] COLLATE Latin1_General_BIN2 LIKE '[A-Z][A-Z][A-Z]'");
            table.HasCheckConstraint("ck_billing_unit_type_name",
                "LEN(LTRIM(RTRIM([DisplayName]))) > 0");
            table.HasCheckConstraint("ck_billing_unit_type_timestamps",
                "[CreatedAtUtc] <= [UpdatedAtUtc] AND " +
                "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND " +
                "DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
            table.HasCheckConstraint("ck_billing_unit_type_version", "[Version] >= 0");
            table.HasCheckConstraint("ck_billing_unit_type_id",
                "[Id] <> '00000000-0000-0000-0000-000000000000'");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Code).HasMaxLength(32).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.UpdatedAtUtc).HasPrecision(3);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Property<byte[]>("RowVersion").IsRequired().IsRowVersion();
        builder.Ignore(x => x.DomainEvents);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.IsActive);
    }
}

internal sealed class UnitPurchaseIntentConfiguration : IEntityTypeConfiguration<UnitPurchaseIntent>
{
    public void Configure(EntityTypeBuilder<UnitPurchaseIntent> builder)
    {
        builder.ToTable("unit_purchase_intents", "billing", table =>
        {
            table.HasTrigger("tr_billing_unit_purchase_intent_append_only");
            table.HasTrigger("tr_billing_unit_purchase_intent_pending_insert");
            table.HasCheckConstraint("ck_billing_unit_purchase_intent_school",
                "[SchoolId] = [TenantId]");
            table.HasCheckConstraint("ck_billing_unit_purchase_intent_quantity",
                "[Quantity] BETWEEN 1 AND 1000000");
            table.HasCheckConstraint("ck_billing_unit_purchase_intent_price",
                "[UnitPrice] > 0 AND [Amount] > 0 AND [Amount] = [UnitPrice] * [Quantity]");
            table.HasCheckConstraint("ck_billing_unit_purchase_intent_version",
                "[UnitTypeVersion] >= 0");
            table.HasCheckConstraint("ck_billing_unit_purchase_intent_currency",
                "[Currency] COLLATE Latin1_General_BIN2 LIKE '[A-Z][A-Z][A-Z]'");
            table.HasCheckConstraint("ck_billing_unit_purchase_intent_code",
                "LEN([UnitTypeCode]) BETWEEN 3 AND 32 AND " +
                "[UnitTypeCode] COLLATE Latin1_General_BIN2 = UPPER([UnitTypeCode])");
            table.HasCheckConstraint("ck_billing_unit_purchase_intent_name",
                "LEN(LTRIM(RTRIM([UnitTypeName]))) > 0");
            table.HasCheckConstraint("ck_billing_unit_purchase_intent_payment_method",
                "LEN([PaymentMethodCode]) BETWEEN 3 AND 40 AND " +
                "[PaymentMethodCode] COLLATE Latin1_General_BIN2 = LOWER([PaymentMethodCode])");
            table.HasCheckConstraint("ck_billing_unit_purchase_intent_created_utc",
                "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND " +
                "DATEPART(TZOFFSET, [TransferOccurredAtUtc]) = 0 AND " +
                "[TransferOccurredAtUtc] <= [CreatedAtUtc]");
            table.HasCheckConstraint("ck_billing_unit_purchase_intent_ids",
                "[PaymentRequestId] <> '00000000-0000-0000-0000-000000000000' AND " +
                "[UnitTypeId] <> '00000000-0000-0000-0000-000000000000'");
        });

        builder.HasKey(x => x.PaymentRequestId);
        builder.Property(x => x.PaymentRequestId).ValueGeneratedNever();
        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value));
        builder.Property(x => x.SchoolId)
            .HasConversion(id => id.Value, value => SchoolId.From(value));
        builder.Property(x => x.UnitTypeId).ValueGeneratedNever();
        builder.Property(x => x.UnitTypeCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.UnitTypeName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.UnitTypeVersion);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.PaymentMethodCode).HasMaxLength(40).IsRequired();
        builder.Property(x => x.TransferOccurredAtUtc).HasPrecision(3);
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);

        // The immutable snapshot must match the reported payment total and school.
        builder.HasOne<PlatformPaymentRequest>()
            .WithMany()
            .HasForeignKey(x => new
            {
                x.PaymentRequestId, x.TenantId, x.SchoolId, x.Amount, x.Currency
            })
            .HasPrincipalKey(x => new
            {
                x.Id, x.TenantId, x.SchoolId, x.Amount, x.Currency
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<UnitType>()
            .WithMany()
            .HasForeignKey(x => x.UnitTypeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.SchoolId, x.CreatedAtUtc });
        builder.HasIndex(x => x.UnitTypeId);
    }
}

internal sealed class UnitGrantConfiguration : IEntityTypeConfiguration<UnitGrant>
{
    public void Configure(EntityTypeBuilder<UnitGrant> builder)
    {
        builder.ToTable("unit_grants", "billing", table =>
        {
            table.HasTrigger("tr_billing_unit_grant_approved_insert");
            table.HasTrigger("tr_billing_unit_grant_append_only");
            table.HasCheckConstraint("ck_billing_unit_grant_school", "[SchoolId] = [TenantId]");
            table.HasCheckConstraint("ck_billing_unit_grant_quantity",
                "[Quantity] BETWEEN 1 AND 1000000");
            table.HasCheckConstraint("ck_billing_unit_grant_price",
                "[UnitPrice] > 0 AND [Amount] > 0 AND [Amount] = [UnitPrice] * [Quantity]");
            table.HasCheckConstraint("ck_billing_unit_grant_currency",
                "[Currency] COLLATE Latin1_General_BIN2 LIKE '[A-Z][A-Z][A-Z]'");
            table.HasCheckConstraint("ck_billing_unit_grant_name",
                "LEN(LTRIM(RTRIM([UnitTypeName]))) > 0");
            table.HasCheckConstraint("ck_billing_unit_grant_payment_method",
                "LEN([PaymentMethodCode]) BETWEEN 3 AND 40 AND " +
                "[PaymentMethodCode] COLLATE Latin1_General_BIN2 = LOWER([PaymentMethodCode])");
            table.HasCheckConstraint("ck_billing_unit_grant_granted_utc",
                "DATEPART(TZOFFSET, [GrantedAtUtc]) = 0 AND " +
                "DATEPART(TZOFFSET, [TransferOccurredAtUtc]) = 0 AND " +
                "[TransferOccurredAtUtc] <= [GrantedAtUtc]");
            table.HasCheckConstraint("ck_billing_unit_grant_ids",
                "[Id] <> '00000000-0000-0000-0000-000000000000' AND " +
                "[PaymentRequestId] <> '00000000-0000-0000-0000-000000000000' AND " +
                "[UnitTypeId] <> '00000000-0000-0000-0000-000000000000'");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.PaymentRequestId).ValueGeneratedNever();
        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value));
        builder.Property(x => x.SchoolId)
            .HasConversion(id => id.Value, value => SchoolId.From(value));
        builder.Property(x => x.UnitTypeId).ValueGeneratedNever();
        builder.Property(x => x.UnitTypeCode).HasMaxLength(32).IsRequired();
        builder.Property(x => x.UnitTypeName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.UnitPrice).HasPrecision(18, 2);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.Currency).HasMaxLength(3).IsRequired();
        builder.Property(x => x.TransferReference).HasMaxLength(100).IsRequired();
        builder.Property(x => x.PaymentMethodCode).HasMaxLength(40).IsRequired();
        builder.Property(x => x.TransferOccurredAtUtc).HasPrecision(3);
        builder.Property(x => x.GrantedAtUtc).HasPrecision(3);

        // One immutable entitlement per approved purchase, with every snapshot field matching.
        builder.HasOne<UnitPurchaseIntent>()
            .WithMany()
            .HasForeignKey(x => new
            {
                x.PaymentRequestId, x.TenantId, x.SchoolId, x.UnitTypeId,
                x.UnitTypeCode, x.UnitTypeName, x.UnitPrice, x.Currency,
                x.Quantity, x.Amount, x.PaymentMethodCode, x.TransferOccurredAtUtc
            })
            .HasPrincipalKey(x => new
            {
                x.PaymentRequestId, x.TenantId, x.SchoolId, x.UnitTypeId,
                x.UnitTypeCode, x.UnitTypeName, x.UnitPrice, x.Currency,
                x.Quantity, x.Amount, x.PaymentMethodCode, x.TransferOccurredAtUtc
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<PlatformPaymentRequest>()
            .WithMany()
            .HasForeignKey(x => new
            {
                x.PaymentRequestId, x.TenantId, x.SchoolId, x.Amount,
                x.Currency, x.TransferReference
            })
            .HasPrincipalKey(x => new
            {
                x.Id, x.TenantId, x.SchoolId, x.Amount,
                x.Currency, x.TransferReference
            })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PaymentRequestId).IsUnique();
        builder.HasIndex(x => new { x.SchoolId, x.GrantedAtUtc });
    }
}
