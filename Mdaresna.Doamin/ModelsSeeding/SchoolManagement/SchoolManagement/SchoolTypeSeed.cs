using Mdaresna.Doamin.Models.Identity;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsSeeding.SchoolManagement.SchoolManagement
{
    public class SchoolTypeSeed : IEntityTypeConfiguration<SchoolType>
    {
        public void Configure(EntityTypeBuilder<SchoolType> builder)
        {
            builder.HasData(
                new SchoolType
                {
                    Id = Guid.Parse("04D490AD-0994-404E-9D8B-8ECF504811F3"),
                    Name = "Standerd",
                    Description = "Standerd Schools"
                }
                );
        }
    }
}
