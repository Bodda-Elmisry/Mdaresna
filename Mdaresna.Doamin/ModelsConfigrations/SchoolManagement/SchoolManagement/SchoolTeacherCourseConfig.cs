using Mdaresna.Doamin.Models.Identity;
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
    public class SchoolTeacherCourseConfig : IEntityTypeConfiguration<SchoolTeacherCourse>
    {
        public void Configure(EntityTypeBuilder<SchoolTeacherCourse> builder)
        {
            builder.HasKey(e => new { e.SchoolId, e.TeacherId, e.CourseId });

            builder
                .HasOne(e => e.School)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Teacher)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Course)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

    //        builder
    //.Property(p => p.CreateDate)
    //.HasDefaultValue(DateTime.Now);

    //        builder
    //            .Property(p => p.LastModifyDate)
    //            .HasDefaultValue(DateTime.Now);
        }
    }
}
