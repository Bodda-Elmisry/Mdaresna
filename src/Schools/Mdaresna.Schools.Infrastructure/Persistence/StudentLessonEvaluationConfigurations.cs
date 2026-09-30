using Mdaresna.Schools.Domain.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

internal sealed class StudentLessonEvaluationRegisterConfiguration : IEntityTypeConfiguration<StudentLessonEvaluationRegister>
{
    public void Configure(EntityTypeBuilder<StudentLessonEvaluationRegister> b)
    {
        b.ToTable("student_lesson_evaluation_registers"); b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
        b.Property(x => x.TimeZoneIdSnapshot).HasMaxLength(100).IsRequired();
        b.HasIndex(x => new { x.ClassSectionId, x.LessonDate, x.WeeklyTimetableSlotId }).IsUnique();
        b.HasOne(x => x.ClassSection).WithMany().HasForeignKey(x => x.ClassSectionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.WeeklyTimetableSlot).WithMany().HasForeignKey(x => x.WeeklyTimetableSlotId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.EvaluatedByUser).WithMany().HasForeignKey(x => x.EvaluatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.FinalizedByUser).WithMany().HasForeignKey(x => x.FinalizedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class StudentLessonEvaluationEntryConfiguration : IEntityTypeConfiguration<StudentLessonEvaluationEntry>
{
    public void Configure(EntityTypeBuilder<StudentLessonEvaluationEntry> b)
    {
        b.ToTable("student_lesson_evaluation_entries"); b.HasKey(x => x.Id);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
        b.HasIndex(x => new { x.RegisterId, x.StudentEnrollmentId }).IsUnique();
        b.HasOne(x => x.Register).WithMany(x => x.Entries).HasForeignKey(x => x.RegisterId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.StudentEnrollment).WithMany().HasForeignKey(x => x.StudentEnrollmentId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class StudentLessonEvaluationAuditConfiguration : IEntityTypeConfiguration<StudentLessonEvaluationAudit>
{
    public void Configure(EntityTypeBuilder<StudentLessonEvaluationAudit> b)
    {
        b.ToTable("student_lesson_evaluation_audits"); b.HasKey(x => x.Id);
        b.Property(x => x.Action).HasMaxLength(24).IsRequired();
        b.Property(x => x.SnapshotJson).HasMaxLength(12000).IsRequired();
        b.HasIndex(x => new { x.RegisterId, x.CreatedAtUtc });
        b.HasOne(x => x.Register).WithMany(x => x.AuditTrail).HasForeignKey(x => x.RegisterId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
