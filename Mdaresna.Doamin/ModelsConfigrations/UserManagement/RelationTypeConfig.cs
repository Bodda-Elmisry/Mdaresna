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
    public class RelationTypeConfig : IEntityTypeConfiguration<RelationType>
    {
        public void Configure(EntityTypeBuilder<RelationType> builder)
        {
            //builder
            //    .Property(p => p.CreateDate)
            //    .HasDefaultValue(DateTime.Now);

            //builder
            //    .Property(p => p.LastModifyDate)
            //    .HasDefaultValue(DateTime.Now);
        }
    }
}
