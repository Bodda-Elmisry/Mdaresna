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
                    Key = "AssignScoolManagerToUser",
                    Name = "Assign Scool Manager To User",
                    Description = "Assign School Manager Permission To User",
                    SchoolPermission = false,
                    AppPermission = true
                },
                new Permission
                {
                    Id = Guid.Parse("9301FC37-AE75-4EF6-B6FA-A656452E5A2E"),
                    Key = "CreateSchool",
                    Name = "Add School",
                    Description = "Create new shool",
                    SchoolPermission = true,
                    AppPermission = false
                },
                new Permission
                {
                    Id = Guid.Parse("219007EA-620E-4D96-8292-2D015EF68DB1"),
                    Key = "ViewChildernsList",
                    Name = "View Childerns",
                    Description = "View Childerns List",
                    SchoolPermission = false,
                    AppPermission = true
                }
                );
        }
    }
}
