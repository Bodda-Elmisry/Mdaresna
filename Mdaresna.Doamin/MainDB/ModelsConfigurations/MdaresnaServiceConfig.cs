using Mdaresna.Doamin.MainDB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.MainDB.ModelsConfigurations;

public class MdaresnaServiceConfig : IEntityTypeConfiguration<MdaresnaService>
{
    public void Configure(EntityTypeBuilder<MdaresnaService> builder)
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
    }
}
