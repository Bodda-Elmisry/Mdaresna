using Mdaresna.Doamin.Models.AdminManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsSeeding.AdminManagement
{
    public class LanguageSeed : IEntityTypeConfiguration<Language>
    {
        public void Configure(EntityTypeBuilder<Language> builder)
        {
            builder.HasData(
                new Language
                {
                    Id = Guid.Parse("7FF35951-0AD2-46C7-83A5-F4487365EC1C"),
                    Name = "Arabic",
                    Description = "For arabic class rooms",
                    CreateDate = new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local),
                    LastModifyDate = new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local)
                },
                new Language
                {
                    Id = Guid.Parse("42F73D52-5E31-485A-8F5B-6F53670447CA"),
                    Name = "English",
                    Description = "For english class rooms",
                    CreateDate = new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local),
                    LastModifyDate = new DateTime(2024, 6, 4, 23, 0, 54, 602, DateTimeKind.Local)
                }
                );
        }
    }
}
