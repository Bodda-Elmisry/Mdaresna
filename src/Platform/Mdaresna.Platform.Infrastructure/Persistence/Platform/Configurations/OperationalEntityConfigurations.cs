using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Configurations;

internal sealed class PlatformAuditEntryConfiguration : IEntityTypeConfiguration<PlatformAuditEntry>
{
    public void Configure(EntityTypeBuilder<PlatformAuditEntry> builder)
    {
        builder.ToTable("audit_entries", "operations", table =>
            table.HasCheckConstraint(
                "ck_operations_audit_entries_occurred_utc",
                "DATEPART(TZOFFSET, [OccurredAtUtc]) = 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Action).HasMaxLength(150).IsRequired();
        builder.Property(x => x.ResourceType).HasMaxLength(150).IsRequired();
        builder.Property(x => x.ResourceId).HasMaxLength(200);
        builder.Property(x => x.AccountId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? IdentityAccountId.From(value.Value) : (IdentityAccountId?)null);
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? TenantId.From(value.Value) : (TenantId?)null);
        builder.Property(x => x.OccurredAtUtc).HasPrecision(3);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(1000);
        builder.Property(x => x.CorrelationId).HasMaxLength(100);
        builder.Property(x => x.MetadataJson).HasColumnType("nvarchar(max)");
        builder.HasIndex(x => new { x.TenantId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.AccountId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.ResourceType, x.ResourceId });
    }
}

internal sealed class PlatformFeatureFlagConfiguration : IEntityTypeConfiguration<PlatformFeatureFlag>
{
    public void Configure(EntityTypeBuilder<PlatformFeatureFlag> builder)
    {
        builder.ToTable("feature_flags", "operations", table =>
            table.HasCheckConstraint(
                "ck_operations_feature_flags_timestamps",
                "[CreatedAtUtc] <= [UpdatedAtUtc] " +
                "AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 " +
                "AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Key).HasMaxLength(150).IsRequired();
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? TenantId.From(value.Value) : (TenantId?)null);
        builder.Property(x => x.Description).HasMaxLength(1000);
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.UpdatedAtUtc).HasPrecision(3);
        builder.Property(x => x.UpdatedByAccountId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? IdentityAccountId.From(value.Value) : (IdentityAccountId?)null);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Key)
            .IsUnique()
            .HasFilter("[TenantId] IS NULL");
        builder.HasIndex(x => new { x.TenantId, x.Key })
            .IsUnique()
            .HasFilter("[TenantId] IS NOT NULL");
    }
}

internal sealed class PlatformOutboxMessageConfiguration : IEntityTypeConfiguration<PlatformOutboxMessage>
{
    public void Configure(EntityTypeBuilder<PlatformOutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "messaging", table =>
            table.HasCheckConstraint(
                "ck_messaging_platform_outbox_timestamps",
                "DATEPART(TZOFFSET, [OccurredAtUtc]) = 0 " +
                "AND ([ProcessedAtUtc] IS NULL OR ([ProcessedAtUtc] >= [OccurredAtUtc] " +
                "AND DATEPART(TZOFFSET, [ProcessedAtUtc]) = 0)) " +
                "AND ([NextAttemptAtUtc] IS NULL OR ([NextAttemptAtUtc] >= [OccurredAtUtc] " +
                "AND DATEPART(TZOFFSET, [NextAttemptAtUtc]) = 0))"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.MessageType).HasMaxLength(300).IsRequired();
        builder.Property(x => x.PayloadJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.OccurredAtUtc).HasPrecision(3);
        builder.Property(x => x.ProcessedAtUtc).HasPrecision(3);
        builder.Property(x => x.NextAttemptAtUtc).HasPrecision(3);
        builder.Property(x => x.LastError).HasMaxLength(4000);
        builder.Property(x => x.TraceParent).HasMaxLength(512);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.ProcessedAtUtc, x.NextAttemptAtUtc })
            .HasFilter("[ProcessedAtUtc] IS NULL");
    }
}

internal sealed class PlatformInboxMessageConfiguration : IEntityTypeConfiguration<PlatformInboxMessage>
{
    public void Configure(EntityTypeBuilder<PlatformInboxMessage> builder)
    {
        builder.ToTable("inbox_messages", "messaging", table =>
            table.HasCheckConstraint(
                "ck_messaging_platform_inbox_timestamps",
                "DATEPART(TZOFFSET, [ReceivedAtUtc]) = 0 " +
                "AND ([ProcessedAtUtc] IS NULL OR ([ProcessedAtUtc] >= [ReceivedAtUtc] " +
                "AND DATEPART(TZOFFSET, [ProcessedAtUtc]) = 0))"));
        builder.HasKey(x => new { x.Consumer, x.MessageId });
        builder.Property(x => x.Consumer).HasMaxLength(200).IsRequired();
        builder.Property(x => x.MessageType).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ReceivedAtUtc).HasPrecision(3);
        builder.Property(x => x.ProcessedAtUtc).HasPrecision(3);
        builder.Property(x => x.LastError).HasMaxLength(4000);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => new { x.ProcessedAtUtc, x.ReceivedAtUtc })
            .HasFilter("[ProcessedAtUtc] IS NULL");
    }
}
