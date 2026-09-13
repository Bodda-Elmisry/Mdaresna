using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Configurations;

internal sealed class PlatformRoleConfiguration : IEntityTypeConfiguration<PlatformRole>
{
    public void Configure(EntityTypeBuilder<PlatformRole> builder)
    {
        builder.ToTable("roles", "access", table =>
        {
            table.HasCheckConstraint(
                "ck_access_roles_timestamps",
                "[CreatedAtUtc] <= [UpdatedAtUtc] AND " +
                "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND " +
                "DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
            table.HasCheckConstraint("ck_access_roles_version", "[Version] >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => PlatformRoleId.From(value))
            .ValueGeneratedNever();
        builder.Property(x => x.Key).HasMaxLength(64).IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(x => x.IsSystem);
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.UpdatedAtUtc).HasPrecision(3);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Property<byte[]>("RowVersion").IsRequired().IsRowVersion();
        builder.Ignore(x => x.Permissions);
        builder.Ignore(x => x.DomainEvents);
        builder.HasIndex(x => x.Key).IsUnique();
        builder.HasIndex(x => x.IsActive);
    }
}

internal sealed class PlatformPermissionRecordConfiguration :
    IEntityTypeConfiguration<PlatformPermissionRecord>
{
    public void Configure(EntityTypeBuilder<PlatformPermissionRecord> builder)
    {
        builder.ToTable("permissions", "access");
        builder.HasKey(x => x.Code);
        builder.Property(x => x.Code)
            .HasConversion(code => code.Value, value => PermissionCode.Create(value))
            .HasMaxLength(100)
            .ValueGeneratedNever();
        builder.Property(x => x.Description).HasMaxLength(500);

        builder.HasData(PlatformPermissionCodes.All
            .Select(code => new PlatformPermissionRecord { Code = code })
            .ToArray());
    }
}

internal sealed class PlatformRolePermissionRecordConfiguration :
    IEntityTypeConfiguration<PlatformRolePermissionRecord>
{
    public void Configure(EntityTypeBuilder<PlatformRolePermissionRecord> builder)
    {
        builder.ToTable("role_permissions", "access");
        builder.HasKey(x => new { x.RoleId, x.PermissionCode });
        builder.Property(x => x.RoleId)
            .HasConversion(id => id.Value, value => PlatformRoleId.From(value));
        builder.Property(x => x.PermissionCode)
            .HasConversion(code => code.Value, value => PermissionCode.Create(value))
            .HasMaxLength(100);

        builder.HasOne<PlatformRole>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne<PlatformPermissionRecord>()
            .WithMany()
            .HasForeignKey(x => x.PermissionCode)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class PlatformRoleAssignmentConfiguration :
    IEntityTypeConfiguration<PlatformRoleAssignment>
{
    public void Configure(EntityTypeBuilder<PlatformRoleAssignment> builder)
    {
        builder.ToTable("role_assignments", "access", table =>
        {
            table.HasCheckConstraint(
                "ck_access_role_assignments_revocation",
                "([RevokedAtUtc] IS NULL AND [RevokedByAccountId] IS NULL) OR " +
                "([RevokedAtUtc] IS NOT NULL AND [RevokedByAccountId] IS NOT NULL " +
                "AND [RevokedAtUtc] >= [AssignedAtUtc])");
            table.HasCheckConstraint(
                "ck_access_role_assignments_timestamps",
                "DATEPART(TZOFFSET, [AssignedAtUtc]) = 0 AND " +
                "([RevokedAtUtc] IS NULL OR DATEPART(TZOFFSET, [RevokedAtUtc]) = 0)");
            table.HasCheckConstraint("ck_access_role_assignments_version", "[Version] >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => PlatformRoleAssignmentId.From(value))
            .ValueGeneratedNever();
        builder.Property(x => x.AccountId)
            .HasConversion(id => id.Value, value => IdentityAccountId.From(value));
        builder.Property(x => x.RoleId)
            .HasConversion(id => id.Value, value => PlatformRoleId.From(value));
        builder.Property(x => x.AssignedByAccountId)
            .HasConversion(id => id.Value, value => IdentityAccountId.From(value));
        builder.Property(x => x.RevokedByAccountId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? IdentityAccountId.From(value.Value) : (IdentityAccountId?)null);
        builder.Property(x => x.AssignedAtUtc).HasPrecision(3);
        builder.Property(x => x.RevokedAtUtc).HasPrecision(3);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Property<byte[]>("RowVersion").IsRequired().IsRowVersion();
        builder.Ignore(x => x.IsActive);
        builder.Ignore(x => x.DomainEvents);

        builder.HasOne<PlatformRole>()
            .WithMany()
            .HasForeignKey(x => x.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.AccountId, x.RoleId })
            .IsUnique()
            .HasFilter("[RevokedAtUtc] IS NULL");
        builder.HasIndex(x => new { x.AccountId, x.RevokedAtUtc });
    }
}
