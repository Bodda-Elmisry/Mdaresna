using Mdaresna.Schools.Domain.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

internal static class OrganizationConfiguration
{
    public static void Common<TEntity>(EntityTypeBuilder<TEntity> b) where TEntity : class, Mdaresna.Schools.Domain.Facilities.ISoftDeletableSchoolEntity => b.HasQueryFilter(x => !x.IsDeleted);
}

internal sealed class SchoolDepartmentConfiguration : IEntityTypeConfiguration<SchoolDepartment>
{
    public void Configure(EntityTypeBuilder<SchoolDepartment> b)
    {
        b.ToTable("school_departments"); b.HasKey(x => x.Id); OrganizationConfiguration.Common(b);
        b.Property(x => x.Code).HasMaxLength(50).IsRequired(); b.Property(x => x.NameAr).HasMaxLength(150).IsRequired(); b.Property(x => x.NameEn).HasMaxLength(150).IsRequired();
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(24);
        b.HasOne(x => x.ParentDepartment).WithMany(x => x.Children).HasForeignKey(x => x.ParentDepartmentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Branch).WithMany().HasForeignKey(x => x.BranchId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class DepartmentMembershipConfiguration : IEntityTypeConfiguration<DepartmentMembership>
{
    public void Configure(EntityTypeBuilder<DepartmentMembership> b)
    {
        b.ToTable("department_memberships"); b.HasKey(x => x.Id); OrganizationConfiguration.Common(b);
        b.Property(x => x.TitleAr).HasMaxLength(150); b.Property(x => x.TitleEn).HasMaxLength(150);
        b.HasOne(x => x.Department).WithMany(x => x.Memberships).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.User).WithMany(x => x.DepartmentMemberships).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class DepartmentLeadershipConfiguration : IEntityTypeConfiguration<DepartmentLeadership>
{
    public void Configure(EntityTypeBuilder<DepartmentLeadership> b)
    {
        b.ToTable("department_leaderships"); b.HasKey(x => x.Id); OrganizationConfiguration.Common(b);
        b.Property(x => x.Role).HasConversion<string>().HasMaxLength(24);
        b.HasOne(x => x.Department).WithMany(x => x.Leaderships).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class AcademicDepartmentSubjectConfiguration : IEntityTypeConfiguration<AcademicDepartmentSubject>
{
    public void Configure(EntityTypeBuilder<AcademicDepartmentSubject> b)
    {
        b.ToTable("academic_department_subjects"); b.HasKey(x => x.Id); OrganizationConfiguration.Common(b);
        b.HasOne(x => x.Department).WithMany(x => x.Subjects).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Subject).WithMany().HasForeignKey(x => x.SubjectId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class SubjectCoordinatorAssignmentConfiguration : IEntityTypeConfiguration<SubjectCoordinatorAssignment>
{
    public void Configure(EntityTypeBuilder<SubjectCoordinatorAssignment> b)
    {
        b.ToTable("subject_coordinator_assignments"); b.HasKey(x => x.Id); OrganizationConfiguration.Common(b);
        b.HasOne(x => x.DepartmentSubject).WithMany(x => x.Coordinators).HasForeignKey(x => x.DepartmentSubjectId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CoordinatorUser).WithMany().HasForeignKey(x => x.CoordinatorUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.EducationProgram).WithMany().HasForeignKey(x => x.EducationProgramId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.EducationStage).WithMany().HasForeignKey(x => x.EducationStageId).OnDelete(DeleteBehavior.Restrict);
    }
}
