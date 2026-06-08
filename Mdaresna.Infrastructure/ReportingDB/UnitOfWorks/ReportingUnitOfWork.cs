using Mdaresna.Infrastructure.Data;
using Mdaresna.Infrastructure.ReportingDB.Repositories;
using Mdaresna.Repository.ReportingDB.IRepositories;
using Mdaresna.Repository.ReportingDB.IUnitOfWorks;
using Microsoft.EntityFrameworkCore.Storage;

namespace Mdaresna.Infrastructure.ReportingDB.UnitOfWorks;

internal sealed class ReportingUnitOfWork : IReportingUnitOfWork
{
    private readonly SchoolReportDBContext context;
    private readonly Dictionary<Type, object> repositories = new();
    private IDbContextTransaction? currentTransaction;

    public ReportingUnitOfWork(SchoolReportDBContext context)
    {
        this.context = context;
    }

    public IReportingGenericRepository<TEntity> Repository<TEntity>() where TEntity : class
    {
        var entityType = typeof(TEntity);
        if (repositories.TryGetValue(entityType, out var repository))
        {
            return (IReportingGenericRepository<TEntity>)repository;
        }

        var createdRepository = new ReportingGenericRepository<TEntity>(context);
        repositories[entityType] = createdRepository;
        return createdRepository;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (currentTransaction is not null)
        {
            throw new InvalidOperationException("A transaction is already active for this unit of work.");
        }

        currentTransaction = await context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = EnsureCurrentTransaction();

        try
        {
            await context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
        }
        finally
        {
            await transaction.DisposeAsync();
            currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        var transaction = EnsureCurrentTransaction();

        try
        {
            await transaction.RollbackAsync(cancellationToken);
            context.ChangeTracker.Clear();
        }
        finally
        {
            await transaction.DisposeAsync();
            currentTransaction = null;
        }
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return context.SaveChangesAsync(cancellationToken);
    }

    public async ValueTask DisposeAsync()
    {
        if (currentTransaction is not null)
        {
            await currentTransaction.DisposeAsync();
            currentTransaction = null;
        }

        await context.DisposeAsync();
    }

    private IDbContextTransaction EnsureCurrentTransaction()
    {
        return currentTransaction
            ?? throw new InvalidOperationException("No active transaction was started for this unit of work.");
    }
}
