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
    public class SchoolGradeConfig : IEntityTypeConfiguration<SchoolGrade>
    {
        public void Configure(EntityTypeBuilder<SchoolGrade> builder)
        {
            builder
                .HasOne(e => e.School)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
