using Mdaresna.Platform.Domain.Registry;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Platform.UnitTests.Domain;

public sealed class TenantTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 9, 12, 8, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Closing_a_tenant_preserves_the_operational_reason()
    {
        var tenant = Tenant.Create(TenantId.New(), "Tenant One", null, Now);

        tenant.Close("Contract ended", Now.AddMinutes(1));

        Assert.Equal(TenantStatus.Closed, tenant.Status);
        Assert.Equal("Contract ended", tenant.SuspensionReason);
        Assert.Equal(2, tenant.Version);
    }

    [Fact]
    public void Reinstating_a_suspended_tenant_clears_the_reason()
    {
        var tenant = Tenant.Create(TenantId.New(), "Tenant One", null, Now);
        tenant.Activate(Now.AddMinutes(1));
        tenant.Suspend("Payment review", Now.AddMinutes(2));

        tenant.Reinstate(Now.AddMinutes(3));

        Assert.Equal(TenantStatus.Active, tenant.Status);
        Assert.Null(tenant.SuspensionReason);
    }
}
