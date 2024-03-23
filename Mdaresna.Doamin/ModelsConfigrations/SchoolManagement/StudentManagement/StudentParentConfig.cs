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
    public class StudentParentConfig : IEntityTypeConfiguration<StudentParent>
    {
        public void Configure(EntityTypeBuilder<StudentParent> builder)
        {

            builder
                .HasKey(e=> new {e.StudentId, e.ParentId});

            builder
                .HasOne(e => e.Student)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Parent)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(e => e.Relation)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
