using Mdaresna.Doamin.MainDB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.MainDB.ModelsConfigurations;

public class MdaresnaSchoolServiceConfig : IEntityTypeConfiguration<MdaresnaSchoolService>
{
    public void Configure(EntityTypeBuilder<MdaresnaSchoolService> builder)
    {
        builder.HasKey(e => new { e.SchoolId, e.ServiceId });

        builder.HasOne(e => e.School)
            .WithMany()
            .HasForeignKey(e => e.SchoolId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Service)
            .WithMany()
            .HasForeignKey(e => e.ServiceId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(e => e.DBSource)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.DBPort)
            .IsRequired(false)
            .HasMaxLength(8);

        builder.Property(e => e.DBUser)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.DBPassword)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.DBCatlog)
            .IsRequired()
            .HasMaxLength(150);

    }
}
