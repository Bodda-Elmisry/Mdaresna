using Mdaresna.Doamin.Models.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsConfigrations.UserManagement
{
    public class UserDeviceConfig : IEntityTypeConfiguration<UserDevice>
    {
        public void Configure(EntityTypeBuilder<UserDevice> builder)
        {
            // SQL Server cannot index nvarchar(max). Device identifiers and FCM
            // registration tokens are bounded so their equality lookups do not scan
            // the entire UserDevices table.
            builder.Property(device => device.DeviceId)
                .HasMaxLength(450);

            builder.Property(device => device.FcmToken)
                .HasMaxLength(440);

            builder.HasIndex(device => device.DeviceId)
                .HasDatabaseName("IX_UserDevices_DeviceId");

            builder.HasIndex(device => device.FcmToken)
                .HasDatabaseName("IX_UserDevices_FcmToken");

            builder.HasIndex(device => new { device.UserId, device.FcmToken })
                .HasDatabaseName("IX_UserDevices_UserId_FcmToken");

            builder.HasOne(e => e.User)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
