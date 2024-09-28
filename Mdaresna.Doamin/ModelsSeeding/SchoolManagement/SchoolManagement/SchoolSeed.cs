using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsSeeding.SchoolManagement.SchoolManagement
{
    public class SchoolSeed : IEntityTypeConfiguration<School>
    {
        public void Configure(EntityTypeBuilder<School> builder)
        {
            builder.HasData(
                new School
                {
                    Id = Guid.Parse("3338E17D-280F-467F-938A-5629415B6E52"),
                    About = "",
                    Vesion = "",
                    Active = true,
                    ImageUrl = "",
                    SchoolTypeId = Guid.Parse("04D490AD-0994-404E-9D8B-8ECF504811F3"),
                    CoinTypeId = Guid.Parse("AA619624-C134-4E20-867B-E635A5A3B609"),
                    AvailableCoins = 100,
                    SchoolAdminId = Guid.Parse("d4c7be15-c9b6-4d83-8516-aff52c94f963"),
                    Name = "Demo School",
                    CreateDate = DateTime.Parse("2024-09-27 21:06:00.2529751"),
                    LastModifyDate = DateTime.Parse("2024-09-27 21:06:00.2529751")
                }
                );
        }
    }
}
