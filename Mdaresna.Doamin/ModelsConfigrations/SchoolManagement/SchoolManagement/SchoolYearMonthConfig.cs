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
    public class SchoolYearMonthConfig : IEntityTypeConfiguration<SchoolYearMonth>
    {
        public void Configure(EntityTypeBuilder<SchoolYearMonth> builder)
        {
            builder
                .HasOne(e => e.Year)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
