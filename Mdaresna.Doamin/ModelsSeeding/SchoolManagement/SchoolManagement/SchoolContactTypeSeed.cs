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
    public class SchoolContactTypeSeed : IEntityTypeConfiguration<SchoolContactType>
    {
        public void Configure(EntityTypeBuilder<SchoolContactType> builder)
        {
            builder.HasData(
                new SchoolContactType
                {
                    Id = Guid.Parse("5160B1A7-B5FF-4807-A3E0-94FD99579407"),
                    Name = "Email"
                    
                },
                new SchoolContactType
                {
                    Id = Guid.Parse("04415888-FE5C-4C91-A8AA-B6A8D1383C08"),
                    Name = "Mobile"
                },
                new SchoolContactType
                {
                    Id = Guid.Parse("7B962CC1-DB7B-489F-B75E-A478FB932E00"),
                    Name = "Phone"
                },
                new SchoolContactType
                {
                    Id = Guid.Parse("A3FDCFA4-0C57-416B-91A9-51E8601E7D0C"),
                    Name = "Fax"
                },
                new SchoolContactType
                {
                    Id = Guid.Parse("3851E877-81EC-4E74-A9EE-AB29265E873F"),
                    Name = "Address"
                }
                );
        }
    }
}
