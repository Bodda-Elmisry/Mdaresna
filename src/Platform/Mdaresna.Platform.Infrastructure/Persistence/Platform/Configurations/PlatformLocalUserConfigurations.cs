using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Configurations;

internal sealed class PlatformLocalUserConfiguration : IEntityTypeConfiguration<PlatformLocalUser>
{
    public void Configure(EntityTypeBuilder<PlatformLocalUser> builder)
    {
        builder.ToTable("local_users", "access", table =>
        {
            table.HasCheckConstraint("ck_access_local_users_status",
                "[Status] IN (N'PendingActivation', N'Active', N'Disabled')");
            table.HasCheckConstraint("ck_access_local_users_identifiers",
                "[Id] <> '00000000-0000-0000-0000-000000000000' AND " +
                "[PersonId] <> '00000000-0000-0000-0000-000000000000'");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UserName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.NormalizedUserName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200);
        builder.Property(x => x.Status).HasMaxLength(32).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.UpdatedAtUtc).HasPrecision(3);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => x.PersonId).IsUnique();
        builder.HasIndex(x => x.NormalizedUserName).IsUnique();
    }
}

internal sealed class PlatformLocalCredentialConfiguration : IEntityTypeConfiguration<PlatformLocalCredential>
{
    public void Configure(EntityTypeBuilder<PlatformLocalCredential> builder)
    {
        builder.ToTable("local_credentials", "access", table =>
        {
            table.HasCheckConstraint("ck_access_local_credentials_failed_count",
                "[FailedSignInCount] >= 0");
            table.HasCheckConstraint("ck_access_local_credentials_hashing_version",
                "[HashingVersion] > 0");
        });
        builder.HasKey(x => x.UserId);
        builder.Property(x => x.PasswordHash).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.HashingAlgorithm).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SecurityStamp).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ChangedAtUtc).HasPrecision(3);
        builder.Property(x => x.LockoutEndUtc).HasPrecision(3);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.User).WithOne(x => x.Credential)
            .HasForeignKey<PlatformLocalCredential>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class PlatformLocalPasswordResetChallengeConfiguration :
    IEntityTypeConfiguration<PlatformLocalPasswordResetChallenge>
{
    public void Configure(EntityTypeBuilder<PlatformLocalPasswordResetChallenge> builder)
    {
        builder.ToTable("local_password_reset_challenges", "access", table =>
        {
            table.HasCheckConstraint("ck_access_local_password_reset_attempts",
                "[SendCount] >= 0 AND [FailedAttempts] >= 0");
        });
        builder.HasKey(x => x.UserId);
        builder.Property(x => x.CodeHash).HasMaxLength(32).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.ExpiresAtUtc).HasPrecision(3);
        builder.Property(x => x.ConsumedAtUtc).HasPrecision(3);
        builder.Property(x => x.LastSentAtUtc).HasPrecision(3);
        builder.Property(x => x.SendWindowStartUtc).HasPrecision(3);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.User).WithOne(x => x.PasswordResetChallenge)
            .HasForeignKey<PlatformLocalPasswordResetChallenge>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class PlatformStaffInvitationChallengeConfiguration :
    IEntityTypeConfiguration<PlatformStaffInvitationChallenge>
{
    public void Configure(EntityTypeBuilder<PlatformStaffInvitationChallenge> builder)
    {
        builder.ToTable("staff_invitation_challenges", "access", table =>
        {
            table.HasCheckConstraint("ck_access_staff_invitation_attempts",
                "[SendCount] >= 0 AND [FailedAttempts] >= 0");
        });
        builder.HasKey(x => x.UserId);
        builder.Property(x => x.CodeHash).HasMaxLength(32).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.ExpiresAtUtc).HasPrecision(3);
        builder.Property(x => x.ConsumedAtUtc).HasPrecision(3);
        builder.Property(x => x.LastSentAtUtc).HasPrecision(3);
        builder.Property(x => x.SendWindowStartUtc).HasPrecision(3);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.User).WithOne(x => x.StaffInvitationChallenge)
            .HasForeignKey<PlatformStaffInvitationChallenge>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
