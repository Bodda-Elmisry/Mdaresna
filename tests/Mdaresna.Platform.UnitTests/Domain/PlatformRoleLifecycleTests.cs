using Mdaresna.Platform.Domain.Access;
using Mdaresna.Platform.Domain.Common;

namespace Mdaresna.Platform.UnitTests.Domain;

public sealed class PlatformRoleLifecycleTests
{
    [Fact]
    public void Custom_role_can_be_deactivated_and_reactivated()
    {
        var actor = IdentityAccountId.From(Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;
        var role = PlatformRole.Create(PlatformRoleId.New(), "school-reviewer", "Reviewer", false,
            [PlatformPermissionCodes.SchoolsRead], actor, now);

        role.Deactivate(actor, now.AddMinutes(1));
        Assert.False(role.IsActive);
        role.Activate(actor, now.AddMinutes(2));
        Assert.True(role.IsActive);
        Assert.Contains(role.DomainEvents, item => item.GetType().Name == "PlatformAccessChangedDomainEvent");
    }

    [Fact]
    public void System_role_cannot_be_deactivated()
    {
        var actor = IdentityAccountId.From(Guid.NewGuid());
        var now = DateTimeOffset.UtcNow;
        var role = PlatformRole.Create(PlatformRoleId.New(), "app-manager", "App Manager", true,
            [PlatformPermissionCodes.AccessManage], actor, now);

        Assert.Throws<PlatformDomainException>(() => role.Deactivate(actor, now.AddMinutes(1)));
        Assert.True(role.IsActive);
    }
}
