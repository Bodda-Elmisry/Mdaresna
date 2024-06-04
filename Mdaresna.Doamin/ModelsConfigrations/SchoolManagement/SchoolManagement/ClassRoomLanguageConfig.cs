using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsConfigrations.SchoolManagement.SchoolManagement
{
    public class ClassRoomLanguageConfig : IEntityTypeConfiguration<ClassRoomLanguage>
    {
        public void Configure(EntityTypeBuilder<ClassRoomLanguage> builder)
        {
            builder.HasKey(m => new {m.SchoolId, m.LanguageId});

            builder.HasOne(m => m.School)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(m => m.Language)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);



        }
    }
}
