using Mdaresna.Infrastructure.Data;
using Mdaresna.Repository.MainDB.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Infrastructure.MainDB.Repositories;

internal sealed class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    private readonly AppMainDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(AppMainDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<TEntity>();
    }

    public IQueryable<TEntity> Query() => _dbSet.AsQueryable();

    public async Task<TEntity?> GetByIdAsync(CancellationToken cancellationToken = default, params object[] keyValues)
    {
        return await _dbSet.FindAsync(keyValues, cancellationToken);
    }

    public async Task<IReadOnlyList<TEntity>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbSet.ToListAsync(cancellationToken);
    }

    public async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await _dbSet.AddAsync(entity, cancellationToken);
    }

    public void Update(TEntity entity)
    {
        _dbSet.Update(entity);
    }

    public void Remove(TEntity entity)
    {
        _dbSet.Remove(entity);
    }
}
