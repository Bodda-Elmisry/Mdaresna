using Mdaresna.Schools.Domain.Academics;
using Mdaresna.Schools.Domain.Documents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

internal sealed class SchoolDocumentConfiguration : IEntityTypeConfiguration<SchoolDocument>
{
    public void Configure(EntityTypeBuilder<SchoolDocument> b)
    {
        b.ToTable("documents"); b.HasKey(x => x.Id);
        b.Property(x => x.Title).HasMaxLength(250).IsRequired();
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
    }
}
internal sealed class SchoolDocumentVersionConfiguration : IEntityTypeConfiguration<SchoolDocumentVersion>
{
    public void Configure(EntityTypeBuilder<SchoolDocumentVersion> b)
    {
        b.ToTable("document_versions"); b.HasKey(x => x.Id);
        b.Property(x => x.OriginalFileName).HasMaxLength(255).IsRequired();
        b.Property(x => x.ContentType).HasMaxLength(150).IsRequired();
        b.Property(x => x.Sha256).HasMaxLength(64).IsRequired();
        b.Property(x => x.StorageKey).HasMaxLength(500).IsRequired();
        b.Property(x => x.ValidationStatus).HasConversion<string>().HasMaxLength(24);
        b.HasIndex(x => new { x.DocumentId, x.VersionNumber }).IsUnique();
        b.HasOne(x => x.Document).WithMany(x => x.Versions).HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class SchoolDocumentAuditConfiguration : IEntityTypeConfiguration<SchoolDocumentAudit>
{
    public void Configure(EntityTypeBuilder<SchoolDocumentAudit> b)
    {
        b.ToTable("document_audits"); b.HasKey(x => x.Id);
        b.Property(x => x.Action).HasMaxLength(50).IsRequired(); b.Property(x => x.Details).HasMaxLength(1000);
        b.HasOne(x => x.Document).WithMany().HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class TeachingPlanConfiguration : IEntityTypeConfiguration<TeachingPlan>
{
    public void Configure(EntityTypeBuilder<TeachingPlan> b)
    {
        b.ToTable("teaching_plans"); b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.SourceAuthority).HasConversion<string>().HasMaxLength(40);
        b.Property(x => x.TitleAr).HasMaxLength(250).IsRequired(); b.Property(x => x.TitleEn).HasMaxLength(250).IsRequired();
        b.Property(x => x.Details).HasMaxLength(8000).IsRequired(); b.Property(x => x.SourceReference).HasMaxLength(500);
        b.HasOne(x => x.ProgramAcademicYear).WithMany().HasForeignKey(x => x.ProgramAcademicYearId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.GradeOffering).WithMany().HasForeignKey(x => x.GradeOfferingId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.GradeSubjectOffering).WithMany().HasForeignKey(x => x.GradeSubjectOfferingId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.AcademicTerm).WithMany().HasForeignKey(x => x.AcademicTermId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ParentPlan).WithMany().HasForeignKey(x => x.ParentPlanId).OnDelete(DeleteBehavior.Restrict);
        b.HasIndex(x => new { x.Type, x.GradeSubjectOfferingId, x.FromDate, x.ToDate });
    }
}
internal sealed class TeachingPlanTargetConfiguration : IEntityTypeConfiguration<TeachingPlanTarget>
{
    public void Configure(EntityTypeBuilder<TeachingPlanTarget> b)
    {
        b.ToTable("teaching_plan_targets"); b.HasKey(x => x.Id);
        b.HasIndex(x => new { x.TeachingPlanId, x.ClassSectionId }).IsUnique();
        b.HasOne(x => x.TeachingPlan).WithMany(x => x.Targets).HasForeignKey(x => x.TeachingPlanId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ClassSection).WithMany().HasForeignKey(x => x.ClassSectionId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class TeachingPlanItemConfiguration : IEntityTypeConfiguration<TeachingPlanItem>
{
    public void Configure(EntityTypeBuilder<TeachingPlanItem> b)
    {
        b.ToTable("teaching_plan_items"); b.HasKey(x => x.Id); b.Property(x => x.Title).HasMaxLength(250).IsRequired(); b.Property(x => x.Details).HasMaxLength(4000);
        b.HasOne(x => x.TeachingPlan).WithMany(x => x.Items).HasForeignKey(x => x.TeachingPlanId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class TeachingPlanDocumentConfiguration : IEntityTypeConfiguration<TeachingPlanDocument>
{
    public void Configure(EntityTypeBuilder<TeachingPlanDocument> b)
    {
        b.ToTable("teaching_plan_documents"); b.HasKey(x => x.Id); b.Property(x => x.Purpose).HasConversion<string>().HasMaxLength(24);
        b.HasIndex(x => new { x.TeachingPlanId, x.DocumentId }).IsUnique();
        b.HasOne(x => x.TeachingPlan).WithMany(x => x.Documents).HasForeignKey(x => x.TeachingPlanId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Document).WithMany(x => x.TeachingPlans).HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class TeachingPlanAuditConfiguration : IEntityTypeConfiguration<TeachingPlanAudit>
{
    public void Configure(EntityTypeBuilder<TeachingPlanAudit> b)
    {
        b.ToTable("teaching_plan_audits"); b.HasKey(x => x.Id); b.Property(x => x.Action).HasMaxLength(50).IsRequired(); b.Property(x => x.Details).HasMaxLength(1000);
        b.HasOne(x => x.TeachingPlan).WithMany().HasForeignKey(x => x.TeachingPlanId).OnDelete(DeleteBehavior.Restrict);
    }
}
