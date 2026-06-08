using Mdaresna.Infrastructure.Data;

namespace Mdarens.ReportingWorker.Factories;

public interface ISchoolDbContextFactory
{
    AppDbContext CreateDbContext(string connectionString);
}
