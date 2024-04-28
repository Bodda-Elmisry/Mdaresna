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
    public class RoleSeed : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.HasData(
                new Role
                {
                    Id = Guid.Parse("228AE7F5-C704-4660-AEB0-0E1F43112AE1"),
                    Name = "Application Owner",
                    Description = "This Role with fuly access to all Application featuers",
                    Active = true,
                    AdminRole = true,
                    SchoolRole = false

                });

            builder.HasData(
                new Role
                {
                    Id = Guid.Parse("6E473A7A-083C-405B-8F0D-04DD85B94EDB"),
                    Name = "Application Manager",
                    Description = "This Role with fuly access to all Application featuers",
                    Active = true,
                    AdminRole = true,
                    SchoolRole = false

                });

            builder.HasData(
                new Role
                {
                    Id = Guid.Parse("4B8A99FE-B759-4C18-9500-8052C3D7AC73"),
                    Name = "School Manager",
                    Description = "This Role with fuly access to all school featuers",
                    Active = true,
                    AdminRole = true,
                    SchoolRole = false

                });

            builder.HasData(
                new Role
                {
                    Id = Guid.Parse("92D00B28-9D25-4BD2-A587-6C22A3A07A92"),
                    Name = "Standerd",
                    Description = "This Role to work in normal account and it's the default role for all accounts. \nWith this role you can track your souns activites and schools.",
                    Active = true,
                    AdminRole = true,
                    SchoolRole = false

                });

        }
    }
}
