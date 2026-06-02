using Mdaresna.Doamin.MainDB.Models;
using Mdaresna.Repository.MainDB.IRepositories;

namespace Mdaresna.Repository.MainDB.IUnitOfWorks;

public interface IMainUnitOfWork
{
    IGenericRepository<MdaresnaSchool> MdaresnaSchool { get; }
    IGenericRepository<MdaresnaService> MdaresnaService { get; }
    IGenericRepository<MdaresnaSchoolService> MdaresnaSchoolService { get; }
    IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class;
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
