using Mdaresna.Schools.Domain.Academics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

internal sealed class ClassOperationalAssignmentConfiguration : IEntityTypeConfiguration<ClassOperationalAssignment>
{
    public void Configure(EntityTypeBuilder<ClassOperationalAssignment> b)
    {
        b.ToTable("class_operational_assignments"); b.HasKey(x => x.Id);
        b.Property(x => x.Responsibility).HasMaxLength(200).IsRequired();
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.HasIndex(x => new { x.UserId, x.IsActive, x.StartsOn, x.EndsOn });
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.BuildingFloor).WithMany().HasForeignKey(x => x.BuildingFloorId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class ClassOperationalGrantConfiguration : IEntityTypeConfiguration<ClassOperationalGrant>
{
    public void Configure(EntityTypeBuilder<ClassOperationalGrant> b)
    {
        b.ToTable("class_operational_grants"); b.HasKey(x => x.Id);
        b.Property(x => x.Action).HasMaxLength(64).IsRequired();
        b.HasIndex(x => new { x.ClassSectionId, x.Action });
        b.HasOne(x => x.Assignment).WithMany(x => x.Grants).HasForeignKey(x => x.AssignmentId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ClassSection).WithMany().HasForeignKey(x => x.ClassSectionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.GradeSubjectOffering).WithMany().HasForeignKey(x => x.GradeSubjectOfferingId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class ClassOperationalAssignmentAuditConfiguration : IEntityTypeConfiguration<ClassOperationalAssignmentAudit>
{
    public void Configure(EntityTypeBuilder<ClassOperationalAssignmentAudit> b)
    {
        b.ToTable("class_operational_assignment_audits"); b.HasKey(x => x.Id);
        b.Property(x => x.Action).HasMaxLength(32); b.Property(x => x.SnapshotJson).IsRequired();
        b.HasOne(x => x.Assignment).WithMany().HasForeignKey(x => x.AssignmentId).OnDelete(DeleteBehavior.Restrict);
    }
}
