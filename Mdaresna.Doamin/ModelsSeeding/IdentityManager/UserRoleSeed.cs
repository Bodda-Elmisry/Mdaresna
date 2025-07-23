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
    public class UserRoleSeed : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            //builder.HasData(
            //    new UserRole
            //    {
            //        Id = Guid.Parse("0CFE751F-0939-4E7F-AD0E-A0418637307F"),
            //        UserId = Guid.Parse("DE36F342-FE3C-46C3-BDFC-BB3FCF2EC7E4"),
            //        RoleId = Guid.Parse("228AE7F5-C704-4660-AEB0-0E1F43112AE1"),
            //    }
            //    );

            //builder.HasData(
            //    new UserRole
            //    {
            //        Id = Guid.Parse("9A231321-579E-4AD2-BCBA-45CBC140DA01"),
            //        UserId = Guid.Parse("DE36F342-FE3C-46C3-BDFC-BB3FCF2EC7E4"),
            //        RoleId = Guid.Parse("92D00B28-9D25-4BD2-A587-6C22A3A07A92"),
            //    }
            //    );

            builder.HasData(
                new UserRole
                {
                    Id = Guid.Parse("87C582E3-7FDC-4235-BE0E-2663925BC9EE"),
                    UserId = Guid.Parse("D4C7BE15-C9B6-4D83-8516-AFF52C94F963"),
                    RoleId = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                }
                );

            builder.HasData(
                new UserRole
                {
                    Id = Guid.Parse("A65CBA57-BAEB-42DF-89F6-88EF7E3D2B66"),
                    UserId = Guid.Parse("D4C7BE15-C9B6-4D83-8516-AFF52C94F963"),
                    RoleId = Guid.Parse("92D00B28-9D25-4BD2-A587-6C22A3A07A92"),
                }
                );
        }
    }
}
