using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SettingsManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.ModelsConfigrations.SettingsManagement
{
    public class ReportQueueConfig : IEntityTypeConfiguration<ReportQueue>
    {
        public void Configure(EntityTypeBuilder<ReportQueue> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.CreatedAt)
                .IsRequired();

            builder.Property(e => e.Status)
                .IsRequired()
                .HasDefaultValue(ReportQueueStatusEnum.Queued);

            builder.Property(e => e.RetryCount)
                .IsRequired()
                .HasDefaultValue(0);

            builder
                .HasOne(e => e.School)
                .WithMany()
                .HasForeignKey(e => e.SchoolId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Grade)
                .WithMany()
                .HasForeignKey(e => e.GradeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Classroom)
                .WithMany()
                .HasForeignKey(e => e.ClassroomId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Month)
                .WithMany()
                .HasForeignKey(e => e.MonthId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.ReviewdBy)
                .WithMany()
                .HasForeignKey(e => e.ReviewdById)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.CreatedBy)
                .WithMany()
                .HasForeignKey(e => e.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
