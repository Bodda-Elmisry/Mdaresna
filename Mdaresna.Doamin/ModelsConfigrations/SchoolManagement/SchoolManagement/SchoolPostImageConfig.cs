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
    public class SchoolPostImageConfig : IEntityTypeConfiguration<SchoolPostImage>
    {
        public void Configure(EntityTypeBuilder<SchoolPostImage> builder)
        {
    //        builder
    //.Property(p => p.CreateDate)
    //.HasDefaultValue(DateTime.Now);

    //        builder
    //            .Property(p => p.LastModifyDate)
    //            .HasDefaultValue(DateTime.Now);
        }
    }
}
