using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Mdaresna.Infrastructure.Data;

public class SchoolReportDBContextFactory : IDesignTimeDbContextFactory<SchoolReportDBContext>
{
    public SchoolReportDBContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile(Path.Combine("..", "Mdaresna", "appsettings.json"), optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString =
            configuration.GetConnectionString("SchoolReportConnection") ??
            configuration.GetConnectionString("DefaultConnection") ??
            "Server=(localdb)\\mssqllocaldb;Database=MdaresnaSchoolReportDesign;Trusted_Connection=True;TrustServerCertificate=True";

        var options = new DbContextOptionsBuilder<SchoolReportDBContext>()
            .UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions.MigrationsAssembly(typeof(SchoolReportDBContext).Assembly.FullName))
            .Options;

        return new SchoolReportDBContext(options);
    }
}
