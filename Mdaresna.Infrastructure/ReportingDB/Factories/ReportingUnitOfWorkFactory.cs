using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.ReportingDB.UnitOfWorks;
using Mdaresna.Repository.ReportingDB.IFactories;
using Mdaresna.Repository.ReportingDB.IUnitOfWorks;

namespace Mdaresna.Infrastructure.ReportingDB.Factories;

internal sealed class ReportingUnitOfWorkFactory : IReportingUnitOfWorkFactory
{
    public async Task<IReportingUnitOfWork> CreateAsync(
        string connectionString,
        CancellationToken cancellationToken = default)
    {
        var context = await SchoolReportDBContext.CreateAndMigrateAsync(
            connectionString,
            cancellationToken);

        return new ReportingUnitOfWork(context);
    }
}
