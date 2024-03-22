using Mdaresna.Doamin.Models.CoinsManagement;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mdaresna.Doamin.ModelsConfigrations.CoinsManagement
{
    public class CoinTypeConfig : IEntityTypeConfiguration<CoinType>
    {
        public void Configure(EntityTypeBuilder<CoinType> builder)
        {
            builder
                .Property(p => p.Value)
                .HasColumnType("decimal(18,2)");
        }
    }
}
