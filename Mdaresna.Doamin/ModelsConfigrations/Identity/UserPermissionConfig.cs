using Mdaresna.Doamin.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsConfigrations.Identity
{
    public class UserPermissionConfig : IEntityTypeConfiguration<UserPermission>
    {
        public void Configure(EntityTypeBuilder<UserPermission> builder)
        {
            builder.HasKey(r => new { r.UserId, r.PermissionId, r.SchoolId });

            builder
                .HasOne(p => p.School)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            //builder
            //   .Property(p => p.CreateDate)
            //   .HasDefaultValue(DateTime.Now);

            //builder
            //    .Property(p => p.LastModifyDate)
            //    .HasDefaultValue(DateTime.Now);
        }
    }
}
