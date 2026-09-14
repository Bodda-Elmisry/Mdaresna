using Mdaresna.Platform.Infrastructure.Persistence.Identity.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Platform.Infrastructure.Persistence.Identity.Configurations;

internal sealed class AccountConfiguration : IEntityTypeConfiguration<Account>
{
    public void Configure(EntityTypeBuilder<Account> builder)
    {
        builder.ToTable("accounts", "identity", table =>
        {
            table.HasCheckConstraint(
                "ck_identity_accounts_status",
                "[Status] IN (N'PendingVerification', N'Active', N'Locked', N'Disabled')");
            table.HasCheckConstraint(
                "ck_identity_accounts_timestamps",
                "[CreatedAtUtc] <= [UpdatedAtUtc] " +
                "AND DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 " +
                "AND DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200);
        builder.Property(x => x.GenderCode).HasMaxLength(16);
        builder.Property(x => x.DateOfBirth).HasColumnType("date");
        builder.Property(x => x.PreferredLocale).HasMaxLength(20);
        builder.Property(x => x.TimeZone).HasMaxLength(100);
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.UpdatedAtUtc).HasPrecision(3);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasIndex(x => x.Status);
    }
}

internal sealed class AccountAppLanguagePreferenceConfiguration :
    IEntityTypeConfiguration<AccountAppLanguagePreference>
{
    public void Configure(EntityTypeBuilder<AccountAppLanguagePreference> builder)
    {
        builder.ToTable("account_app_language_preferences", "identity", table =>
        {
            table.HasCheckConstraint("ck_identity_app_language_app_code",
                "[AppCode] IN (N'platform', N'schools', N'family')");
            table.HasCheckConstraint("ck_identity_app_language_language_code",
                "[LanguageCode] IN (N'ar', N'en')");
            table.HasCheckConstraint("ck_identity_app_language_updated_at_utc",
                "DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
        });
        builder.HasKey(x => new { x.AccountId, x.AppCode });
        builder.Property(x => x.AppCode).HasMaxLength(16).IsRequired();
        builder.Property(x => x.LanguageCode).HasMaxLength(2).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasPrecision(3);
        builder.HasOne(x => x.Account)
            .WithMany(x => x.AppLanguagePreferences)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class LoginIdentifierConfiguration : IEntityTypeConfiguration<LoginIdentifier>
{
    public void Configure(EntityTypeBuilder<LoginIdentifier> builder)
    {
        builder.ToTable("login_identifiers", "identity", table =>
        {
            table.HasCheckConstraint(
                "ck_identity_login_identifiers_scope",
                "([Type] IN (N'Email', N'Phone') AND [SchoolId] IS NULL) OR " +
                "([Type] IN (N'SchoolUsername', N'StudentCode') AND [SchoolId] IS NOT NULL)");
            table.HasCheckConstraint(
                "ck_identity_login_identifiers_verification",
                "([IsVerified] = 1 AND [VerifiedAtUtc] IS NOT NULL) OR " +
                "([IsVerified] = 0 AND [VerifiedAtUtc] IS NULL)");
            table.HasCheckConstraint(
                "ck_identity_login_identifiers_primary_scope",
                "[IsPrimary] = 0 OR ([Type] IN (N'Email', N'Phone') AND [SchoolId] IS NULL)");
            table.HasCheckConstraint(
                "ck_identity_login_identifiers_timestamps",
                "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND " +
                "([VerifiedAtUtc] IS NULL OR ([VerifiedAtUtc] >= [CreatedAtUtc] " +
                "AND DATEPART(TZOFFSET, [VerifiedAtUtc]) = 0))");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.NormalizedValue).HasMaxLength(320).IsRequired();
        builder.Property(x => x.DisplayValue).HasMaxLength(320).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.VerifiedAtUtc).HasPrecision(3);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.Account)
            .WithMany(x => x.LoginIdentifiers)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.Type, x.NormalizedValue })
            .IsUnique()
            .HasFilter("[SchoolId] IS NULL");
        builder.HasIndex(x => new { x.SchoolId, x.Type, x.NormalizedValue })
            .IsUnique()
            .HasFilter("[SchoolId] IS NOT NULL");
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => new { x.AccountId, x.Type })
            .IsUnique()
            .HasFilter("[IsPrimary] = 1");
    }
}

internal sealed class AccountContactConfiguration : IEntityTypeConfiguration<AccountContact>
{
    public void Configure(EntityTypeBuilder<AccountContact> builder)
    {
        builder.ToTable("account_contacts", "identity", table =>
        {
            table.HasCheckConstraint("ck_identity_account_contacts_type",
                "[Type] IN (N'Phone', N'Email', N'Address')");
            table.HasCheckConstraint("ck_identity_account_contacts_value",
                "[Value] <> N'' AND [NormalizedValue] <> N''");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(16).IsRequired();
        builder.Property(x => x.Value).HasMaxLength(500).IsRequired();
        builder.Property(x => x.NormalizedValue).HasMaxLength(500).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.UpdatedAtUtc).HasPrecision(3);
        builder.HasOne(x => x.Account)
            .WithMany(x => x.Contacts)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.AccountId);
        builder.HasIndex(x => new { x.AccountId, x.Type, x.NormalizedValue })
            .IsUnique()
            .HasFilter("[Type] IN (N'Phone', N'Email')");
    }
}

internal sealed class AccountProfileImageConfiguration : IEntityTypeConfiguration<AccountProfileImage>
{
    public void Configure(EntityTypeBuilder<AccountProfileImage> builder)
    {
        builder.ToTable("account_profile_images", "identity");
        builder.HasKey(x => x.AccountId);
        builder.Property(x => x.Content).IsRequired();
        builder.Property(x => x.ContentType).HasMaxLength(32).IsRequired();
        builder.Property(x => x.UpdatedAtUtc).HasPrecision(3);
        builder.HasOne(x => x.Account)
            .WithOne(x => x.ProfileImage)
            .HasForeignKey<AccountProfileImage>(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class PasswordCredentialConfiguration : IEntityTypeConfiguration<PasswordCredential>
{
    public void Configure(EntityTypeBuilder<PasswordCredential> builder)
    {
        builder.ToTable("password_credentials", "identity", table =>
        {
            table.HasCheckConstraint(
                "ck_identity_password_credentials_failed_count",
                "[FailedSignInCount] >= 0");
            table.HasCheckConstraint(
                "ck_identity_password_credentials_hashing_version",
                "[HashingVersion] > 0");
            table.HasCheckConstraint(
                "ck_identity_password_credentials_timestamps",
                "DATEPART(TZOFFSET, [ChangedAtUtc]) = 0 AND " +
                "([LockoutEndUtc] IS NULL OR DATEPART(TZOFFSET, [LockoutEndUtc]) = 0)");
        });
        builder.HasKey(x => x.AccountId);
        builder.Property(x => x.PasswordHash).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.HashingAlgorithm).HasMaxLength(50).IsRequired();
        builder.Property(x => x.SecurityStamp).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ChangedAtUtc).HasPrecision(3);
        builder.Property(x => x.LockoutEndUtc).HasPrecision(3);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.Account)
            .WithOne(x => x.PasswordCredential)
            .HasForeignKey<PasswordCredential>(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class IdentitySessionConfiguration : IEntityTypeConfiguration<IdentitySession>
{
    public void Configure(EntityTypeBuilder<IdentitySession> builder)
    {
        builder.ToTable("sessions", "identity", table =>
        {
            table.HasCheckConstraint(
                "ck_identity_sessions_expiry",
                "[ExpiresAtUtc] > [CreatedAtUtc]");
            table.HasCheckConstraint(
                "ck_identity_sessions_replacement_not_self",
                "[ReplacedBySessionId] IS NULL OR [ReplacedBySessionId] <> [Id]");
            table.HasCheckConstraint(
                "ck_identity_sessions_replacement_requires_revocation",
                "[ReplacedBySessionId] IS NULL OR [RevokedAtUtc] IS NOT NULL");
            table.HasCheckConstraint(
                "ck_identity_sessions_timestamps",
                "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 " +
                "AND DATEPART(TZOFFSET, [ExpiresAtUtc]) = 0 " +
                "AND ([LastSeenAtUtc] IS NULL OR ([LastSeenAtUtc] >= [CreatedAtUtc] " +
                "AND DATEPART(TZOFFSET, [LastSeenAtUtc]) = 0)) " +
                "AND ([RevokedAtUtc] IS NULL OR ([RevokedAtUtc] >= [CreatedAtUtc] " +
                "AND DATEPART(TZOFFSET, [RevokedAtUtc]) = 0))");
        });
        builder.HasKey(x => x.Id);
        builder.HasAlternateKey(x => new { x.AccountId, x.Id });
        builder.Property(x => x.RefreshTokenHash).HasMaxLength(256).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.ExpiresAtUtc).HasPrecision(3);
        builder.Property(x => x.LastSeenAtUtc).HasPrecision(3);
        builder.Property(x => x.RevokedAtUtc).HasPrecision(3);
        builder.Property(x => x.RevocationReason).HasMaxLength(500);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(1000);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.Account)
            .WithMany(x => x.Sessions)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<IdentitySession>()
            .WithMany()
            .HasForeignKey(x => new { x.AccountId, x.ReplacedBySessionId })
            .HasPrincipalKey(x => new { x.AccountId, x.Id })
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasIndex(x => x.RefreshTokenHash).IsUnique();
        builder.HasIndex(x => new { x.AccountId, x.ExpiresAtUtc });
        builder.HasIndex(x => x.ReplacedBySessionId)
            .IsUnique()
            .HasFilter("[ReplacedBySessionId] IS NOT NULL");
    }
}

internal sealed class MfaMethodConfiguration : IEntityTypeConfiguration<MfaMethod>
{
    public void Configure(EntityTypeBuilder<MfaMethod> builder)
    {
        builder.ToTable("mfa_methods", "identity", table =>
        {
            table.HasCheckConstraint(
                "ck_identity_mfa_methods_type",
                "[Type] IN (N'Authenticator', N'Email', N'Sms')");
            table.HasCheckConstraint(
                "ck_identity_mfa_methods_primary_enabled",
                "[IsPrimary] = 0 OR [IsEnabled] = 1");
            table.HasCheckConstraint(
                "ck_identity_mfa_methods_authenticator_secret",
                "[Type] <> N'Authenticator' OR ([SecretReference] IS NOT NULL " +
                "AND LEN(LTRIM(RTRIM([SecretReference]))) > 0)");
            table.HasCheckConstraint(
                "ck_identity_mfa_methods_timestamps",
                "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND " +
                "([LastUsedAtUtc] IS NULL OR ([LastUsedAtUtc] >= [CreatedAtUtc] " +
                "AND DATEPART(TZOFFSET, [LastUsedAtUtc]) = 0))");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.SecretReference).HasMaxLength(500);
        builder.Property(x => x.DestinationHint).HasMaxLength(200);
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.LastUsedAtUtc).HasPrecision(3);
        builder.Property(x => x.RowVersion).IsRowVersion();

        builder.HasOne(x => x.Account)
            .WithMany(x => x.MfaMethods)
            .HasForeignKey(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.AccountId, x.Type });
        builder.HasIndex(x => x.AccountId)
            .IsUnique()
            .HasFilter("[IsPrimary] = 1 AND [IsEnabled] = 1");
    }
}

internal sealed class IdentitySecurityEventConfiguration : IEntityTypeConfiguration<IdentitySecurityEvent>
{
    public void Configure(EntityTypeBuilder<IdentitySecurityEvent> builder)
    {
        builder.ToTable("security_events", "identity", table =>
            table.HasCheckConstraint(
                "ck_identity_security_events_occurred_utc",
                "DATEPART(TZOFFSET, [OccurredAtUtc]) = 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EventType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.OccurredAtUtc).HasPrecision(3);
        builder.Property(x => x.IpAddress).HasMaxLength(64);
        builder.Property(x => x.UserAgent).HasMaxLength(1000);
        builder.Property(x => x.MetadataJson).HasColumnType("nvarchar(max)");
        builder.HasIndex(x => new { x.AccountId, x.OccurredAtUtc });
        builder.HasIndex(x => new { x.EventType, x.OccurredAtUtc });
    }
}

internal sealed class IdentityOutboxMessageConfiguration : IEntityTypeConfiguration<IdentityOutboxMessage>
{
    public void Configure(EntityTypeBuilder<IdentityOutboxMessage> builder)
    {
        builder.ToTable("outbox_messages", "messaging", table =>
        {
            table.HasCheckConstraint(
                "ck_messaging_identity_outbox_occurred_utc",
                "DATEPART(TZOFFSET, [OccurredAtUtc]) = 0");
            table.HasCheckConstraint(
                "ck_messaging_identity_outbox_nullable_utc",
                "([ProcessedAtUtc] IS NULL OR DATEPART(TZOFFSET, [ProcessedAtUtc]) = 0) " +
                "AND ([NextAttemptAtUtc] IS NULL OR DATEPART(TZOFFSET, [NextAttemptAtUtc]) = 0)");
        });
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

internal sealed class AccountActivationChallengeConfiguration : IEntityTypeConfiguration<AccountActivationChallenge>
{
    public void Configure(EntityTypeBuilder<AccountActivationChallenge> builder)
    {
        builder.ToTable("account_activation_challenges", "identity", table =>
        {
            table.HasCheckConstraint("ck_identity_activation_challenge_expiry",
                "[ExpiresAtUtc] > [CreatedAtUtc]");
            table.HasCheckConstraint("ck_identity_activation_challenge_counts",
                "[SendCount] >= 0 AND [FailedAttempts] >= 0");
        });
        builder.HasKey(x => x.AccountId);
        builder.Property(x => x.CodeHash).HasMaxLength(32).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.ExpiresAtUtc).HasPrecision(3);
        builder.Property(x => x.ConsumedAtUtc).HasPrecision(3);
        builder.Property(x => x.LastSentAtUtc).HasPrecision(3);
        builder.Property(x => x.SendWindowStartUtc).HasPrecision(3);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.Account)
            .WithOne(x => x.ActivationChallenge)
            .HasForeignKey<AccountActivationChallenge>(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class AccountPasswordResetChallengeConfiguration : IEntityTypeConfiguration<AccountPasswordResetChallenge>
{
    public void Configure(EntityTypeBuilder<AccountPasswordResetChallenge> builder)
    {
        builder.ToTable("account_password_reset_challenges", "identity", table =>
        {
            table.HasCheckConstraint("ck_identity_password_reset_challenge_expiry",
                "[ExpiresAtUtc] > [CreatedAtUtc]");
            table.HasCheckConstraint("ck_identity_password_reset_challenge_counts",
                "[SendCount] >= 0 AND [FailedAttempts] >= 0");
        });
        builder.HasKey(x => x.AccountId);
        builder.Property(x => x.CodeHash).HasMaxLength(32).IsRequired();
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.ExpiresAtUtc).HasPrecision(3);
        builder.Property(x => x.ConsumedAtUtc).HasPrecision(3);
        builder.Property(x => x.LastSentAtUtc).HasPrecision(3);
        builder.Property(x => x.SendWindowStartUtc).HasPrecision(3);
        builder.Property(x => x.RowVersion).IsRowVersion();
        builder.HasOne(x => x.Account)
            .WithOne(x => x.PasswordResetChallenge)
            .HasForeignKey<AccountPasswordResetChallenge>(x => x.AccountId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
