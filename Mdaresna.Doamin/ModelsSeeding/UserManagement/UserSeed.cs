using Mdaresna.Doamin.Enums;
using Mdaresna.Doamin.Models.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsSeeding.UserManagement
{
    public class UserSeed : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasData(
                new User // App Manager
                {
                    Id = Guid.Parse("DE36F342-FE3C-46C3-BDFC-BB3FCF2EC7E4"),
                    UserName = "AliFath",
                    FirstName = "Ali",
                    LastName = "Fath",
                    PhoneNumber = "01210199123",
                    Password = "gNL9jSV6CPO827CIcQlRcgFHuqH5gmfAfn95jWLI9ec=",
                    ImageUrl = "",
                    UserType = UserTypeEnum.ApplicationManager,
                    EmailConfirmed = false,
                    PhoneConfirmed = true,
                    EncriptionKey = "UMvC6sSFY2ujmzh6A1aaK3QHEBS00QUa",
                    CreateDate = DateTime.Parse("2024-09-27 21:06:00.2529751"),
                    LastModifyDate = DateTime.Parse("2024-09-27 21:06:00.2529751")
                },
                new User //School Manager
                {
                    Id = Guid.Parse("d4c7be15-c9b6-4d83-8516-aff52c94f963"),
                    UserName = "Ali",
                    FirstName = "Ali",
                    LastName = "Fath",
                    PhoneNumber = "01288826193",
                    PhoneConfirmed = true,
                    Password = "JA848ibn1deWJVFuHcsLXx7Bxo4NBelM0gF2tu9nwGI=",
                    EncriptionKey = "S4qu2kt4eTBicZKp5inKySLYhcvTZUfu",
                    ImageUrl = "",
                    UserType = UserTypeEnum.Normal,
                    EmailConfirmed = false,
                    CreateDate = DateTime.Parse("2024-09-27 21:06:00.2529751"),
                    LastModifyDate = DateTime.Parse("2024-09-27 21:06:00.2529751")
                }
                );
        }
    }
}
