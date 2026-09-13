using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Configurations;

internal sealed class TenantConfiguration : IEntityTypeConfiguration<Tenant>
{
    public void Configure(EntityTypeBuilder<Tenant> builder)
    {
        builder.ToTable("tenants", "registry", table =>
        {
            table.HasCheckConstraint(
                "ck_registry_tenants_status",
                "[Status] IN (N'Draft', N'Active', N'Suspended', N'Closed')");
            table.HasCheckConstraint(
                "ck_registry_tenants_status_reason",
                "([Status] IN (N'Suspended', N'Closed') AND [SuspensionReason] IS NOT NULL " +
                "AND LEN(LTRIM(RTRIM([SuspensionReason]))) > 0) OR " +
                "([Status] IN (N'Draft', N'Active') AND [SuspensionReason] IS NULL)");
            table.HasCheckConstraint(
                "ck_registry_tenants_timestamps",
                "[CreatedAtUtc] <= [UpdatedAtUtc] AND " +
                "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND " +
                "DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
            table.HasCheckConstraint("ck_registry_tenants_version", "[Version] >= 0");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => TenantId.From(value))
            .ValueGeneratedNever();
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.LegalName).HasMaxLength(250);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.SuspensionReason).HasMaxLength(500);
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.UpdatedAtUtc).HasPrecision(3);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Property<byte[]>("RowVersion").IsRequired().IsRowVersion();
        builder.Ignore(x => x.DomainEvents);
        builder.Ignore(x => x.CanRegisterSchools);
        builder.HasIndex(x => x.Status);
    }
}

internal sealed class SchoolRegistrationConfiguration : IEntityTypeConfiguration<SchoolRegistration>
{
    public void Configure(EntityTypeBuilder<SchoolRegistration> builder)
    {
        builder.ToTable("schools", "registry", table =>
        {
            table.HasCheckConstraint(
                "ck_registry_schools_type",
                "[SchoolType] IN (N'Private', N'Government')");
            table.HasCheckConstraint(
                "ck_registry_schools_deployment_mode",
                "[DeploymentMode] IN (N'SharedSaaS', N'DedicatedCloud', N'GovernmentOnPremises')");
            table.HasCheckConstraint(
                "ck_registry_schools_deployment_matches_type",
                "[DeploymentMode] <> N'GovernmentOnPremises' OR [SchoolType] = N'Government'");
            table.HasCheckConstraint(
                "ck_registry_schools_status",
                "[Status] IN (N'Draft', N'PendingVerification', N'Approved', N'Provisioning', " +
                "N'ProvisioningFailed', N'Active', N'Suspended', N'Closed')");
            table.HasCheckConstraint(
                "ck_registry_schools_provisioning_operation",
                "([Status] IN (N'Draft', N'PendingVerification', N'Approved') " +
                "AND [ProvisioningOperationId] IS NULL) OR " +
                "([Status] IN (N'Provisioning', N'ProvisioningFailed', N'Active', N'Suspended') " +
                "AND [ProvisioningOperationId] IS NOT NULL " +
                "AND [ProvisioningOperationId] <> '00000000-0000-0000-0000-000000000000') OR " +
                "([Status] = N'Closed' AND ([ProvisioningOperationId] IS NULL OR " +
                "[ProvisioningOperationId] <> '00000000-0000-0000-0000-000000000000'))");
            table.HasCheckConstraint(
                "ck_registry_schools_status_reason",
                "([Status] IN (N'ProvisioningFailed', N'Suspended', N'Closed') " +
                "AND [StatusReason] IS NOT NULL AND LEN(LTRIM(RTRIM([StatusReason]))) > 0) OR " +
                "([Status] IN (N'Draft', N'PendingVerification', N'Approved', N'Provisioning', N'Active') " +
                "AND [StatusReason] IS NULL)");
            table.HasCheckConstraint(
                "ck_registry_schools_timestamps",
                "[CreatedAtUtc] <= [UpdatedAtUtc] AND " +
                "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0 AND " +
                "DATEPART(TZOFFSET, [UpdatedAtUtc]) = 0");
            table.HasCheckConstraint("ck_registry_schools_version", "[Version] >= 0");
            table.HasCheckConstraint(
                "ck_registry_schools_school_is_tenant",
                "[Id] = [TenantId]");
        });
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .HasConversion(id => id.Value, value => SchoolId.From(value))
            .ValueGeneratedNever();
        builder.Property(x => x.TenantId)
            .HasConversion(id => id.Value, value => TenantId.From(value));
        builder.Property(x => x.RegistrationRequestId).ValueGeneratedNever();
        builder.Property(x => x.Code)
            .HasConversion(code => code.Value, value => SchoolCode.Create(value))
            .HasMaxLength(32)
            .IsRequired();
        builder.Property(x => x.DisplayName).HasMaxLength(200).IsRequired();
        builder.Property(x => x.SchoolType).HasConversion<string>().HasMaxLength(32).IsRequired();
        builder.Property(x => x.DeploymentMode).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(40).IsRequired();
        builder.Property(x => x.RequestedByAccountId).ValueGeneratedNever();
        builder.Property(x => x.ProvisioningOperationId);
        builder.Property(x => x.StatusReason).HasMaxLength(1000);
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);
        builder.Property(x => x.UpdatedAtUtc).HasPrecision(3);
        builder.Property(x => x.Version).IsConcurrencyToken();
        builder.Property<byte[]>("RowVersion").IsRequired().IsRowVersion();
        builder.Ignore(x => x.DomainEvents);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.TenantId).IsUnique();
        builder.HasIndex(x => x.RegistrationRequestId).IsUnique();
        builder.HasIndex(x => new { x.TenantId, x.Status });
        builder.HasIndex(x => x.ProvisioningOperationId)
            .IsUnique()
            .HasFilter("[ProvisioningOperationId] IS NOT NULL");
    }
}

internal sealed class ExternalIdentifierMappingConfiguration : IEntityTypeConfiguration<ExternalIdentifierMapping>
{
    public void Configure(EntityTypeBuilder<ExternalIdentifierMapping> builder)
    {
        builder.ToTable("external_id_mappings", "registry", table =>
            table.HasCheckConstraint(
                "ck_registry_external_id_mappings_created_utc",
                "DATEPART(TZOFFSET, [CreatedAtUtc]) = 0"));
        builder.HasKey(x => x.Id);
        builder.Property(x => x.SourceSystem).HasMaxLength(100).IsRequired();
        builder.Property(x => x.EntityType).HasMaxLength(100).IsRequired();
        builder.Property(x => x.ExternalId).HasMaxLength(200).IsRequired();
        builder.Property(x => x.InternalId).IsRequired();
        builder.Property(x => x.TenantId)
            .HasConversion(
                id => id.HasValue ? id.Value.Value : (Guid?)null,
                value => value.HasValue ? TenantId.From(value.Value) : (TenantId?)null);
        builder.Property(x => x.CreatedAtUtc).HasPrecision(3);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(x => x.TenantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.SourceSystem, x.EntityType, x.ExternalId })
            .IsUnique()
            .HasFilter("[TenantId] IS NULL");
        builder.HasIndex(x => new { x.TenantId, x.SourceSystem, x.EntityType, x.ExternalId })
            .IsUnique()
            .HasFilter("[TenantId] IS NOT NULL");
        builder.HasIndex(x => new { x.EntityType, x.InternalId });
        builder.HasIndex(x => x.TenantId);
    }
}
