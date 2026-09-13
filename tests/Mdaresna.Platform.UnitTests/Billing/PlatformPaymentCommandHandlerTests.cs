using Mdaresna.Platform.Application.Billing.ReviewSchoolPlatformPayment;
using Mdaresna.Platform.Application.Billing.SubmitSchoolPlatformPayment;
using Mdaresna.Platform.Application.Billing.Units;
using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Contracts.Billing;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Billing.Units;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.UnitTests.TestDoubles;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Billing;

public sealed class PlatformPaymentCommandHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 9, 13, 9, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Same_request_id_is_idempotent_but_changed_payload_is_rejected()
    {
        var school = CreateSchool();
        var payments = new FakePaymentRepository();
        var audit = new FakeBillingAuditWriter();
        var unitOfWork = new FakeBillingUnitOfWork();
        var handler = CreateSubmitHandler(school, payments, audit, unitOfWork);
        var command = CreateSubmitCommand(school);

        var first = await handler.HandleAsync(command);
        var replay = await handler.HandleAsync(command);

        Assert.True(first.WasCreated);
        Assert.False(replay.WasCreated);
        Assert.Single(payments.Requests);
        Assert.Single(audit.Actions);
        Assert.Equal(1, unitOfWork.SaveCount);

        var conflict = await Assert.ThrowsAsync<PlatformConflictException>(() =>
            handler.HandleAsync(command with { Amount = command.Amount + 1 }));
        Assert.Equal("request.idempotency_key_reused", conflict.Code);
    }

    [Fact]
    public async Task Transfer_reference_is_unique_for_each_school()
    {
        var school = CreateSchool();
        var payments = new FakePaymentRepository();
        var handler = CreateSubmitHandler(
            school, payments, new FakeBillingAuditWriter(), new FakeBillingUnitOfWork());
        var first = CreateSubmitCommand(school);
        await handler.HandleAsync(first);

        var conflict = await Assert.ThrowsAsync<PlatformConflictException>(() =>
            handler.HandleAsync(first with { RequestId = Guid.NewGuid() }));

        Assert.Equal("platform_payment.transfer_reference_in_use", conflict.Code);
        Assert.Single(payments.Requests);
    }

    [Fact]
    public async Task School_must_belong_to_the_claimed_tenant()
    {
        var school = CreateSchool();
        var handler = CreateSubmitHandler(
            school, new FakePaymentRepository(), new FakeBillingAuditWriter(), new FakeBillingUnitOfWork());

        var conflict = await Assert.ThrowsAsync<PlatformConflictException>(() =>
            handler.HandleAsync(CreateSubmitCommand(school) with { TenantId = TenantId.New() }));

        Assert.Equal("school.unavailable_for_payment", conflict.Code);
    }

    [Fact]
    public async Task Approval_records_ledger_audit_and_typed_outbox_in_one_save()
    {
        var payments = new FakePaymentRepository();
        var request = CreateRequest();
        payments.Requests.Add(request);
        var audit = new FakeBillingAuditWriter();
        var outbox = new FakeOutboxWriter();
        var unitOfWork = new FakeBillingUnitOfWork();
        var handler = new ReviewSchoolPlatformPaymentCommandHandler(
            payments, audit, outbox, unitOfWork, new FakeClock(Now.AddMinutes(1)),
            new FakeUnitPurchaseRepository(), new FakeUnitCommerceAuditWriter());
        var command = new ReviewSchoolPlatformPaymentCommand(
            request.Id, PlatformPaymentReviewDecision.Approve,
            IdentityAccountId.New(), "Verified with bank", Guid.NewGuid());

        var result = await handler.HandleAsync(command);

        Assert.Equal(PlatformPaymentStatus.Approved, result.Status);
        Assert.NotNull(result.LedgerEntryId);
        Assert.Null(result.UnitGrantId);
        Assert.Single(payments.LedgerEntries);
        Assert.Single(audit.Actions);
        Assert.Equal("Approved", audit.Actions[0].Action);
        Assert.Equal(2, unitOfWork.SaveCount);
        Assert.Empty(request.DomainEvents);

        var envelope = outbox.Single<SchoolPlatformPaymentReviewedV1>();
        Assert.Equal(request.Id, envelope.Data.PaymentRequestId);
        Assert.Equal(SchoolPlatformPaymentStatusV1.Approved, envelope.Data.Status);
        Assert.Equal(request.SchoolId, envelope.Scope.SchoolId);
        Assert.Equal(request.TenantId, envelope.Scope.TenantId);
    }

    [Fact]
    public async Task Rejection_has_no_ledger_entry_and_cannot_be_reviewed_again()
    {
        var payments = new FakePaymentRepository();
        var request = CreateRequest();
        payments.Requests.Add(request);
        var outbox = new FakeOutboxWriter();
        var handler = new ReviewSchoolPlatformPaymentCommandHandler(
            payments, new FakeBillingAuditWriter(), outbox,
            new FakeBillingUnitOfWork(), new FakeClock(Now.AddMinutes(1)),
            new FakeUnitPurchaseRepository(), new FakeUnitCommerceAuditWriter());
        var command = new ReviewSchoolPlatformPaymentCommand(
            request.Id, PlatformPaymentReviewDecision.Reject,
            IdentityAccountId.New(), "No matching transfer", Guid.NewGuid());

        var result = await handler.HandleAsync(command);

        Assert.Equal(PlatformPaymentStatus.Rejected, result.Status);
        Assert.Null(result.LedgerEntryId);
        Assert.Null(result.UnitGrantId);
        Assert.Empty(payments.LedgerEntries);
        Assert.Equal(SchoolPlatformPaymentStatusV1.Rejected,
            outbox.Single<SchoolPlatformPaymentReviewedV1>().Data.Status);
        await Assert.ThrowsAsync<PlatformConflictException>(() => handler.HandleAsync(command));
    }

    [Fact]
    public async Task Review_domain_event_remains_until_save_succeeds()
    {
        var payments = new FakePaymentRepository();
        var request = CreateRequest();
        payments.Requests.Add(request);
        var handler = new ReviewSchoolPlatformPaymentCommandHandler(
            payments, new FakeBillingAuditWriter(), new FakeOutboxWriter(),
            new FakeBillingUnitOfWork(new InvalidOperationException("Save failed")),
            new FakeClock(Now.AddMinutes(1)), new FakeUnitPurchaseRepository(),
            new FakeUnitCommerceAuditWriter());

        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.HandleAsync(
            new ReviewSchoolPlatformPaymentCommand(
                request.Id, PlatformPaymentReviewDecision.Approve,
                IdentityAccountId.New(), null, Guid.NewGuid())));

        Assert.Single(request.DomainEvents);
    }

    [Fact]
    public async Task Unit_purchase_approval_grants_snapshot_and_stages_school_event_once()
    {
        var payments = new FakePaymentRepository();
        var purchases = new FakeUnitPurchaseRepository();
        var unitAudit = new FakeUnitCommerceAuditWriter();
        var outbox = new FakeOutboxWriter();
        var unitOfWork = new FakeBillingUnitOfWork();
        var (request, intent) = CreateUnitPurchase();
        payments.Requests.Add(request);
        purchases.Intents.Add(intent);
        var handler = new ReviewSchoolPlatformPaymentCommandHandler(
            payments, new FakeBillingAuditWriter(), outbox, unitOfWork,
            new FakeClock(Now.AddMinutes(1)), purchases, unitAudit);
        var command = new ReviewSchoolPlatformPaymentCommand(
            request.Id, PlatformPaymentReviewDecision.Approve,
            IdentityAccountId.New(), "Matched bank transfer", Guid.NewGuid());

        var result = await handler.HandleAsync(command);

        var grant = Assert.Single(purchases.Grants);
        var envelope = outbox.Single<SchoolUnitsGrantedV1>();
        Assert.Equal(grant.Id, result.UnitGrantId);
        Assert.Equal(grant.Id, envelope.MessageId);
        Assert.Equal(grant.Id, envelope.Data.GrantId);
        Assert.Equal(request.Id, envelope.Data.PaymentRequestId);
        Assert.Equal(request.TenantId, envelope.Scope.TenantId);
        Assert.Equal(request.SchoolId, envelope.Scope.SchoolId);
        Assert.Equal(intent.Quantity, envelope.Data.Quantity);
        Assert.Equal(intent.UnitPrice, envelope.Data.UnitPrice);
        Assert.Equal(intent.Amount, envelope.Data.Amount);
        Assert.Equal(intent.PaymentMethodCode, envelope.Data.PaymentMethodCode);
        Assert.Equal(intent.TransferOccurredAtUtc, envelope.Data.TransferOccurredAtUtc);
        Assert.Equal("platform.unit_grant.issued", Assert.Single(unitAudit.Records).Action);
        Assert.Single(payments.LedgerEntries);
        Assert.Equal(2, unitOfWork.SaveCount);
        await Assert.ThrowsAsync<PlatformConflictException>(() => handler.HandleAsync(command));
        Assert.Single(purchases.Grants);
    }

    [Fact]
    public async Task Unit_purchase_rejection_does_not_grant_or_stage_school_units_event()
    {
        var payments = new FakePaymentRepository();
        var purchases = new FakeUnitPurchaseRepository();
        var outbox = new FakeOutboxWriter();
        var (request, intent) = CreateUnitPurchase();
        payments.Requests.Add(request);
        purchases.Intents.Add(intent);
        var handler = new ReviewSchoolPlatformPaymentCommandHandler(
            payments, new FakeBillingAuditWriter(), outbox, new FakeBillingUnitOfWork(),
            new FakeClock(Now.AddMinutes(1)), purchases, new FakeUnitCommerceAuditWriter());

        var result = await handler.HandleAsync(new ReviewSchoolPlatformPaymentCommand(
            request.Id, PlatformPaymentReviewDecision.Reject,
            IdentityAccountId.New(), "No matching transfer", Guid.NewGuid()));

        Assert.Null(result.UnitGrantId);
        Assert.Empty(purchases.Grants);
        Assert.Empty(outbox.Messages.OfType<Mdaresna.IntegrationContracts.Messaging.IntegrationMessageEnvelope<SchoolUnitsGrantedV1>>());
    }

    private static SubmitSchoolPlatformPaymentCommandHandler CreateSubmitHandler(
        SchoolRegistration school,
        FakePaymentRepository payments,
        FakeBillingAuditWriter audit,
        FakeBillingUnitOfWork unitOfWork) => new(
        payments,
        new FakeSchoolRegistrationRepository(school),
        audit,
        unitOfWork,
        new FakeClock(Now));

    private static SchoolRegistration CreateSchool()
    {
        var tenantId = TenantId.New();
        return SchoolRegistration.Create(
            SchoolId.From(tenantId.Value), tenantId, Guid.NewGuid(), SchoolCode.Create("SCHOOL-001"),
            "School One", SchoolType.Private, DeploymentMode.SharedSaaS,
            Guid.NewGuid(), Now);
    }

    private static SubmitSchoolPlatformPaymentCommand CreateSubmitCommand(SchoolRegistration school) => new(
        Guid.NewGuid(), school.TenantId, school.Id, 250.50m, "EGP", "BANK-001",
        IdentityAccountId.New(), Guid.NewGuid());

    private static PlatformPaymentRequest CreateRequest() => PlatformPaymentRequest.Create(
        Guid.NewGuid(), TenantId.New(), SchoolId.New(), 250.50m, "EGP", "BANK-001",
        IdentityAccountId.New(), Now);

    private static (PlatformPaymentRequest Payment, UnitPurchaseIntent Intent) CreateUnitPurchase()
    {
        var tenantId = TenantId.New();
        var schoolId = SchoolId.From(tenantId.Value);
        var unitType = UnitType.Create(Guid.NewGuid(), "STUDENT-ACTIVATION",
            "Student activation", 12.50m, "EGP", Now.AddDays(-1));
        var paymentId = Guid.NewGuid();
        var intent = UnitPurchaseIntent.Create(paymentId, tenantId, schoolId,
            unitType, 20, "bank-transfer", Now.AddMinutes(-30), Now);
        var payment = PlatformPaymentRequest.Create(paymentId, tenantId, schoolId,
            intent.Amount, intent.Currency, "BANK-UNITS-001", IdentityAccountId.New(), Now);
        return (payment, intent);
    }
}
