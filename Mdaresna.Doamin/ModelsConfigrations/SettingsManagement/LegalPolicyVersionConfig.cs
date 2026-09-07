using Mdaresna.Doamin.Models.SettingsManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.ModelsConfigrations.SettingsManagement
{
    public class LegalPolicyVersionConfig : IEntityTypeConfiguration<LegalPolicyVersion>
    {
        public static readonly Guid InitialPolicyId =
            Guid.Parse("E302C5A7-39AD-4A59-9593-E80E544FC246");

        public void Configure(EntityTypeBuilder<LegalPolicyVersion> builder)
        {
            builder.HasKey(policy => policy.Id);
            builder.Property(policy => policy.Version).HasMaxLength(50).IsRequired();
            builder.Property(policy => policy.TitleAr).HasMaxLength(300).IsRequired();
            builder.Property(policy => policy.TitleEn).HasMaxLength(300).IsRequired();

            builder
                .HasIndex(policy => policy.Version)
                .IsUnique()
                .HasFilter("[Deleted] = 0");

            builder
                .HasIndex(policy => policy.IsActive)
                .IsUnique()
                .HasFilter("[IsActive] = 1 AND [Deleted] = 0");

            builder.HasData(new LegalPolicyVersion
            {
                Id = InitialPolicyId,
                Version = "2026-05-22",
                TitleAr = "سياسة الخصوصية وشروط الاستخدام",
                TitleEn = "Privacy Policy and Terms of Use",
                PrivacyPolicyAr = string.Empty,
                PrivacyPolicyEn = string.Empty,
                UgcTermsAr = string.Empty,
                UgcTermsEn = string.Empty,
                IsActive = true,
                EffectiveDateUtc = new DateTime(2026, 5, 22, 0, 0, 0, DateTimeKind.Utc),
                CreateDate = new DateTime(2026, 5, 22, 0, 0, 0, DateTimeKind.Utc),
                LastModifyDate = new DateTime(2026, 5, 22, 0, 0, 0, DateTimeKind.Utc),
                Deleted = false
            });
        }
    }
}
