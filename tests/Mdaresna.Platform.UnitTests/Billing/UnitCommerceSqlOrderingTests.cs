using Mdaresna.Platform.Application.Errors;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Billing;
using Mdaresna.Platform.Domain.Billing.Units;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.Infrastructure.Persistence.Platform;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Billing;
using Mdaresna.Platform.Infrastructure.Persistence.Platform.Entities;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Mdaresna.Platform.UnitTests.Billing;

/// <summary>
/// Opt-in against the already-migrated local development database. Each scenario
/// rolls back all its rows. Set MDARESNA_PLATFORM_UNIT_INTEGRATION_CONNECTION.
/// </summary>
public sealed class UnitCommerceSqlOrderingTests
{
    [Fact]
    public async Task ReviewStagesApprovalBeforeGrantTriggerWithinOneTransaction()
    {
        var connection = GetApprovedLocalConnection();
        if (connection is null)
        {
            return;
        }

        var options = CreateOptions(connection);
        await using var db = new PlatformDbContext(options);
        var unitOfWork = new PlatformBillingUnitOfWork(db);
        Guid paymentId = Guid.Empty;
        Guid schoolGuid = Guid.Empty;
        Guid unitTypeId = Guid.Empty;
        Guid outboxId = Guid.Empty;

        // Throw only after proving the persisted state within the transaction.
        // The transaction then rolls the entire fixture back.
        await Assert.ThrowsAsync<RollbackProbeException>(() =>
            unitOfWork.ExecuteInTransactionAsync<int>(async ct =>
            {
                var fixture = await SeedPurchaseAsync(db, unitOfWork, ct);
                paymentId = fixture.Payment.Id;
                schoolGuid = fixture.SchoolGuid;
                unitTypeId = fixture.UnitTypeId;
                fixture.Payment.Review(PlatformPaymentReviewDecision.Approve,
                    fixture.ReviewerId, fixture.Now.AddMinutes(1), null);
                await unitOfWork.SaveChangesAsync(ct);

                var grant = UnitGrant.FromApprovedPayment(
                    fixture.Payment, fixture.Intent, fixture.Now.AddMinutes(1));
                db.PlatformPaymentLedgerEntries.Add(
                    PlatformPaymentLedgerEntry.FromApprovedRequest(fixture.Payment));
                db.UnitGrants.Add(grant);
                db.OutboxMessages.Add(CreateOutbox(grant));
                outboxId = grant.Id;
                await unitOfWork.SaveChangesAsync(ct);

                Assert.Equal(PlatformPaymentStatus.Approved,
                    (await db.PlatformPaymentRequests.AsNoTracking()
                        .SingleAsync(x => x.Id == paymentId, ct)).Status);
                Assert.Equal(grant.Id,
                    (await db.UnitGrants.AsNoTracking()
                        .SingleAsync(x => x.PaymentRequestId == paymentId, ct)).Id);
                throw new RollbackProbeException();
            }));

        await AssertNotPersistedAsync(options, paymentId, schoolGuid, unitTypeId, outboxId);
    }

    [Fact]
    public async Task FailedSecondFlushRollsBackTheEarlierPaymentApproval()
    {
        var connection = GetApprovedLocalConnection();
        if (connection is null)
        {
            return;
        }

        var options = CreateOptions(connection);
        await using var db = new PlatformDbContext(options);
        var unitOfWork = new PlatformBillingUnitOfWork(db);
        Guid paymentId = Guid.Empty;
        Guid schoolGuid = Guid.Empty;
        Guid unitTypeId = Guid.Empty;
        Guid outboxId = Guid.Empty;

        await Assert.ThrowsAsync<PlatformConflictException>(() =>
            unitOfWork.ExecuteInTransactionAsync<int>(async ct =>
            {
                var fixture = await SeedPurchaseAsync(db, unitOfWork, ct);
                paymentId = fixture.Payment.Id;
                schoolGuid = fixture.SchoolGuid;
                unitTypeId = fixture.UnitTypeId;
                fixture.Payment.Review(PlatformPaymentReviewDecision.Approve,
                    fixture.ReviewerId, fixture.Now.AddMinutes(1), null);
                await unitOfWork.SaveChangesAsync(ct);

                // Two different grant IDs for one payment violate the unique
                // PaymentRequestId index at the second SQL flush.
                var grant = UnitGrant.FromApprovedPayment(
                    fixture.Payment, fixture.Intent, fixture.Now.AddMinutes(1));
                var duplicate = UnitGrant.FromApprovedPayment(
                    fixture.Payment, fixture.Intent, fixture.Now.AddMinutes(1));
                db.PlatformPaymentLedgerEntries.Add(
                    PlatformPaymentLedgerEntry.FromApprovedRequest(fixture.Payment));
                db.UnitGrants.AddRange(grant, duplicate);
                db.OutboxMessages.Add(CreateOutbox(grant));
                outboxId = grant.Id;
                await unitOfWork.SaveChangesAsync(ct);
                return 0;
            }));

        await AssertNotPersistedAsync(options, paymentId, schoolGuid, unitTypeId, outboxId);
    }

