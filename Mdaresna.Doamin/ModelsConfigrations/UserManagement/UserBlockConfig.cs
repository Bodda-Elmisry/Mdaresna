using Mdaresna.Doamin.Models.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.ModelsConfigrations.UserManagement
{
    public class UserBlockConfig : IEntityTypeConfiguration<UserBlock>
    {
        public void Configure(EntityTypeBuilder<UserBlock> builder)
        {
            builder
                .HasOne(b => b.BlockerUser)
                .WithMany()
                .HasForeignKey(b => b.BlockerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(b => b.BlockedUser)
                .WithMany()
                .HasForeignKey(b => b.BlockedUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasIndex(b => new { b.BlockerUserId, b.BlockedUserId })
                .IsUnique();
        }
    }
}
