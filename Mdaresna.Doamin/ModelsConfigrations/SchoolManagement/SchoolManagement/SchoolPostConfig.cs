using Mdaresna.Doamin.Enums;
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
                .HasIndex(post => new
                {
                    post.SchoolId,
                    post.ModerationStatus,
                    post.Visibility,
                    post.LastModifyDate,
                    post.PostDate
                })
                .HasDatabaseName("IX_SchoolPosts_Feed")
                .HasFilter("[Deleted] = 0");

            builder
                .Property(p => p.ModerationReason)
                .HasMaxLength(120);

            builder
                .Property(p => p.Visibility)
                .HasDefaultValue(SchoolPostVisibilityEnum.Public);

            builder
                .HasOne(p => p.School)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