    private static async Task<PurchaseFixture> SeedPurchaseAsync(
        PlatformDbContext db,
        PlatformBillingUnitOfWork unitOfWork,
        CancellationToken cancellationToken)
    {
        var now = new DateTimeOffset(2026, 9, 13, 10, 0, 0, TimeSpan.Zero);
        var schoolGuid = Guid.NewGuid();
        var tenantId = TenantId.From(schoolGuid);
        var schoolId = SchoolId.From(schoolGuid);
        var requesterId = IdentityAccountId.From(Guid.NewGuid());
        var reviewerId = IdentityAccountId.From(Guid.NewGuid());
        var unitType = UnitType.Create(
            Guid.NewGuid(), $"U-{Guid.NewGuid():N}"[..18], "Activation unit", 10m,
            "EGP", now);
        db.Tenants.Add(Tenant.Create(tenantId, "Unit test school", null, now));
        db.Schools.Add(SchoolRegistration.Create(
            schoolId, tenantId, Guid.NewGuid(),
            SchoolCode.Create($"S-{Guid.NewGuid():N}"[..18]),
            "Unit test school", SchoolType.Private,
            DeploymentMode.SharedSaaS, requesterId.Value, now));
        db.UnitTypes.Add(unitType);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        var payment = PlatformPaymentRequest.Create(
            Guid.NewGuid(), tenantId, schoolId, 20m, "EGP",
            $"REF-{Guid.NewGuid():N}", requesterId, now);
        var intent = UnitPurchaseIntent.Create(
            payment.Id, tenantId, schoolId, unitType, 2,
            "bank-transfer", now, now);
        db.PlatformPaymentRequests.Add(payment);
        db.UnitPurchaseIntents.Add(intent);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new PurchaseFixture(payment, intent, reviewerId, now,
            schoolGuid, unitType.Id);
    }

    private static PlatformOutboxMessage CreateOutbox(UnitGrant grant) => new()
    {
        Id = grant.Id,
        MessageType = "mdaresna.platform.school-units-granted.v1",
        SchemaVersion = 1,
        PayloadJson = "{}",
        OccurredAtUtc = grant.GrantedAtUtc,
        CorrelationId = Guid.NewGuid()
    };

    private static async Task AssertNotPersistedAsync(
        DbContextOptions<PlatformDbContext> options,
        Guid paymentId,
        Guid schoolGuid,
        Guid unitTypeId,
        Guid outboxId)
    {
        Assert.NotEqual(Guid.Empty, paymentId);
        Assert.NotEqual(Guid.Empty, schoolGuid);
        Assert.NotEqual(Guid.Empty, unitTypeId);
        Assert.NotEqual(Guid.Empty, outboxId);
        await using var verificationDb = new PlatformDbContext(options);
        Assert.False(await verificationDb.Tenants.AsNoTracking()
            .AnyAsync(x => x.Id == TenantId.From(schoolGuid)));
        Assert.False(await verificationDb.Schools.AsNoTracking()
            .AnyAsync(x => x.Id == SchoolId.From(schoolGuid)));
        Assert.False(await verificationDb.UnitTypes.AsNoTracking()
            .AnyAsync(x => x.Id == unitTypeId));
        Assert.False(await verificationDb.PlatformPaymentRequests.AsNoTracking()
            .AnyAsync(x => x.Id == paymentId));
        Assert.False(await verificationDb.UnitPurchaseIntents.AsNoTracking()
            .AnyAsync(x => x.PaymentRequestId == paymentId));
        Assert.False(await verificationDb.UnitGrants.AsNoTracking()
            .AnyAsync(x => x.PaymentRequestId == paymentId));
        Assert.False(await verificationDb.OutboxMessages.AsNoTracking()
            .AnyAsync(x => x.Id == outboxId));
    }

    private static string? GetApprovedLocalConnection()
    {
        var connection = Environment.GetEnvironmentVariable(
            "MDARESNA_PLATFORM_UNIT_INTEGRATION_CONNECTION");
        if (string.IsNullOrWhiteSpace(connection))
        {
            return null;
        }

        var target = new SqlConnectionStringBuilder(connection);
        Assert.Equal(@"(localdb)\MSSQLLocalDB", target.DataSource, ignoreCase: true);
        Assert.Equal("MdaresnaPlatformLocal", target.InitialCatalog, ignoreCase: true);
        return connection;
    }

    private static DbContextOptions<PlatformDbContext> CreateOptions(string connection) =>
        new DbContextOptionsBuilder<PlatformDbContext>()
            .UseSqlServer(connection)
            .Options;

    private sealed record PurchaseFixture(
        PlatformPaymentRequest Payment,
        UnitPurchaseIntent Intent,
        IdentityAccountId ReviewerId,
        DateTimeOffset Now,
        Guid SchoolGuid,
        Guid UnitTypeId);

    private sealed class RollbackProbeException : Exception;
}
