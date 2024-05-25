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
    public class StudentAttendanceConfig : IEntityTypeConfiguration<StudentAttendance>
    {
        public void Configure(EntityTypeBuilder<StudentAttendance> builder)
        {
            builder
                .HasOne(e => e.Student)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e=> e.ClassRoom)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e=> e.Supervisor)
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
