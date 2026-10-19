using Mdaresna.Platform.Application.Billing.Subscriptions;
using Mdaresna.Platform.Application.Billing.ReviewSchoolPlatformPayment;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Billing.Subscriptions;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Infrastructure.Messaging;
using Mdaresna.Platform.Infrastructure.Persistence;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;
using Mdaresna.SharedKernel.Time;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.UnitTests.Billing;

public sealed class SubscriptionRelationalTests
{
    [SubscriptionDatabaseTheory]
    [InlineData("PostgreSql")]
    [InlineData("SqlServer")]
    public async Task Migrated_catalog_reorders_and_payment_subscription_outbox_commit_or_rollback_together(string provider)
    {
        var database = "mdaresna_contract_qa_" + Guid.NewGuid().ToString("N");
        var source = Environment.GetEnvironmentVariable(provider == "PostgreSql" ? "MDARESNA_SUBSCRIPTIONS_PG_QA" : "MDARESNA_SUBSCRIPTIONS_SQL_QA")!;
        string connection;
        if (provider == "PostgreSql") { var b = new Npgsql.NpgsqlConnectionStringBuilder(source) { Database = database, Pooling = false }; connection = b.ConnectionString; }
        else { var b = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(source) { InitialCatalog = database, Pooling = false }; connection = b.ConnectionString; }
        PlatformDbContext Create() => provider == "PostgreSql"
            ? new PostgreSqlPlatformDbContext(new DbContextOptionsBuilder<PostgreSqlPlatformDbContext>().UseNpgsql(connection).Options)
            : new PlatformDbContext(new DbContextOptionsBuilder<PlatformDbContext>().UseSqlServer(connection).Options);
        await using var db = Create();
        try
        {
            await db.Database.MigrateAsync();
            var now = DateTimeOffset.UtcNow;
            var actor = new CatalogActor(IdentityAccountId.New(), Guid.NewGuid());
            var school = Guid.NewGuid(); var tenant = TenantId.From(school);
            db.Tenants.Add(Tenant.Create(tenant, "Contract QA", null, now));
            db.Schools.Add(SchoolRegistration.Create(SchoolId.From(school), tenant, Guid.NewGuid(), SchoolCode.Create("QA-" + school.ToString("N")[..12].ToUpperInvariant()), "Contract QA", SchoolType.Private, DeploymentMode.SharedSaaS, actor.AccountId.Value, now));
            await db.SaveChangesAsync();
            var catalog = new SystemCatalogService(db);
            var system = Guid.NewGuid();
            await catalog.CreateSystemAsync(system, "ACADEMIC", CapacityMetric.Student, new("أكاديمي", "Academic", ["academic.students"], true, 0), actor, default);
            var plans = new List<SystemPlanItem>();
            for (var i = 1; i <= 2; i++)
            {
                var parent = (await catalog.ListSystemsAsync(default)).Single();
                var plan = await catalog.CreatePlanAsync(system, new(Guid.NewGuid(), "PLAN-" + i, "باقة", "Plan", i, parent.Version, new(100 * i, 100, 150, "EGP")), actor, default);
                parent = (await catalog.ListSystemsAsync(default)).Single();
                plans.Add(await catalog.PublishAsync(plan.Id, 1, plan.Version, parent.Version, actor, default));
            }
            await catalog.ReorderAsync(system, (await catalog.ListSystemsAsync(default)).Single().Version, [plans[1].Id, plans[0].Id], actor, default);
            Assert.Equal(plans[1].Id, (await catalog.ListPlansAsync(system, default)).First().Id);
            var uow = new PlatformBillingUnitOfWork(db);
            var service = new SubscriptionService(db, uow);
            var quote = await service.QuoteAsync(new(Guid.NewGuid(), school, plans[0].Id, SubscriptionChangeKind.New, 0, 0, DateTimeOffset.UtcNow), actor, default);
            var paymentId = Guid.NewGuid();
            await service.SubmitAsync(quote.Id, new(paymentId, "QA-" + Guid.NewGuid().ToString("N")), actor, default);
            var payments = new PlatformPaymentRequestRepository(db);
            var handler = new ReviewSchoolPlatformPaymentCommandHandler(payments, new PlatformBillingAuditWriter(db), new PlatformOutboxWriter(db), uow,
                SystemClock.Instance, new UnitPurchaseRepository(db), new UnitCommerceAuditWriter(db), service);
            await handler.HandleAsync(new(paymentId, PlatformPaymentReviewDecision.Approve, IdentityAccountId.New(), null, Guid.NewGuid()));
            Assert.Single(await db.SchoolSubscriptions.ToArrayAsync());
            Assert.Single(await db.PlatformPaymentLedgerEntries.ToArrayAsync());
            Assert.Single(await db.OutboxMessages.Where(x => x.MessageType == "mdaresna.schools.subscription.snapshot-changed").ToArrayAsync());
            var subscription = await db.SchoolSubscriptions.AsNoTracking().SingleAsync();
            await service.RecordAppliedAsync(school, system, subscription.Id, 1, DateTimeOffset.UtcNow, default);
            await service.RecordAppliedAsync(school, system, subscription.Id, 1, DateTimeOffset.UtcNow, default);
            Assert.Equal(1, (await service.DeliveryAsync(subscription.Id, default))!.AppliedRevision);
            await Assert.ThrowsAsync<ArgumentException>(() => service.RecordAppliedAsync(Guid.NewGuid(), system, subscription.Id, 1, DateTimeOffset.UtcNow, default));
            await Assert.ThrowsAsync<ArgumentException>(() => service.RecordAppliedAsync(school, system, subscription.Id, 2, DateTimeOffset.UtcNow, default));
            var extra = await service.QuoteAsync(new(Guid.NewGuid(), school, subscription.PlanId, SubscriptionChangeKind.AddOn, subscription.Version, 2, subscription.ValidFromUtc), actor, default);
            var extraPayment = Guid.NewGuid();
            await service.SubmitAsync(extra.Id, new(extraPayment, "QA-" + Guid.NewGuid().ToString("N")), actor, default);
            await Assert.ThrowsAsync<RollbackProbe>(() => uow.ExecuteInTransactionAsync<int>(async ct => {
                var payment = await db.PlatformPaymentRequests.SingleAsync(x => x.Id == extraPayment, ct);
                payment.Review(PlatformPaymentReviewDecision.Approve, IdentityAccountId.New(), DateTimeOffset.UtcNow, null);
                await uow.SaveChangesAsync(ct);
                db.PlatformPaymentLedgerEntries.Add(PlatformPaymentLedgerEntry.FromApprovedRequest(payment));
                await service.ApplyReviewAsync(payment, actor, ct);
                await uow.SaveChangesAsync(ct);
                throw new RollbackProbe();
            }));
            await using var verify = Create();
            Assert.Equal(PlatformPaymentStatus.Pending, (await verify.PlatformPaymentRequests.SingleAsync(x => x.Id == extraPayment)).Status);
            Assert.Equal(0, (await verify.SchoolSubscriptions.SingleAsync()).AddOnCapacity);
            Assert.Equal(1, await verify.PlatformPaymentLedgerEntries.CountAsync());
            Assert.Equal(1, await verify.OutboxMessages.CountAsync(x => x.MessageType == "mdaresna.schools.subscription.snapshot-changed"));
            await service.ResynchronizeAsync(subscription.Id, subscription.Version, actor, default);
            Assert.Equal(subscription.Version, (await verify.SchoolSubscriptions.AsNoTracking().SingleAsync()).Version);
            Assert.Equal(2, await verify.OutboxMessages.CountAsync(x => x.MessageType == "mdaresna.schools.subscription.snapshot-changed"));
            Assert.Equal(1, (await service.DeliveryAsync(subscription.Id, default))!.AppliedRevision);
        }
        finally
        {
            if (!database.StartsWith("mdaresna_contract_qa_", StringComparison.Ordinal)) throw new InvalidOperationException("Invalid QA database.");
            await db.Database.EnsureDeletedAsync();
        }
    }
    private sealed class RollbackProbe : Exception;
}
public sealed class SubscriptionDatabaseTheoryAttribute : TheoryAttribute
{
    public SubscriptionDatabaseTheoryAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MDARESNA_SUBSCRIPTIONS_PG_QA")) || string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("MDARESNA_SUBSCRIPTIONS_SQL_QA")))
            Skip = "Requires isolated opt-in PostgreSQL and SQL Server QA endpoints.";
    }
}
