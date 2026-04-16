using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.ModelsConfigrations.SchoolManagement.SchoolManagement
{
    public class SchoolPostConfig : IEntityTypeConfiguration<SchoolPost>
    {
        public void Configure(EntityTypeBuilder<SchoolPost> builder)
        {
            builder
                .Property(p => p.ModerationReason)
                .HasMaxLength(120);

            builder
                .HasOne(p => p.School)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
