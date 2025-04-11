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
    public class SchoolEmployeeConfig : IEntityTypeConfiguration<SchoolEmployee>
    {
        public void Configure(EntityTypeBuilder<SchoolEmployee> builder)
        {
            builder.HasKey(s => new { s.SchoolId, s.EmployeeId });

            builder
                .HasOne(s => s.School)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            builder
                .HasOne(s => s.Employee)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
