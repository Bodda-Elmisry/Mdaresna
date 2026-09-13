using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Configurations;

internal sealed class PlatformSmsLogConfiguration : IEntityTypeConfiguration<PlatformSmsLog>
{
    public void Configure(EntityTypeBuilder<PlatformSmsLog> builder)
    {
        builder.ToTable("sms_logs", "platform", table =>
        {
            table.HasCheckConstraint(
                "ck_platform_sms_logs_status",
                "[Status] IN ('Pending', 'Accepted', 'Rejected', 'Failed')");
            table.HasCheckConstraint(
                "ck_platform_sms_logs_source_system",
                "[SourceSystem] IN ('platform', 'schools', 'family')");
            table.HasCheckConstraint(
                "ck_platform_sms_logs_school_scope",
                "[SourceSystem] <> 'schools' OR [SchoolId] IS NOT NULL");
            table.HasCheckConstraint(
                "ck_platform_sms_logs_completion",
                "([Status] = 'Pending' AND [CompletedAtUtc] IS NULL) " +
                "OR ([Status] <> 'Pending' AND [CompletedAtUtc] IS NOT NULL)");
            table.HasCheckConstraint(
                "ck_platform_sms_logs_http_status",
                "[HttpStatusCode] IS NULL OR [HttpStatusCode] BETWEEN 100 AND 599");
            table.HasCheckConstraint(
                "ck_platform_sms_logs_timestamps",
                "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 " +
                "AND ([CompletedAtUtc] IS NULL OR " +
                "([CompletedAtUtc] >= [CreatedAtUtc] " +
                "AND DATEPART(TZOFFSET, [CompletedAtUtc]) = 0))");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.SourceSystem).HasMaxLength(16).IsRequired();
        builder.Property(x => x.MessageTypeCode).HasMaxLength(64).IsRequired();
        builder.Property(x => x.RecipientEncrypted).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.RecipientMasked).HasMaxLength(32).IsRequired();
        builder.Property(x => x.MessageEncrypted).IsRequired();
        builder.Property(x => x.ResponseEncrypted);
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.FailureReason).HasMaxLength(256);
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.CompletedAtUtc).HasPrecision(3);

        builder.HasOne(x => x.Provider)
            .WithMany()
            .HasForeignKey(x => x.ProviderId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.CreatedAtUtc);
        builder.HasIndex(x => new { x.ProviderId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.SourceSystem, x.SchoolId, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.SourceSystem, x.SourceMessageId })
            .IsUnique()
            .HasFilter("[SourceMessageId] IS NOT NULL");
    }
}
