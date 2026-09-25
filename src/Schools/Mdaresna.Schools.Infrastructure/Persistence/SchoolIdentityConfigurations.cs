using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Domain.School;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

internal sealed class SchoolInformationConfiguration : IEntityTypeConfiguration<SchoolInformation>
{
    public void Configure(EntityTypeBuilder<SchoolInformation> b)
    {
        b.ToTable("school_information"); b.HasKey(x => x.Id);
        b.HasIndex(x => x.PlatformSchoolReferenceId).IsUnique();
        b.HasIndex(x => x.PlatformTenantReferenceId).IsUnique();
        b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Code).HasMaxLength(32).IsRequired();
        b.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        b.Property(x => x.SchoolType).HasMaxLength(32).IsRequired();
        b.Property(x => x.DeploymentMode).HasMaxLength(32).IsRequired();
        b.Property(x => x.Status).HasMaxLength(32).IsRequired();
        b.Property(x => x.Address).HasMaxLength(500); b.Property(x => x.PrimaryPhone).HasMaxLength(32);
        b.Property(x => x.UnitTypeCode).HasMaxLength(32).IsRequired();
        b.Property(x => x.UnitTypeName).HasMaxLength(200).IsRequired();
        b.Property(x => x.UnitPrice).HasPrecision(18, 4);
        b.Property(x => x.Currency).HasMaxLength(3).IsRequired();
    }
}

internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> b)
    {
        b.ToTable("persons"); b.HasKey(x => x.Id);
        b.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        b.Property(x => x.FirstName).HasMaxLength(100); b.Property(x => x.MiddleName).HasMaxLength(100);
        b.Property(x => x.LastName).HasMaxLength(100); b.Property(x => x.GenderCode).HasMaxLength(20);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
    }
}

