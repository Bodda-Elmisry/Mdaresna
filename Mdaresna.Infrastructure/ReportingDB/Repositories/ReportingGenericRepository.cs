using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.ReportingDB.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Infrastructure.ReportingDB.Repositories;

internal sealed class ReportingGenericRepository<TEntity> : IReportingGenericRepository<TEntity>
    where TEntity : class
{
    private readonly DbSet<TEntity> dbSet;

    public ReportingGenericRepository(SchoolReportDBContext context)
    {
        dbSet = context.Set<TEntity>();
    }

    public IQueryable<TEntity> Query()
    {
        return dbSet.AsQueryable();
    }

    public async Task<TEntity?> GetByIdAsync(
        CancellationToken cancellationToken = default,
        params object[] keyValues)
    {
        return await dbSet.FindAsync(keyValues, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await dbSet.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        dbSet.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        dbSet.Remove(entity);
    }
}
