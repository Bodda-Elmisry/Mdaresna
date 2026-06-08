using Mdaresna.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Mdarens.ReportingWorker.Factories;

public class SchoolDbContextFactory : ISchoolDbContextFactory
{
    public AppDbContext CreateDbContext(string connectionString)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
