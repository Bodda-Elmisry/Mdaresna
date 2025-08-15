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
    public class SchoolImageConfig : IEntityTypeConfiguration<SchoolImage>
    {
        public void Configure(EntityTypeBuilder<SchoolImage> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasOne(i => i.School)
                .WithMany(i=> i.SchoolImages)
                .HasForeignKey(i => i.SchoolId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("FK_SchoolImages_Schools_SchoolId");
        }
    }
}