internal sealed class PersonContactConfiguration : IEntityTypeConfiguration<PersonContact>
{
    public void Configure(EntityTypeBuilder<PersonContact> b)
    {
        b.ToTable("person_contacts"); b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(20);
        b.Property(x => x.Value).HasMaxLength(500).IsRequired();
        b.Property(x => x.NormalizedValue).HasMaxLength(500).IsRequired();
        b.HasIndex(x => new { x.PersonId, x.Type, x.NormalizedValue }).IsUnique();
        b.HasOne(x => x.Person).WithMany(x => x.Contacts).HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class PersonProfileImageConfiguration : IEntityTypeConfiguration<PersonProfileImage>
{
    public void Configure(EntityTypeBuilder<PersonProfileImage> b)
    {
        b.ToTable("person_profile_images"); b.HasKey(x => x.PersonId);
        b.Property(x => x.Content).IsRequired();
        b.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
        b.HasOne(x => x.Person).WithOne(x => x.ProfileImage).HasForeignKey<PersonProfileImage>(x => x.PersonId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class LocalUserConfiguration : IEntityTypeConfiguration<LocalUserAccount>
{
    public void Configure(EntityTypeBuilder<LocalUserAccount> b)
    {
        b.ToTable("local_users"); b.HasKey(x => x.Id);
        b.Property(x => x.UserName).HasMaxLength(100).IsRequired();
        b.Property(x => x.NormalizedUserName).HasMaxLength(100).IsRequired();
        b.Property(x => x.Kind).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        b.HasIndex(x => x.NormalizedUserName).IsUnique(); b.HasIndex(x => x.PersonId).IsUnique();
        b.HasOne(x => x.Person).WithOne(x => x.UserAccount).HasForeignKey<LocalUserAccount>(x => x.PersonId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class StaffAbsenceConfiguration : IEntityTypeConfiguration<StaffAbsence>
{
    public void Configure(EntityTypeBuilder<StaffAbsence> b)
    {
        b.ToTable("staff_absences"); b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.Notes).HasMaxLength(1000);
        b.Property(x => x.SourceType).HasMaxLength(50).IsRequired();
        b.Property(x => x.SourceReferenceId).HasMaxLength(100);
        b.HasOne(x => x.User).WithMany(x => x.Absences).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SchoolUserNotificationConfiguration : IEntityTypeConfiguration<SchoolUserNotification>
{
    public void Configure(EntityTypeBuilder<SchoolUserNotification> b)
    {
        b.ToTable("school_user_notifications"); b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasMaxLength(50).IsRequired();
        b.Property(x => x.TitleAr).HasMaxLength(200).IsRequired(); b.Property(x => x.TitleEn).HasMaxLength(200).IsRequired();
        b.Property(x => x.BodyAr).HasMaxLength(1000).IsRequired(); b.Property(x => x.BodyEn).HasMaxLength(1000).IsRequired();
        b.Property(x => x.RelatedEntityType).HasMaxLength(50); b.Property(x => x.RelatedEntityId).HasMaxLength(100);
        b.HasIndex(x => new { x.RecipientUserId, x.IsRead, x.CreatedAtUtc });
        b.HasOne(x => x.RecipientUser).WithMany(x => x.Notifications).HasForeignKey(x => x.RecipientUserId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class LocalUserCredentialConfiguration : IEntityTypeConfiguration<LocalUserCredential>
{
    public void Configure(EntityTypeBuilder<LocalUserCredential> b)
    {
        b.ToTable("local_user_credentials"); b.HasKey(x => x.UserId);
        b.Property(x => x.PasswordHash).HasMaxLength(1000).IsRequired();
        b.Property(x => x.SecurityStamp).HasMaxLength(100).IsRequired();
        b.HasOne(x => x.User).WithOne(x => x.Credential).HasForeignKey<LocalUserCredential>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class LocalUserActivationChallengeConfiguration : IEntityTypeConfiguration<LocalUserActivationChallenge>
{
    public void Configure(EntityTypeBuilder<LocalUserActivationChallenge> b)
    {
        b.ToTable("local_user_activation_challenges"); b.HasKey(x => x.UserId);
        b.Property(x => x.CodeHash).HasMaxLength(32).IsRequired();
        b.Property(x => x.CodeSalt).HasMaxLength(32).IsRequired();
        b.HasOne(x => x.User).WithOne().HasForeignKey<LocalUserActivationChallenge>(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class LocalUserSessionConfiguration : IEntityTypeConfiguration<LocalUserSession>
{
    public void Configure(EntityTypeBuilder<LocalUserSession> b)
    {
        b.ToTable("local_user_sessions"); b.HasKey(x => x.Id);
        b.Property(x => x.RefreshTokenHash).HasMaxLength(256).IsRequired(); b.HasIndex(x => x.RefreshTokenHash).IsUnique();
        b.Property(x => x.RevocationReason).HasMaxLength(200);
        b.HasOne(x => x.User).WithMany(x => x.Sessions).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class LocalRoleConfiguration : IEntityTypeConfiguration<LocalRole>
{
    public void Configure(EntityTypeBuilder<LocalRole> b)
    {
        b.ToTable("local_roles"); b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(100).IsRequired(); b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.DisplayNameAr).HasMaxLength(150).IsRequired(); b.Property(x => x.DisplayNameEn).HasMaxLength(150).IsRequired();
        b.HasData(new { Id = SchoolIdentitySeed.SchoolAdminRoleId, Code = SchoolIdentitySeed.SchoolAdminRoleCode,
            DisplayNameAr = "مدير المدرسة", DisplayNameEn = "School Admin", IsSystem = true, IsActive = true,
            CreatedAtUtc = SchoolIdentitySeed.SeededAtUtc, UpdatedAtUtc = SchoolIdentitySeed.SeededAtUtc, RowVersion = Array.Empty<byte>() });
    }
}

internal sealed class LocalPermissionConfiguration : IEntityTypeConfiguration<LocalPermission>
{
    public void Configure(EntityTypeBuilder<LocalPermission> b)
    {
        b.ToTable("local_permissions"); b.HasKey(x => x.Id);
        b.Property(x => x.Code).HasMaxLength(150).IsRequired(); b.HasIndex(x => x.Code).IsUnique();
        b.Property(x => x.Module).HasMaxLength(100).IsRequired();
        b.Property(x => x.DisplayNameAr).HasMaxLength(150).IsRequired(); b.Property(x => x.DisplayNameEn).HasMaxLength(150).IsRequired();
        b.HasData(SchoolIdentitySeed.Permissions.Select(x => new { x.Id, x.Code, x.Module, DisplayNameAr = x.Ar,
            DisplayNameEn = x.En, IsActive = true, CreatedAtUtc = SchoolIdentitySeed.SeededAtUtc, UpdatedAtUtc = SchoolIdentitySeed.SeededAtUtc }));
    }
}

internal sealed class LocalRolePermissionConfiguration : IEntityTypeConfiguration<LocalRolePermission>
{
    public void Configure(EntityTypeBuilder<LocalRolePermission> b)
    {
        b.ToTable("local_role_permissions"); b.HasKey(x => new { x.RoleId, x.PermissionId });
        b.HasOne(x => x.Role).WithMany(x => x.Permissions).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Permission).WithMany(x => x.Roles).HasForeignKey(x => x.PermissionId).OnDelete(DeleteBehavior.Cascade);
        b.HasData(SchoolIdentitySeed.Permissions.Select(x => new { RoleId = SchoolIdentitySeed.SchoolAdminRoleId,
            PermissionId = x.Id, GrantedAtUtc = SchoolIdentitySeed.SeededAtUtc, GrantedByUserId = (Guid?)null }));
    }
}

internal sealed class LocalUserRoleConfiguration : IEntityTypeConfiguration<LocalUserRole>
{
    public void Configure(EntityTypeBuilder<LocalUserRole> b)
    {
        b.ToTable("local_user_roles"); b.HasKey(x => new { x.UserId, x.RoleId });
        b.HasOne(x => x.User).WithMany(x => x.Roles).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Role).WithMany(x => x.Users).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
    }
}
