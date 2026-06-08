using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.ModelsConfigrations.SchoolManagement.SchoolManagement
{
    public class SchoolYearMonthConfig : IEntityTypeConfiguration<SchoolYearMonth>
    {
        public void Configure(EntityTypeBuilder<SchoolYearMonth> builder)
        {
            builder.Property(e => e.ReportStatus)
                .IsRequired()
                .HasDefaultValue(ReportStatusEnum.NotCreated);

            builder
                .HasOne(e => e.Year)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

    //        builder
    //.Property(p => p.CreateDate)
    //.HasDefaultValue(DateTime.Now);

    //        builder
    //            .Property(p => p.LastModifyDate)
    //            .HasDefaultValue(DateTime.Now);
        }
    }
}
