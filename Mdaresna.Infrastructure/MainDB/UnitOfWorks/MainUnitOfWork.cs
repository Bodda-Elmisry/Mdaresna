using Mdaresna.Doamin.MainDB.Models;
using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.MainDB.Repositories;
using Mdaresna.Repository.MainDB.IRepositories;
using Mdaresna.Repository.MainDB.IUnitOfWorks;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mdaresna.Infrastructure.MainDB.UnitOfWorks;

internal sealed class MainUnitOfWork : IMainUnitOfWork, IAsyncDisposable
{
    private readonly AppMainDbContext _context;
    private readonly Dictionary<Type, object> _repositories = new();
    private IDbContextTransaction? _currentTransaction;

    public MainUnitOfWork(AppMainDbContext context)
    {
        _context = context;
    }

    public IGenericRepository<MdaresnaSchool> MdaresnaSchool => Repository<MdaresnaSchool>();
    public IGenericRepository<MdaresnaService> MdaresnaService => Repository<MdaresnaService>();
    public IGenericRepository<MdaresnaSchoolService> MdaresnaSchoolService => Repository<MdaresnaSchoolService>();

    public IGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var entityType = typeof(TEntity);
        if (_repositories.TryGetValue(entityType, out var repository))
        {
            return (IGenericRepository<TEntity>)repository;
        }

        var createdRepository = new GenericRepository<TEntity>(_context);
        _repositories[entityType] = createdRepository;
        return createdRepository;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            throw new InvalidOperationException("A transaction is already active for this unit of work.");
        }

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = EnsureCurrentTransaction();

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await transaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = EnsureCurrentTransaction();

        try
        {
            await transaction.RollbackAsync(cancellationToken);
            _context.ChangeTracker.Clear();
        }
        finally
        {
            await transaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return _context.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (_currentTransaction is not null)
        {
            await _currentTransaction.DisposeAsync();
            _currentTransaction = null;
        }
    }

    private IDbContextTransaction EnsureCurrentTransaction()
    {
        return _currentTransaction
            ?? throw new InvalidOperationException("No active transaction was started for this unit of work.");
    }
}
