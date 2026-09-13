using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Configurations;

internal sealed class PlatformSmsProviderConfiguration : IEntityTypeConfiguration<PlatformSmsProvider>
{
    public void Configure(EntityTypeBuilder<PlatformSmsProvider> builder)
    {
        builder.ToTable("sms_providers", "platform", table =>
        {
            table.HasCheckConstraint(
                "ck_platform_sms_providers_api_url_https",
                "[ApiUrlTemplate] LIKE 'https://%'");
            table.HasCheckConstraint(
                "ck_platform_sms_providers_message_length",
                "[MessageCharactersLength] BETWEEN 1 AND 1000");
            table.HasCheckConstraint(
                "ck_platform_sms_providers_priority",
                "[Priority] >= 1");
            table.HasCheckConstraint(
                "ck_platform_sms_providers_deleted_inactive",
                "[IsDeleted] = 0 OR [IsActive] = 0");
            table.HasCheckConstraint(
                "ck_platform_sms_providers_timestamps",
                "[CreatedAtUtc] <= [UpdatedAtUtc] " +
                "AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 " +
                "AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
        });

        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.ProviderUserName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.EncryptedPassword).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.SenderName).HasMaxLength(300).IsRequired();
        builder.Property(x => x.ApiUrlTemplate).HasMaxLength(2000).IsRequired();
        builder.Property(x => x.MessageCharactersLength).IsRequired();
        builder.Property(x => x.Priority).IsRequired();
        builder.Property(x => x.SuccessResponsePrefix).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.UpdatedAtUtc).HasPrecision(3);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasIndex(x => new { x.IsDeleted, x.IsActive, x.Priority });
    }
}
