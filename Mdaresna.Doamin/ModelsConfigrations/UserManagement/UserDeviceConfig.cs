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
            builder.HasOne(e => e.User)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
