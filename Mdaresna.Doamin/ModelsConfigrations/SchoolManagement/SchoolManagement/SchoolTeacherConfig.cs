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
    public class SchoolTeacherConfig : IEntityTypeConfiguration<SchoolTeacher>
    {
        public void Configure(EntityTypeBuilder<SchoolTeacher> builder)
        {
            builder.HasKey(s => new { s.SchoolId, s.TeacherId });

            builder
                .HasOne(s => s.School)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(s => s.Teacher)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
