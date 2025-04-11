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
    public class ClassroomEmployeeConfig : IEntityTypeConfiguration<ClassroomEmployee>
    {
        public void Configure(EntityTypeBuilder<ClassroomEmployee> builder)
        {
            builder.HasKey(e => new { e.EmployeeId, e.ClassRoomId });

            builder
                .HasOne(e => e.Employee)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);


            builder
                .HasOne(e => e.ClassRoom)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
