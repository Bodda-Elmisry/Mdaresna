using Mdaresna.Doamin.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsConfigrations.Identity
{
    public class UserPermissionSchoolClassRoomConfig : IEntityTypeConfiguration<UserPermissionSchoolClassRoom>
    {
        public void Configure(EntityTypeBuilder<UserPermissionSchoolClassRoom> builder)
        {
            builder.HasKey(r => new { r.UserId, r.PermissionId, r.ClassRoomId });

            //builder
            //   .Property(p => p.CreateDate)
            //   .HasDefaultValue(DateTime.Now);

            //builder
            //    .Property(p => p.LastModifyDate)
            //    .HasDefaultValue(DateTime.Now);
        }
    }
}
