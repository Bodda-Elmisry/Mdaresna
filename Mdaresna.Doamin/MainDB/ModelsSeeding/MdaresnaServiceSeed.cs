using Mdaresna.Doamin.MainDB.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mdaresna.Doamin.MainDB.ModelsSeeding;

public class MdaresnaServiceSeed : IEntityTypeConfiguration<MdaresnaService>
{
    public void Configure(EntityTypeBuilder<MdaresnaService> builder)
    {
        builder.HasData(
            new MdaresnaService
            {
                Id = Guid.Parse("8488E63B-FD78-43BF-800B-03412C372DB5"),
                Name = "ReportingService",
                IsActive = true,
                DBType = Enums.DBTypeEnum.PostGreSQL,
                DBSource = "localhost",
                DBPort = "5432",
                DBUser = "postgres",
                Deleted = false,
                CreateDate = new DateTime(2026,5,26)
            });
    }
}
