using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Doamin.Models.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.ModelsConfigrations.SettingsManagement
{
    public class WhatsAppVerificationRequestConfig : IEntityTypeConfiguration<WhatsAppVerificationRequest>
    {
        public void Configure(EntityTypeBuilder<WhatsAppVerificationRequest> builder)
        {
            builder.HasKey(item => item.Id);
            builder.Property(item => item.RowVersion).IsRowVersion();

            builder.HasOne<VerificationChallenge>()
                .WithMany()
                .HasForeignKey(item => item.VerificationChallengeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(item => item.PreparedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<User>()
                .WithMany()
                .HasForeignKey(item => item.SentConfirmedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasIndex(item => item.VerificationChallengeId).IsUnique();
            builder.HasIndex(item => new { item.Status, item.RequestedAtUtc });
        }
    }
}
