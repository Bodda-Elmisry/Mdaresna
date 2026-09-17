using Mdaresna.IntegrationContracts.Messaging;
using Mdaresna.Platform.Application.Abstractions.Security;
using Mdaresna.Platform.Application.Registry.ConsumeSchoolRegistrationRequest;
using Mdaresna.Platform.Application.Registry.CreateTenant;
using Mdaresna.Platform.Application.Registry.Lifecycle;
using Mdaresna.Platform.Application.Registry.RegisterSchool;
using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Platform.UnitTests.TestDoubles;
using Mdaresna.Schools.Contracts.Registration;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Registry;

public sealed class ConsumeSchoolRegistrationRequestV2HandlerTests
{
    [Fact]
    public async Task Request_resolves_owner_and_creates_pending_school_with_primary_phone()
    {
        var now = new DateTimeOffset(2026, 9, 17, 0, 0, 0, TimeSpan.Zero);
        var tenants = new FakeTenantRepository();
        var schools = new FakeSchoolRegistrationRepository();
        var outbox = new FakeOutboxWriter();
        var audit = new FakeRegistryAuditWriter();
        var unitOfWork = new FakePlatformUnitOfWork();
        var clock = new FakeClock(now);
        var owner = new Provisioner();
        var handler = new ConsumeSchoolRegistrationRequestV2Handler(
            owner, schools,
            new CreateTenantCommandHandler(tenants, outbox, audit, unitOfWork, clock),
            new RegisterSchoolCommandHandler(tenants, schools, outbox, audit, unitOfWork, clock),
            new TransitionSchoolCommandHandler(schools, outbox, audit, unitOfWork, clock));
        var tenantId = TenantId.New();
        var data = new SchoolRegistrationRequestedV2(Guid.NewGuid(), tenantId, "SCH-1",
            "مدرسة النور", RequestedSchoolTypeV2.Private, "القاهرة", "00201111111111",
            "أحمد علي", "00201222222222", now);
        var envelope = IntegrationMessageEnvelope<SchoolRegistrationRequestedV2>.Create(
            now, "schools", IntegrationMessageScope.ForTenant(tenantId), data);

        await handler.HandleAsync(envelope);

        var school = Assert.Single(schools.Items);
        Assert.Equal(SchoolLifecycleStatus.PendingVerification, school.Status);
        Assert.Equal("00201111111111", school.PrimaryPhone);
        Assert.Equal("00201222222222", owner.Phone);
    }

    private sealed class Provisioner : ISharedIdentityAccountProvisioner
    {
        public string? Phone { get; private set; }
        public Task<IdentityAccountId> GetOrCreateByPhoneAsync(Guid registrationRequestId,
            string ownerName, string ownerPhone, CancellationToken cancellationToken = default)
        {
            Phone = ownerPhone;
            return Task.FromResult(IdentityAccountId.From(Guid.NewGuid()));
        }
    }
}
