using Mdaresna.Schools.Domain.Students;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Schools.Infrastructure.Persistence;

internal sealed class HomeworkAssignmentConfiguration : IEntityTypeConfiguration<HomeworkAssignment>
{
    public void Configure(EntityTypeBuilder<HomeworkAssignment> b)
    {
        b.ToTable("homework_assignments"); b.HasKey(x => x.Id);
        b.Property(x => x.DeliveryMode).HasConversion<string>().HasMaxLength(16);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(16);
        b.Property(x => x.Title).HasMaxLength(200).IsRequired();
        b.Property(x => x.Instructions).HasMaxLength(6000).IsRequired();
        b.Property(x => x.BookReference).HasMaxLength(300);
        b.Property(x => x.TimeZoneIdSnapshot).HasMaxLength(100).IsRequired();
        b.Property(x => x.TotalScore).HasPrecision(10, 2);
        b.Property(x => x.ShowCorrectAnswersAfter).HasConversion<string>().HasMaxLength(24);
        b.HasIndex(x => new { x.ClassSectionSubjectId, x.Status, x.DueAtUtc });
        b.HasOne(x => x.ClassSectionSubject).WithMany().HasForeignKey(x => x.ClassSectionSubjectId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CurriculumSubjectBook).WithMany().HasForeignKey(x => x.CurriculumSubjectBookId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.CreatedByUser).WithMany().HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.PublishedByUser).WithMany().HasForeignKey(x => x.PublishedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class HomeworkQuestionConfiguration : IEntityTypeConfiguration<HomeworkQuestion>
{
    public void Configure(EntityTypeBuilder<HomeworkQuestion> b)
    {
        b.ToTable("homework_questions"); b.HasKey(x => x.Id);
        b.Property(x => x.Type).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Prompt).HasMaxLength(3000).IsRequired();
        b.Property(x => x.MaxScore).HasPrecision(10, 2);
        b.Property(x => x.ModelAnswer).HasMaxLength(4000); b.Property(x => x.Explanation).HasMaxLength(4000);
        b.HasIndex(x => new { x.HomeworkAssignmentId, x.SortOrder }).IsUnique();
        b.HasOne(x => x.HomeworkAssignment).WithMany(x => x.Questions).HasForeignKey(x => x.HomeworkAssignmentId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class HomeworkQuestionOptionConfiguration : IEntityTypeConfiguration<HomeworkQuestionOption>
{
    public void Configure(EntityTypeBuilder<HomeworkQuestionOption> b)
    {
        b.ToTable("homework_question_options"); b.HasKey(x => x.Id); b.Property(x => x.Text).HasMaxLength(1000).IsRequired();
        b.HasIndex(x => new { x.HomeworkQuestionId, x.SortOrder }).IsUnique();
        b.HasOne(x => x.HomeworkQuestion).WithMany(x => x.Options).HasForeignKey(x => x.HomeworkQuestionId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class HomeworkQuestionBlankConfiguration : IEntityTypeConfiguration<HomeworkQuestionBlank>
{
    public void Configure(EntityTypeBuilder<HomeworkQuestionBlank> b)
    {
        b.ToTable("homework_question_blanks"); b.HasKey(x => x.Id); b.Property(x => x.Token).HasMaxLength(100).IsRequired(); b.Property(x => x.MaxScore).HasPrecision(10, 2);
        b.HasIndex(x => new { x.HomeworkQuestionId, x.SortOrder }).IsUnique();
        b.HasOne(x => x.HomeworkQuestion).WithMany(x => x.Blanks).HasForeignKey(x => x.HomeworkQuestionId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class HomeworkBlankAcceptedAnswerConfiguration : IEntityTypeConfiguration<HomeworkBlankAcceptedAnswer>
{
    public void Configure(EntityTypeBuilder<HomeworkBlankAcceptedAnswer> b)
    {
        b.ToTable("homework_blank_accepted_answers"); b.HasKey(x => x.Id); b.Property(x => x.Answer).HasMaxLength(1000).IsRequired(); b.Property(x => x.NormalizedAnswer).HasMaxLength(1000).IsRequired();
        b.HasIndex(x => new { x.HomeworkQuestionBlankId, x.SortOrder }).IsUnique();
        b.HasOne(x => x.HomeworkQuestionBlank).WithMany(x => x.AcceptedAnswers).HasForeignKey(x => x.HomeworkQuestionBlankId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class StudentHomeworkConfiguration : IEntityTypeConfiguration<StudentHomework>
{
    public void Configure(EntityTypeBuilder<StudentHomework> b)
    {
        b.ToTable("student_homework"); b.HasKey(x => x.Id); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32); b.Property(x => x.FinalScore).HasPrecision(10, 2); b.Property(x => x.TeacherFeedback).HasMaxLength(4000); b.Property(x => x.ExcuseReason).HasMaxLength(1000);
        b.HasIndex(x => new { x.HomeworkAssignmentId, x.StudentEnrollmentId }).IsUnique();
        b.HasOne(x => x.HomeworkAssignment).WithMany(x => x.Students).HasForeignKey(x => x.HomeworkAssignmentId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.StudentEnrollment).WithMany().HasForeignKey(x => x.StudentEnrollmentId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.GradedByUser).WithMany().HasForeignKey(x => x.GradedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class HomeworkSubmissionAttemptConfiguration : IEntityTypeConfiguration<HomeworkSubmissionAttempt>
{
    public void Configure(EntityTypeBuilder<HomeworkSubmissionAttempt> b)
    {
        b.ToTable("homework_submission_attempts"); b.HasKey(x => x.Id); b.Property(x => x.Channel).HasConversion<string>().HasMaxLength(16); b.Property(x => x.Status).HasConversion<string>().HasMaxLength(32); b.Property(x => x.SubmittedByActorType).HasMaxLength(24); b.Property(x => x.AutoScore).HasPrecision(10, 2); b.Property(x => x.FinalScore).HasPrecision(10, 2);
        b.HasIndex(x => new { x.StudentHomeworkId, x.AttemptNumber }).IsUnique();
        b.HasOne(x => x.StudentHomework).WithMany(x => x.Attempts).HasForeignKey(x => x.StudentHomeworkId).OnDelete(DeleteBehavior.Cascade);
    }
}

internal sealed class HomeworkStudentAnswerConfiguration : IEntityTypeConfiguration<HomeworkStudentAnswer>
{
    public void Configure(EntityTypeBuilder<HomeworkStudentAnswer> b)
    {
        b.ToTable("homework_student_answers"); b.HasKey(x => x.Id); b.Property(x => x.TextAnswer).HasMaxLength(6000); b.Property(x => x.GradingStatus).HasConversion<string>().HasMaxLength(32); b.Property(x => x.Score).HasPrecision(10, 2); b.Property(x => x.TeacherFeedback).HasMaxLength(4000);
        b.HasIndex(x => new { x.HomeworkSubmissionAttemptId, x.HomeworkQuestionId }).IsUnique();
        b.HasOne(x => x.Attempt).WithMany(x => x.Answers).HasForeignKey(x => x.HomeworkSubmissionAttemptId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.HomeworkQuestion).WithMany(x => x.Answers).HasForeignKey(x => x.HomeworkQuestionId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.GradedByUser).WithMany().HasForeignKey(x => x.GradedByUserId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class HomeworkStudentSelectedOptionConfiguration : IEntityTypeConfiguration<HomeworkStudentSelectedOption>
{
    public void Configure(EntityTypeBuilder<HomeworkStudentSelectedOption> b)
    {
        b.ToTable("homework_student_selected_options"); b.HasKey(x => new { x.HomeworkStudentAnswerId, x.HomeworkQuestionOptionId });
        b.HasOne(x => x.HomeworkStudentAnswer).WithMany(x => x.SelectedOptions).HasForeignKey(x => x.HomeworkStudentAnswerId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.HomeworkQuestionOption).WithMany(x => x.SelectedByAnswers).HasForeignKey(x => x.HomeworkQuestionOptionId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class HomeworkStudentBlankAnswerConfiguration : IEntityTypeConfiguration<HomeworkStudentBlankAnswer>
{
    public void Configure(EntityTypeBuilder<HomeworkStudentBlankAnswer> b)
    {
        b.ToTable("homework_student_blank_answers"); b.HasKey(x => x.Id); b.Property(x => x.Answer).HasMaxLength(1000).IsRequired(); b.Property(x => x.NormalizedAnswer).HasMaxLength(1000).IsRequired(); b.Property(x => x.Score).HasPrecision(10, 2);
        b.HasIndex(x => new { x.HomeworkStudentAnswerId, x.HomeworkQuestionBlankId }).IsUnique();
        b.HasOne(x => x.HomeworkStudentAnswer).WithMany(x => x.BlankAnswers).HasForeignKey(x => x.HomeworkStudentAnswerId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.HomeworkQuestionBlank).WithMany(x => x.StudentAnswers).HasForeignKey(x => x.HomeworkQuestionBlankId).OnDelete(DeleteBehavior.Restrict);
    }
}

internal sealed class HomeworkAuditConfiguration : IEntityTypeConfiguration<HomeworkAudit>
{
    public void Configure(EntityTypeBuilder<HomeworkAudit> b)
    {
        b.ToTable("homework_audits"); b.HasKey(x => x.Id); b.Property(x => x.Action).HasMaxLength(40).IsRequired();
        b.HasIndex(x => new { x.HomeworkAssignmentId, x.CreatedAtUtc });
        b.HasOne(x => x.HomeworkAssignment).WithMany(x => x.AuditTrail).HasForeignKey(x => x.HomeworkAssignmentId).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.StudentHomework).WithMany().HasForeignKey(x => x.StudentHomeworkId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.ActorUser).WithMany().HasForeignKey(x => x.ActorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
