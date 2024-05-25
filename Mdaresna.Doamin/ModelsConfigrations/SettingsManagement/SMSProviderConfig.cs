using Mdaresna.Doamin.Models.SettingsManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsConfigrations.SettingsManagement
{
    public class SMSProviderConfig : IEntityTypeConfiguration<SMSProvider>
    {
        public void Configure(EntityTypeBuilder<SMSProvider> builder)
        {
            //builder
            //    .Property(p => p.CreateDate)
            //    .HasDefaultValue(DateTime.Now);

            //builder
            //    .Property(p => p.LastModifyDate)
            //    .HasDefaultValue(DateTime.Now);
        }
    }
}
