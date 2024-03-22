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
    public class SchoolPostConfig : IEntityTypeConfiguration<SchoolPost>
    {
        public void Configure(EntityTypeBuilder<SchoolPost> builder)
        {
            builder
                .HasOne(p => p.School)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
