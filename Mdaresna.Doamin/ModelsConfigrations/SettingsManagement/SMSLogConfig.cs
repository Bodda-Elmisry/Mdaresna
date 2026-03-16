using Mdaresna.Doamin.Models.SettingsManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.ModelsConfigrations.SettingsManagement
{
    public class SMSLogConfig : IEntityTypeConfiguration<SMSLog>
    {
        public void Configure(EntityTypeBuilder<SMSLog> builder)
        {
            builder
                .HasOne(e => e.SMSProvider)
                .WithMany()
                .HasForeignKey(e => e.SMSProviderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
