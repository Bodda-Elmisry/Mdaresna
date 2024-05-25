using Mdaresna.Doamin.Models.CoinsManagement;
using Mdaresna.Doamin.Models.SchoolManagement.SchoolManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsSeeding.CoinsManagement
{
    public class CoinTypeSeed : IEntityTypeConfiguration<CoinType>
    {
        public void Configure(EntityTypeBuilder<CoinType> builder)
        {
            builder.HasData(
                new CoinType
                {
                    Id = Guid.Parse("AA619624-C134-4E20-867B-E635A5A3B609"),
                    Name = "Standerd",
                    Value = 1
                }
                );
            

        }
    }
}
