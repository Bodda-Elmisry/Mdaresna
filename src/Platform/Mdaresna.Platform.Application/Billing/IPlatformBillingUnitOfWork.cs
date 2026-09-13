namespace Mdaresna.Platform.Application.Billing;

/// <summary>Coordinates billing writes. A single SaveChanges is atomic; staged review writes use an explicit transaction.</summary>
public interface IPlatformBillingUnitOfWork
{
    Task SaveChangesAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a complete business operation within one transaction. The callback must
    /// load its aggregates inside the callback and may call SaveChangesAsync more than once.
    /// It must not perform irreversible external work because a transient SQL failure may
    /// cause the callback to run again.
    /// </summary>
    Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default);
}
