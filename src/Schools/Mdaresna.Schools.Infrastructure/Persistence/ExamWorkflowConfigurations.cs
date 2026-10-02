using Mdaresna.Schools.Domain.Exams;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
namespace Mdaresna.Schools.Infrastructure.Persistence;

internal sealed class ExamWorkflowConfiguration : IEntityTypeConfiguration<ExamWorkflow>
{
    public void Configure(EntityTypeBuilder<ExamWorkflow> b) {
        b.ToTable("exam_workflows"); b.HasKey(x => x.ExamSeriesId);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Revision).IsConcurrencyToken();
        b.HasOne(x => x.Series).WithMany().HasForeignKey(x => x.ExamSeriesId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class ExamWorkflowStepConfiguration : IEntityTypeConfiguration<ExamWorkflowStep>
{
    public void Configure(EntityTypeBuilder<ExamWorkflowStep> b) {
        b.ToTable("exam_workflow_steps"); b.HasKey(x => x.Id);
        b.Property(x => x.Stage).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.Status).HasConversion<string>().HasMaxLength(24);
        b.Property(x => x.ReturnReason).HasMaxLength(1000);
        b.HasIndex(x => new { x.AssigneeUserId, x.Status, x.DueAtUtc });
        b.HasOne(x => x.Workflow).WithMany(x => x.Steps).HasForeignKey(x => x.ExamSeriesId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<ExamPaper>().WithMany().HasForeignKey(x => x.ExamPaperId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Mdaresna.Schools.Domain.Identity.LocalUserAccount>().WithMany().HasForeignKey(x => x.AssigneeUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Mdaresna.Schools.Domain.Identity.LocalUserAccount>().WithMany().HasForeignKey(x => x.BackupUserId).OnDelete(DeleteBehavior.Restrict);
        b.HasOne<Mdaresna.Schools.Domain.Identity.LocalUserAccount>().WithMany().HasForeignKey(x => x.SupervisorUserId).OnDelete(DeleteBehavior.Restrict);
    }
}
internal sealed class ExamNotificationOutboxConfiguration : IEntityTypeConfiguration<ExamNotificationOutbox>
{
    public void Configure(EntityTypeBuilder<ExamNotificationOutbox> b) {
        b.ToTable("exam_notification_outbox"); b.HasKey(x => x.Id);
        b.Property(x => x.EventKey).HasMaxLength(250);
        b.HasIndex(x => x.EventKey).IsUnique();
        b.HasIndex(x => x.DeliveredAtUtc);
        b.HasOne<Mdaresna.Schools.Domain.Identity.SchoolUserNotification>().WithMany().HasForeignKey(x => x.NotificationId).OnDelete(DeleteBehavior.Restrict);
    }
}
