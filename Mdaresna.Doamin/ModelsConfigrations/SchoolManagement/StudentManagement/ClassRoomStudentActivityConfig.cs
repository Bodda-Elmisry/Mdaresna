using Mdaresna.Doamin.Models.SchoolManagement.StudentManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsConfigrations.SchoolManagement.StudentManagement
{
    public class ClassRoomStudentActivityConfig : IEntityTypeConfiguration<ClassRoomStudentActivity>
    {
        public void Configure(EntityTypeBuilder<ClassRoomStudentActivity> builder)
        {
            builder
                .HasKey(e => new { e.StudentId, e.ActivityId });

            builder
                .Property(e => e.Result)
                .HasColumnType("decimal(18,2)");

            builder
                .HasOne(e => e.Student)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Activity)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            //builder
            //    .Property(p => p.CreateDate)
            //    .HasDefaultValue(DateTime.Now);

            //builder
            //    .Property(p => p.LastModifyDate)
            //    .HasDefaultValue(DateTime.Now);
        }
    }
}
