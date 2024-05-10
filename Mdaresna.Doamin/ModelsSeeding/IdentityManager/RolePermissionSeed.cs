using Mdaresna.Doamin.Models.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsSeeding.IdentityManager
{
    public class RolePermissionSeed : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.HasData(
                new RolePermission
                {
                    PermissionId = Guid.Parse("219007EA-620E-4D96-8292-2D015EF68DB1"),
                    RoleId = Guid.Parse("92D00B28-9D25-4BD2-A587-6C22A3A07A92")
                },
                new RolePermission
                {
                    PermissionId = Guid.Parse("9301FC37-AE75-4EF6-B6FA-A656452E5A2E"),
                    RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73")
                }
                );
        }
    }
}
