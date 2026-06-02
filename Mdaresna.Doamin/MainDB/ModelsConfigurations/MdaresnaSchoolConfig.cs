using Mdaresna.Doamin.MainDB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.MainDB.ModelsConfigurations;

public class MdaresnaSchoolConfig : IEntityTypeConfiguration<MdaresnaSchool>
{
    public void Configure(EntityTypeBuilder<MdaresnaSchool> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(e => e.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

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
