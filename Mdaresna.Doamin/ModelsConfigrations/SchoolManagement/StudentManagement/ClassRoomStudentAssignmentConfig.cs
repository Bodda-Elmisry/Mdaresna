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
    public class ClassRoomStudentAssignmentConfig : IEntityTypeConfiguration<ClassRoomStudentAssignment>
    {
        public void Configure(EntityTypeBuilder<ClassRoomStudentAssignment> builder)
        {
            builder
                .HasKey(e => new { e.StudentId, e.AssignmentId });


            builder
                .Property(x => x.Result)
                .HasColumnType("decimal(18,2)");


            builder
                .HasOne(e => e.Student)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Assignment)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
