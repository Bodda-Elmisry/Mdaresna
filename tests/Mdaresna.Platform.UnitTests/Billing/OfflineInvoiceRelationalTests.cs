using Mdaresna.Platform.Application.Billing.Subscriptions;
using Mdaresna.Platform.Application.Billing.ReviewSchoolPlatformPayment;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Billing.Subscriptions;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Infrastructure.Messaging;
using Mdaresna.Platform.Infrastructure.Persistence;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Repositories;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Registry;
using Mdaresna.Schools.Contracts.Registration;
using Mdaresna.SharedKernel.Time;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.UnitTests.Billing;

public sealed class OfflineInvoiceRelationalTests
{
    [SubscriptionDatabaseTheory]
    [InlineData("PostgreSql")]
    [InlineData("SqlServer")]
    public async Task Registration_invoice_freezes_selected_plans_and_requires_full_approved_offline_payment_before_provisioning(string provider)
    {
        var database = "mdaresna_invoice_qa_" + Guid.NewGuid().ToString("N");
        var source = Environment.GetEnvironmentVariable(provider == "PostgreSql" ? "MDARESNA_SUBSCRIPTIONS_PG_QA" : "MDARESNA_SUBSCRIPTIONS_SQL_QA")!;
        string connection;
        if (provider == "PostgreSql") connection = new Npgsql.NpgsqlConnectionStringBuilder(source) { Database = database, Pooling = false }.ConnectionString;
        else connection = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(source) { InitialCatalog = database, Pooling = false }.ConnectionString;
        await using PlatformDbContext db = provider == "PostgreSql"
            ? new PostgreSqlPlatformDbContext(new DbContextOptionsBuilder<PostgreSqlPlatformDbContext>().UseNpgsql(connection).Options)
            : new PlatformDbContext(new DbContextOptionsBuilder<PlatformDbContext>().UseSqlServer(connection).Options);
        try
        {
            await db.Database.MigrateAsync();
            var now = DateTimeOffset.UtcNow; var owner = IdentityAccountId.New(); var recorder = new CatalogActor(IdentityAccountId.New(), Guid.NewGuid());
            var reviewer = IdentityAccountId.New(); var school = Guid.NewGuid(); var tenant = TenantId.From(school);
            var catalog = new SystemCatalogService(db); var selections = new List<RegistrationPlanSelection>();
            foreach (var metric in new[] { CapacityMetric.Student, CapacityMetric.Employee })
            {
                var system = Guid.NewGuid(); var code = metric == CapacityMetric.Student ? "ACADEMIC" : "ADMINISTRATIVE";
                await catalog.CreateSystemAsync(system, code, metric, new(code, code, [metric == CapacityMetric.Student ? "academic.students" : "administration.employees"], true, 0), recorder, default);
                var parent = (await catalog.ListSystemsAsync(default)).Single(x => x.Id == system);
                var plan = await catalog.CreatePlanAsync(system, new(Guid.NewGuid(), code + "-PLAN", "باقة", "Plan", 1, parent.Version,
                    new(metric == CapacityMetric.Student ? 300 : 50, metric == CapacityMetric.Student ? 100 : 50, 125, "EGP")), recorder, default);
                parent = (await catalog.ListSystemsAsync(default)).Single(x => x.Id == system);
                await catalog.PublishAsync(plan.Id, 1, plan.Version, parent.Version, recorder, default); selections.Add(new(plan.Id, 1, metric == CapacityMetric.Student ? 10 : 0));
            }
            var uow = new PlatformBillingUnitOfWork(db); var invoices = new PlatformInvoiceService(db, uow); var subscriptions = new SubscriptionService(db, uow);
            var request = new SchoolRegistrationRequestedV2(school, tenant, "QA-SCHOOL", "Invoice QA", RequestedSchoolTypeV2.Private, "QA address", "01000000000", "Fixture owner", "01000000001", now);
            var offer = await invoices.PrepareRegistrationAsync(new(request, selections.ToArray()), default);
            Assert.Equal(offer, await invoices.PrepareRegistrationAsync(new(request, selections.ToArray()), default));
            await Assert.ThrowsAsync<PlatformConflictException>(() => invoices.PrepareRegistrationAsync(new(request, selections.Select(x => x with { AdditionalQuantity = x.AdditionalQuantity + 1 }).ToArray()), default));
            var studentSystem = (await catalog.ListSystemsAsync(default)).Single(x => x.CapacityMetric == CapacityMetric.Student);
            var studentPlan = (await catalog.ListPlansAsync(studentSystem.Id, default)).Single();
            await catalog.AddRevisionAsync(studentPlan.Id, studentPlan.Version, studentSystem.Version, new(300, 999, 150, "EGP"), recorder, default);
            var transition = new TransitionSchoolCommandHandler(new SchoolRegistrationRepository(db), new UnitTypeRepository(db), new PlatformOutboxWriter(db), new PlatformRegistryAuditWriter(db), new PlatformUnitOfWork(db), SystemClock.Instance, uow, subscriptions);
            var legacyRegistration = new Mdaresna.Platform.Application.Registry.ConsumeSchoolRegistrationRequest.ConsumeSchoolRegistrationRequestV2Handler(
                new FixtureIdentity(owner), new SchoolRegistrationRepository(db),
                new(new TenantRepository(db), new PlatformOutboxWriter(db), new PlatformRegistryAuditWriter(db), new PlatformUnitOfWork(db), SystemClock.Instance),
                new(new TenantRepository(db), new SchoolRegistrationRepository(db), new PlatformOutboxWriter(db), new PlatformRegistryAuditWriter(db), new PlatformUnitOfWork(db), SystemClock.Instance), transition);
            var ingestor = new PlatformSchoolRegistrationRequestV3Ingestor(db, legacyRegistration, invoices);
            var envelope = Mdaresna.IntegrationContracts.Messaging.IntegrationMessageEnvelope<SchoolRegistrationRequestedV3>.Create(
                now, "schools", Mdaresna.IntegrationContracts.Messaging.IntegrationMessageScope.ForTenant(tenant), new(request, offer));
            Assert.True(await ingestor.IngestAsync(envelope));
            Assert.False(await ingestor.IngestAsync(envelope));
            var registration = await db.Schools.AsNoTracking().SingleAsync();
            var invoice = Assert.Single(await invoices.ListAsync(school, 1, default));
            Assert.Equal(33750m, invoice.Amount); Assert.Equal(2, invoice.Lines.Length); Assert.Equal(InvoicePaymentStatus.Unpaid, invoice.PaymentStatus);
            TransitionSchoolCommand Approve() => new(tenant, SchoolId.From(school), SchoolLifecycleAction.Approve, reviewer, registration.Version, Guid.NewGuid());
            var unpaid = await Assert.ThrowsAsync<PlatformConflictException>(() => transition.HandleAsync(Approve())); Assert.Equal("school.subscription_payment_required", unpaid.Code);
            var begin = new Mdaresna.Platform.Application.Registry.BeginSchoolProvisioning.BeginSchoolProvisioningCommandHandler(
                new SchoolRegistrationRepository(db), new UnitTypeRepository(db), new PlatformOutboxWriter(db), new PlatformRegistryAuditWriter(db), new PlatformUnitOfWork(db), SystemClock.Instance, uow, subscriptions);
            await Assert.ThrowsAsync<PlatformConflictException>(() => begin.HandleAsync(new(tenant, SchoolId.From(school), Guid.NewGuid(), reviewer, Guid.NewGuid(), Guid.NewGuid())));
            var partial = new RecordInvoiceReceipt(Guid.NewGuid(), invoice.Version, 100, OfflinePaymentMethod.CashRepresentative, "QA-REC-1", "Fixture representative", now);
            await Assert.ThrowsAsync<ArgumentException>(() => invoices.RecordAsync(invoice.Id, partial, recorder, default));
            await Assert.ThrowsAsync<ArgumentException>(() => invoices.RecordAsync(invoice.Id, partial with { Amount = invoice.Amount, RepresentativeName = null }, recorder, default));
            var cash = partial with { Amount = invoice.Amount };
            await invoices.RecordAsync(invoice.Id, cash, recorder, default); await invoices.RecordAsync(invoice.Id, cash, recorder, default);
            Assert.Equal(InvoicePaymentStatus.PendingReview, (await invoices.ListAsync(school, 1, default)).Single().PaymentStatus);
            await Assert.ThrowsAsync<PlatformConflictException>(() => transition.HandleAsync(Approve()));
            var review = new ReviewSchoolPlatformPaymentCommandHandler(new PlatformPaymentRequestRepository(db), new PlatformBillingAuditWriter(db), new PlatformOutboxWriter(db), uow,
                SystemClock.Instance, new UnitPurchaseRepository(db), new UnitCommerceAuditWriter(db), subscriptions);
            await review.HandleAsync(new(cash.PaymentRequestId, PlatformPaymentReviewDecision.Reject, reviewer, "Fixture rejection", Guid.NewGuid()));
            invoice = (await invoices.ListAsync(school, 1, default)).Single(); Assert.Equal(InvoicePaymentStatus.Rejected, invoice.PaymentStatus);
            var transfer = new RecordInvoiceReceipt(Guid.NewGuid(), invoice.Version, invoice.Amount, OfflinePaymentMethod.BankTransfer, "QA-BANK-1", null, now);
            await invoices.RecordAsync(invoice.Id, transfer, recorder, default);
            await review.HandleAsync(new(transfer.PaymentRequestId, PlatformPaymentReviewDecision.Approve, reviewer, null, Guid.NewGuid()));
            invoice = (await invoices.ListAsync(school, 1, default)).Single(); Assert.Equal(InvoicePaymentStatus.Paid, invoice.PaymentStatus); Assert.Equal(2, invoice.Receipts.Length);
            Assert.Empty(await db.SchoolSubscriptions.AsNoTracking().ToArrayAsync());
            Assert.Equal(33750m, (await db.PlatformPaymentLedgerEntries.AsNoTracking().SingleAsync()).Amount);
            registration = await db.Schools.AsNoTracking().SingleAsync();
            await transition.HandleAsync(Approve());
            Assert.Equal(SchoolLifecycleStatus.Provisioning, (await db.Schools.AsNoTracking().SingleAsync()).Status);
            Assert.Equal(2, await db.SchoolSubscriptions.CountAsync());
            Assert.Equal(2, await db.OutboxMessages.CountAsync(x => x.MessageType == "mdaresna.schools.subscription.snapshot-changed"));
            Assert.Equal(1, await db.OutboxMessages.CountAsync(x => x.MessageType == "mdaresna.schools.provision.v2"));
            Assert.All(await db.SchoolSubscriptions.ToArrayAsync(), x => Assert.True(x.ValidFromUtc >= now));
            var contract = (await subscriptions.ListAsync(school, default)).Single(x => x.SystemId == studentSystem.Id);
            Assert.Equal(10, contract.AddOnCapacity);
            var addOn = await subscriptions.QuoteAsync(new(Guid.NewGuid(), school, contract.PlanId, SubscriptionChangeKind.AddOn, contract.Version, 2, contract.ValidFromUtc), recorder, default);
            var extraInvoiceId = await invoices.IssueQuoteAsync(addOn.Id, recorder, default);
            Assert.Equal(extraInvoiceId, await invoices.IssueQuoteAsync(addOn.Id, recorder, default));
            var extraInvoice = (await invoices.ListAsync(school, 1, default)).Single(x => x.Id == extraInvoiceId);
            Assert.Equal(250m, extraInvoice.Amount);
            var extraCash = new RecordInvoiceReceipt(Guid.NewGuid(), extraInvoice.Version, extraInvoice.Amount, OfflinePaymentMethod.CashRepresentative, "QA-ADDON-CASH", "Fixture representative", now);
            await invoices.RecordAsync(extraInvoiceId, extraCash, recorder, default);
            await review.HandleAsync(new(extraCash.PaymentRequestId, PlatformPaymentReviewDecision.Approve, reviewer, null, Guid.NewGuid()));
            Assert.Equal(12, (await subscriptions.ListAsync(school, default)).Single(x => x.Id == contract.Id).AddOnCapacity);
            Assert.Equal(2, await db.PlatformPaymentLedgerEntries.CountAsync());
        }
        finally
        {
            if (!database.StartsWith("mdaresna_invoice_qa_", StringComparison.Ordinal)) throw new InvalidOperationException("Invalid QA database.");
            await db.Database.EnsureDeletedAsync();
        }
    }
    private sealed class FixtureIdentity(IdentityAccountId id) : Mdaresna.Platform.Application.Abstractions.Security.ISharedIdentityAccountProvisioner
    {
        public Task<IdentityAccountId> GetOrCreateByPhoneAsync(Guid registrationRequestId, string ownerName, string ownerPhone, CancellationToken cancellationToken = default) => Task.FromResult(id);
    }
}
