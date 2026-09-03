using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.ModelsConfigrations.SchoolManagement.SchoolManagement
{
    public class SchoolPostReactionConfig : IEntityTypeConfiguration<SchoolPostReaction>
    {
        public void Configure(EntityTypeBuilder<SchoolPostReaction> builder)
        {
            builder
                .Property(reaction => reaction.ReactionType)
                .HasDefaultValue(SchoolPostReactionTypeEnum.Like);

            builder
                .HasIndex(reaction => new { reaction.PostId, reaction.UserId })
                .IsUnique();

            builder
                .HasOne(reaction => reaction.Post)
                .WithMany()
                .HasForeignKey(reaction => reaction.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            builder
                .HasOne(reaction => reaction.User)
                .WithMany()
                .HasForeignKey(reaction => reaction.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
