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
    public class PermissionSeed : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.HasData(
                new Permission
                {
                    Id = Guid.Parse("0225DBD5-9675-438C-87F2-63FB6921841C"),
                    Name = "AssignScoolManagerToUser",
                    Description = "Assign School Manager Permission To User",
                    SchoolPermission = false,
                    AppPermission = true
                }
                );
        }
    }
}
