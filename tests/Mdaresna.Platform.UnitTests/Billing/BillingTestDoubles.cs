using Mdaresna.Platform.Application.Billing;
using Mdaresna.Platform.Application.Billing.Units;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Billing.Units;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Billing;

internal sealed class FakePaymentRepository : IPlatformPaymentRequestRepository
{
    public List<PlatformPaymentRequest> Requests { get; } = [];
    public List<PlatformPaymentLedgerEntry> LedgerEntries { get; } = [];

    public Task<PlatformPaymentRequest?> FindByIdAsync(
        Guid requestId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Requests.SingleOrDefault(x => x.Id == requestId));

    public Task<PlatformPaymentRequest?> FindForReviewAsync(
        Guid requestId,
        CancellationToken cancellationToken = default) =>
        FindByIdAsync(requestId, cancellationToken);

    public Task<bool> TransferReferenceExistsAsync(
        SchoolId schoolId,
        string transferReference,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Requests.Any(x =>
            x.SchoolId == schoolId && x.TransferReference == transferReference));

    public Task AddAsync(
        PlatformPaymentRequest request,
        CancellationToken cancellationToken = default)
    {
        Requests.Add(request);
        return Task.CompletedTask;
    }

    public void AddLedgerEntry(PlatformPaymentLedgerEntry entry) => LedgerEntries.Add(entry);
}

internal sealed class FakeBillingAuditWriter : IPlatformBillingAuditWriter
{
    public List<(string Action, Guid RequestId)> Actions { get; } = [];

    public void RecordSubmitted(PlatformPaymentRequest request, string? correlationId) =>
        Actions.Add(("submitted", request.Id));

    public void RecordReviewed(PlatformPaymentRequest request, string? correlationId) =>
        Actions.Add((request.Status.ToString(), request.Id));
}

internal sealed class FakeBillingUnitOfWork(Exception? saveException = null) : IPlatformBillingUnitOfWork
{
    public int SaveCount { get; private set; }

    public Task<TResult> ExecuteInTransactionAsync<TResult>(
        Func<CancellationToken, Task<TResult>> operation,
        CancellationToken cancellationToken = default) => operation(cancellationToken);

    public Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveCount++;
        if (saveException is not null)
        {
            throw saveException;
        }

        return Task.CompletedTask;
    }
}

internal sealed class FakeUnitPurchaseRepository : IUnitPurchaseRepository
{
    public List<UnitPurchaseIntent> Intents { get; } = [];
    public List<UnitGrant> Grants { get; } = [];

    public Task<UnitPurchaseIntent?> FindIntentAsync(
        Guid paymentRequestId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Intents.SingleOrDefault(x => x.PaymentRequestId == paymentRequestId));

    public Task AddIntentAsync(UnitPurchaseIntent intent, CancellationToken cancellationToken = default)
    {
        Intents.Add(intent);
        return Task.CompletedTask;
    }

    public Task<UnitGrant?> FindGrantAsync(
        Guid paymentRequestId,
        CancellationToken cancellationToken = default) =>
        Task.FromResult(Grants.SingleOrDefault(x => x.PaymentRequestId == paymentRequestId));

    public void AddGrant(UnitGrant grant) => Grants.Add(grant);
}

internal sealed class FakeUnitCommerceAuditWriter : IUnitCommerceAuditWriter
{
    public List<UnitCommerceAuditRecord> Records { get; } = [];

    public void Stage(UnitCommerceAuditRecord record) => Records.Add(record);
}
