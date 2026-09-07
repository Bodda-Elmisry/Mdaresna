using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Doamin.Models.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.ModelsConfigrations.SettingsManagement
{
    public class VerificationDeliveryAttemptConfig : IEntityTypeConfiguration<VerificationDeliveryAttempt>
    {
        public void Configure(EntityTypeBuilder<VerificationDeliveryAttempt> builder)
        {
            builder.HasKey(item => item.Id);
            builder.Property(item => item.PhoneNumber).HasMaxLength(50).IsRequired();
            builder.Property(item => item.ProviderResponse).HasMaxLength(500);

            builder.HasOne<VerificationChallenge>()
                .WithMany()
                .HasForeignKey(item => item.VerificationChallengeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(item => item.SentByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(item => new
            {
                item.PhoneNumber,
                item.Purpose,
                item.Channel,
                item.CreateDate
            });
        }
    }
}
