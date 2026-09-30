using Mdaresna.Schools.Domain.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

internal sealed class StudentLessonNoteConfiguration : IEntityTypeConfiguration<StudentLessonNote>
{
    public void Configure(EntityTypeBuilder<StudentLessonNote> b)
    {
        b.ToTable("student_lesson_notes", t => t.HasCheckConstraint("CK_student_lesson_notes_rating",
            "\"RatingLevel\" IS NULL OR (\"RatingLevel\" >= 1 AND \"RatingLevel\" <= 5)")); b.HasKey(x => x.Id);
        b.Property(x => x.Category).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Visibility).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.NoteText).HasMaxLength(2000).IsRequired();
        b.HasIndex(x => new { x.ClassSectionId, x.LessonDate, x.WeeklyTimetableSlotId });
        b.HasIndex(x => new { x.StudentEnrollmentId, x.LessonDate });
        b.HasOne(x => x.ClassSection).WithMany().HasForeignKey(x => x.ClassSectionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.StudentEnrollment).WithMany().HasForeignKey(x => x.StudentEnrollmentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.WeeklyTimetableSlot).WithMany().HasForeignKey(x => x.WeeklyTimetableSlotId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.UpdatedByUser).WithMany().HasForeignKey(x => x.UpdatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.DeletedByUser).WithMany().HasForeignKey(x => x.DeletedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class StudentLessonNoteAuditConfiguration : IEntityTypeConfiguration<StudentLessonNoteAudit>
{
    public void Configure(EntityTypeBuilder<StudentLessonNoteAudit> b)
    {
        b.ToTable("student_lesson_note_audits"); b.HasKey(x => x.Id);
        b.Property(x => x.Action).HasMaxLength(24).IsRequired();
        b.Property(x => x.SnapshotJson).HasMaxLength(6000).IsRequired();
        b.HasIndex(x => new { x.StudentLessonNoteId, x.CreatedAtUtc });
        b.HasOne(x => x.StudentLessonNote).WithMany(x => x.AuditTrail).HasForeignKey(x => x.StudentLessonNoteId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
