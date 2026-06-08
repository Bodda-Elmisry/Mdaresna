using Mdaresna.Repository.ReportingDB.IRepositories;

namespace Mdaresna.Repository.ReportingDB.IUnitOfWorks;

public interface IReportingUnitOfWork : IAsyncDisposable
{
    IReportingGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;

    Task BeginTransactionAsync(CancellationToken cancellationToken = default);

    Task CommitTransactionAsync(CancellationToken cancellationToken = default);

    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
