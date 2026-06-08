using Mdaresna.Repository.ReportingDB.IUnitOfWorks;

namespace Mdaresna.Repository.ReportingDB.IFactories;

public interface IReportingUnitOfWorkFactory
{
    Task<IReportingUnitOfWork> CreateAsync(
        string connectionString,
        CancellationToken cancellationToken = default);
}
