using Mdaresna.Schools.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

internal sealed class PersonConfiguration : IEntityTypeConfiguration<Person>
{
    public void Configure(EntityTypeBuilder<Person> b)
    {
        b.ToTable("persons"); b.HasKey(x => x.Id);
        b.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        b.Property(x => x.FirstName).HasMaxLength(100); b.Property(x => x.MiddleName).HasMaxLength(100);
        b.Property(x => x.LastName).HasMaxLength(100); b.Property(x => x.GenderCode).HasMaxLength(20);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.RowVersion).IsRowVersion();
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

internal sealed class LocalUserConfiguration : IEntityTypeConfiguration<LocalUserAccount>
{
    public void Configure(EntityTypeBuilder<LocalUserAccount> b)
    {
        b.ToTable("local_users"); b.HasKey(x => x.Id);
        b.Property(x => x.UserName).HasMaxLength(100).IsRequired();
        b.Property(x => x.NormalizedUserName).HasMaxLength(100).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32);
        b.Property(x => x.RowVersion).IsRowVersion();
        b.HasIndex(x => x.NormalizedUserName).IsUnique(); b.HasIndex(x => x.PersonId).IsUnique();
        b.HasOne(x => x.Person).WithOne(x => x.UserAccount).HasForeignKey<LocalUserAccount>(x => x.PersonId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class LocalUserCredentialConfiguration : IEntityTypeConfiguration<LocalUserCredential>
{
    public void Configure(EntityTypeBuilder<LocalUserCredential> b)
    {
        b.ToTable("local_user_credentials"); b.HasKey(x => x.UserId);
        b.Property(x => x.PasswordHash).HasMaxLength(1000).IsRequired();
        b.Property(x => x.SecurityStamp).HasMaxLength(100).IsRequired(); b.Property(x => x.RowVersion).IsRowVersion();
        b.HasOne(x => x.User).WithOne(x => x.Credential).HasForeignKey<LocalUserCredential>(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
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
        b.Property(x => x.RowVersion).IsRowVersion();
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
