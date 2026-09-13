using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Mdaresna.Platform.Application.Billing;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Billing.Units;

namespace Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;

internal sealed class PlatformBillingUnitOfWork(PlatformDbContext dbContext) : IPlatformBillingUnitOfWork
{
    public Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        // A retry must re-read the payment and re-stage its ledger, grant and outbox from
        // scratch. Saving these in two ordered batches within one transaction satisfies
        // the SQL approval triggers without exposing a partially approved payment.
        var strategy = dbContext.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            cancellationToken.ThrowIfCancellationRequested();
            dbContext.ChangeTracker.Clear();
            await using var transaction = await dbContext.Database
                .BeginTransactionAsync(cancellationToken);
            var result = await operation(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return result;
        });
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // EF Core uses one relational transaction for all tracked changes in this SaveChanges call.
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception) when (
            exception.Entries.Any(entry => entry.Entity is PlatformPaymentRequest))
        {
            throw new PlatformConflictException(
                "platform_payment.concurrent_review",
                "Platform payment request was changed by another operation.");
        }
        catch (DbUpdateConcurrencyException exception) when (
            exception.Entries.Any(entry => entry.Entity is UnitType))
        {
            throw new PlatformConflictException(
                "unit_type.concurrent_change",
                "Unit type was changed by another operation.");
        }
        catch (DbUpdateException updateException) when (
            updateException.Entries.Any(entry => entry.Entity is UnitGrant) &&
            updateException.InnerException is SqlException sqlException &&
            sqlException.Number is 2601 or 2627)
        {
            throw new PlatformConflictException(
                "unit_grant.duplicate",
                "Units have already been granted for this payment request.");
        }
        catch (DbUpdateException updateException) when (
            updateException.Entries.Any(entry => entry.Entity is UnitPurchaseIntent) &&
            updateException.InnerException is SqlException sqlException &&
            sqlException.Number is 2601 or 2627)
        {
            throw new PlatformConflictException(
                "unit_purchase.duplicate",
                "A unit purchase already exists for this payment request.");
        }
        catch (DbUpdateException updateException) when (
            updateException.Entries.Any(entry => entry.Entity is UnitType) &&
            updateException.InnerException is SqlException sqlException &&
            sqlException.Number is 2601 or 2627)
        {
            throw new PlatformConflictException(
                "unit_type.duplicate_code",
                "A unit type with this code or ID already exists.");
        }
        catch (DbUpdateException updateException) when (
            updateException.Entries.Any(entry =>
                entry.Entity is PlatformPaymentRequest or PlatformPaymentLedgerEntry) &&
            updateException.InnerException is SqlException sqlException &&
            sqlException.Number is 2601 or 2627)
        {
            throw new PlatformConflictException(
                "platform_payment.duplicate_request_or_transfer",
                "Payment request or transfer reference already exists.");
        }
    }
}
