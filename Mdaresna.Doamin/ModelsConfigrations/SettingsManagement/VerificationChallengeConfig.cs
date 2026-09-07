using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Doamin.Models.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.ModelsConfigrations.SettingsManagement
{
    public class VerificationChallengeConfig : IEntityTypeConfiguration<VerificationChallenge>
    {
        public void Configure(EntityTypeBuilder<VerificationChallenge> builder)
        {
            builder.HasKey(item => item.Id);
            builder.Property(item => item.PhoneNumber).HasMaxLength(50).IsRequired();
            builder.Property(item => item.CodeHash).HasMaxLength(64);
            builder.Property(item => item.RowVersion).IsRowVersion();

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(item => item.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(item => new { item.PhoneNumber, item.Purpose, item.CreateDate });
            builder.HasIndex(item => new { item.Status, item.ExpiresAtUtc });
        }
    }
}
