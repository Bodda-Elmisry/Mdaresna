using Mdaresna.Doamin.Models.SettingsManagement;
using Mdaresna.Doamin.Models.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.ModelsConfigrations.SettingsManagement
{
    public class UserLegalPolicyAcceptanceConfig : IEntityTypeConfiguration<UserLegalPolicyAcceptance>
    {
        public void Configure(EntityTypeBuilder<UserLegalPolicyAcceptance> builder)
        {
            builder.HasKey(acceptance => acceptance.Id);

            builder
                .HasOne<LegalPolicyVersion>()
                .WithMany()
                .HasForeignKey(acceptance => acceptance.LegalPolicyVersionId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(acceptance => acceptance.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne<User>()
                .WithMany()
                .HasForeignKey(acceptance => acceptance.RevokedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasIndex(acceptance => new
                {
                    acceptance.UserId,
                    acceptance.LegalPolicyVersionId
                })
                .IsUnique()
                .HasFilter("[RevokedAtUtc] IS NULL AND [Deleted] = 0");

            builder.HasIndex(acceptance => acceptance.LegalPolicyVersionId);
            builder.HasIndex(acceptance => acceptance.AcceptedAtUtc);
        }
    }
}
