using Mdaresna.Doamin.Models.UserManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsConfigrations.UserManagement
{
    public class SchoolUserConfig : IEntityTypeConfiguration<SchoolUser>
    {
        public void Configure(EntityTypeBuilder<SchoolUser> builder)
        {
            builder.HasKey(s => new { s.UserId, s.SchoolId });

            builder
                .HasOne(p => p.School)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
