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
    public class ClassRoomExamConfig : IEntityTypeConfiguration<ClassRoomExam>
    {
        public void Configure(EntityTypeBuilder<ClassRoomExam> builder)
        {
            builder
                .Property(p => p.Rate)
                .HasColumnType("decimal(18,2)");

            builder
                .HasOne(e=> e.ClassRoom)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Supervisor)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Month)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Course)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            //builder
            //   .Property(p => p.CreateDate)
            //   .HasDefaultValue(DateTime.Now);

            //builder
            //    .Property(p => p.LastModifyDate)
            //    .HasDefaultValue(DateTime.Now);
        }
    }
}
