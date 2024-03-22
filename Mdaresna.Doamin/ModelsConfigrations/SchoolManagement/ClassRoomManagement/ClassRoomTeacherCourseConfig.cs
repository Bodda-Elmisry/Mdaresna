using Mdaresna.Doamin.Models.SchoolManagement.ClassRoomManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsConfigrations.SchoolManagement.ClassRoomManagement
{
    public class ClassRoomTeacherCourseConfig : IEntityTypeConfiguration<ClassRoomTeacherCourse>
    {
        public void Configure(EntityTypeBuilder<ClassRoomTeacherCourse> builder)
        {
            builder.HasKey(e=> new {e.TeacherId, e.CourseId, e.ClassRoomId});

            builder
                .HasOne(e => e.Course)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);


            builder
                .HasOne(e => e.Teacher)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);


            builder
                .HasOne(e => e.ClassRoom)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
