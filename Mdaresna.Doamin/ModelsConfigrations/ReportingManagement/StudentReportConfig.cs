using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.ReportingManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.ModelsConfigrations.ReportingManagement;

public class StudentReportConfig : IEntityTypeConfiguration<StudentReport>
{
    public void Configure(EntityTypeBuilder<StudentReport> builder)
    {
        builder.ToTable("StudentReports");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.SchoolId)
            .IsRequired();

        builder.Property(e => e.StudentId)
            .IsRequired();

        builder.Property(e => e.ReportQueueId)
            .IsRequired(false);

        builder.Property(e => e.GradeId)
            .IsRequired(false);

        builder.Property(e => e.ClassRoomId)
            .IsRequired(false);

        builder.Property(e => e.MonthId)
            .IsRequired(false);

        builder.Property(e => e.WeekName)
            .HasMaxLength(100)
            .IsRequired(false);

        builder.Property(e => e.ReportDetails)
            .HasColumnName("ReportDeatils")
            .HasColumnType("nvarchar(max)")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("CeratedAt")
            .IsRequired()
            .HasDefaultValueSql("SYSUTCDATETIME()");

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.Version)
            .IsRequired()
            .HasDefaultValue(1);

        builder.Property(e => e.ReportType)
            .IsRequired()
            .HasDefaultValue(StudentReportTypesEnum.Monthly);

        builder.HasIndex(e => e.ReportQueueId);

        builder.HasIndex(e => e.GradeId);

        builder.HasIndex(e => e.ClassRoomId);

        builder.HasCheckConstraint(
            "CK_StudentReports_ReportDeatils_IsJson",
            "ISJSON([ReportDeatils]) = 1");
    }
}
