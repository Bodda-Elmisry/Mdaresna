using Mdaresna.Schools.Domain.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

internal sealed class ClassActivityConfiguration : IEntityTypeConfiguration<ClassActivity>
{
    public void Configure(EntityTypeBuilder<ClassActivity> b)
    {
        b.ToTable("class_activities"); b.HasKey(x => x.Id);
        b.Property(x => x.Scope).HasConversion<string>().HasMaxLength(16);
        b.Property(x => x.Category).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
        b.Property(x => x.AudienceMode).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Details).HasMaxLength(6000).IsRequired();
        b.Property(x => x.Location).HasMaxLength(300);
        b.Property(x => x.TotalScore).HasPrecision(10, 2);
        b.Property(x => x.TimeZoneIdSnapshot).HasMaxLength(100).IsRequired();
        b.Property(x => x.CancellationReason).HasMaxLength(1000);
        b.HasIndex(x => new { x.ClassSectionId, x.Status, x.ActivityDate });
        b.HasIndex(x => new { x.ClassSectionSubjectId, x.ActivityDate });
        b.HasOne(x => x.ClassSection).WithMany().HasForeignKey(x => x.ClassSectionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ClassSectionSubject).WithMany().HasForeignKey(x => x.ClassSectionSubjectId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.PublishedByUser).WithMany().HasForeignKey(x => x.PublishedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CompletedByUser).WithMany().HasForeignKey(x => x.CompletedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CancelledByUser).WithMany().HasForeignKey(x => x.CancelledByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ClassActivityAudienceStudentConfiguration : IEntityTypeConfiguration<ClassActivityAudienceStudent>
{
    public void Configure(EntityTypeBuilder<ClassActivityAudienceStudent> b)
    {
        b.ToTable("class_activity_audience_students"); b.HasKey(x => new { x.ClassActivityId, x.StudentEnrollmentId });
        b.HasOne(x => x.ClassActivity).WithMany(x => x.AudienceStudents).HasForeignKey(x => x.ClassActivityId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.StudentEnrollment).WithMany().HasForeignKey(x => x.StudentEnrollmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ClassActivityParticipantConfiguration : IEntityTypeConfiguration<ClassActivityParticipant>
{
    public void Configure(EntityTypeBuilder<ClassActivityParticipant> b)
    {
        b.ToTable("class_activity_participants"); b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Score).HasPrecision(10, 2);
        b.Property(x => x.Note).HasMaxLength(2000);
        b.HasIndex(x => new { x.ClassActivityId, x.StudentEnrollmentId }).IsUnique();
        b.HasOne(x => x.ClassActivity).WithMany(x => x.Participants).HasForeignKey(x => x.ClassActivityId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.StudentEnrollment).WithMany().HasForeignKey(x => x.StudentEnrollmentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.EvaluatedByUser).WithMany().HasForeignKey(x => x.EvaluatedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class ClassActivityAuditConfiguration : IEntityTypeConfiguration<ClassActivityAudit>
{
    public void Configure(EntityTypeBuilder<ClassActivityAudit> b)
    {
        b.ToTable("class_activity_audits"); b.HasKey(x => x.Id);
        b.Property(x => x.Action).HasMaxLength(40).IsRequired();
        b.Property(x => x.Reason).HasMaxLength(1000);
        b.HasIndex(x => new { x.ClassActivityId, x.CreatedAtUtc });
        b.HasOne(x => x.ClassActivity).WithMany(x => x.AuditTrail).HasForeignKey(x => x.ClassActivityId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
