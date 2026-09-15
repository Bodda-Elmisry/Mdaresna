using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Configurations;

internal sealed class PlatformUserDeviceConfiguration : IEntityTypeConfiguration<PlatformUserDevice>
{
    public void Configure(EntityTypeBuilder<PlatformUserDevice> builder)
    {
        builder.ToTable("user_devices", "messaging", table =>
        {
            table.HasCheckConstraint("ck_messaging_user_devices_platform",
                "[Platform] IN ('android','ios','web')");
            table.HasCheckConstraint("ck_messaging_user_devices_language",
                "[LanguageCode] IN ('ar','en')");
            table.HasCheckConstraint("ck_messaging_user_devices_timestamps",
                "[CreatedAtUtc] <= [UpdatedAtUtc] AND [UpdatedAtUtc] <= [LastSeenAtUtc] " +
                "AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 " +
                "AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0 " +
                "AND DATEPART(TZOFFSET, [LastSeenAtUtc]) = 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.AccountId).HasConversion(id => id.Value, value => IdentityAccountId.From(value));
        builder.Property(x => x.InstallationId).HasMaxLength(128).IsRequired();
        builder.Property(x => x.FcmToken).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Platform).HasMaxLength(16).IsRequired();
        builder.Property(x => x.LanguageCode).HasMaxLength(5).IsRequired();
        builder.Property(x => x.DeviceName).HasMaxLength(200);
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.UpdatedAtUtc).HasPrecision(3);
        builder.Property(x => x.LastSeenAtUtc).HasPrecision(3);
        builder.HasIndex(x => new { x.AccountId, x.InstallationId }).IsUnique();
        builder.HasIndex(x => x.FcmToken).IsUnique();
        builder.HasIndex(x => new { x.AccountId, x.LastSeenAtUtc });
    }
}

internal sealed class PlatformNotificationConfiguration : IEntityTypeConfiguration<PlatformNotification>
{
    public void Configure(EntityTypeBuilder<PlatformNotification> builder)
    {
        builder.ToTable("notifications", "messaging", table =>
            table.HasCheckConstraint("ck_messaging_notifications_timestamps",
                "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 " +
                "AND ([ExpiresAtUtc] IS NULL OR ([ExpiresAtUtc] > [CreatedAtUtc] " +
                "AND DATEPART(TZOFFSET, [ExpiresAtUtc]) = 0))"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasMaxLength(150).IsRequired();
        builder.Property(x => x.TitleAr).HasMaxLength(200).IsRequired();
        builder.Property(x => x.TitleEn).HasMaxLength(200).IsRequired();
        builder.Property(x => x.BodyAr).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.BodyEn).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.ActionUrl).HasMaxLength(500);
        builder.Property(x => x.DataJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.ExpiresAtUtc).HasPrecision(3);
        builder.HasIndex(x => x.CreatedAtUtc);
    }
}

internal sealed class PlatformNotificationRecipientConfiguration :
    IEntityTypeConfiguration<PlatformNotificationRecipient>
{
    public void Configure(EntityTypeBuilder<PlatformNotificationRecipient> builder)
    {
        builder.ToTable("notification_recipients", "messaging", table =>
            table.HasCheckConstraint("ck_messaging_notification_recipients_timestamps",
                "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 " +
                "AND ([ReadAtUtc] IS NULL OR ([ReadAtUtc] >= [CreatedAtUtc] " +
                "AND DATEPART(TZOFFSET, [ReadAtUtc]) = 0))"));
        builder.HasKey(x => new { x.NotificationId, x.AccountId });
        builder.Property(x => x.AccountId).HasConversion(id => id.Value, value => IdentityAccountId.From(value));
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.ReadAtUtc).HasPrecision(3);
        builder.HasOne<PlatformNotification>().WithMany().HasForeignKey(x => x.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.AccountId, x.ReadAtUtc, x.CreatedAtUtc });
    }
}

internal sealed class PlatformNotificationDeliveryConfiguration :
    IEntityTypeConfiguration<PlatformNotificationDelivery>
{
    public void Configure(EntityTypeBuilder<PlatformNotificationDelivery> builder)
    {
        builder.ToTable("notification_deliveries", "messaging", table =>
        {
            table.HasCheckConstraint("ck_messaging_notification_deliveries_status",
                "[Status] IN ('Pending','Sent','Failed','Skipped')");
            table.HasCheckConstraint("ck_messaging_notification_deliveries_attempts", "[AttemptCount] >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.FcmTokenSnapshot).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.Status).HasMaxLength(16).IsRequired();
        builder.Property(x => x.LastError).HasMaxLength(2000);
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.NextAttemptAtUtc).HasPrecision(3);
        builder.Property(x => x.SentAtUtc).HasPrecision(3);
        builder.HasOne<PlatformNotification>().WithMany().HasForeignKey(x => x.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => new { x.Status, x.NextAttemptAtUtc, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.NotificationId, x.DeviceId }).IsUnique();
    }
}
