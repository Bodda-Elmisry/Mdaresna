using Mdaresna.Schools.Application.Identity;
using Mdaresna.Schools.Domain.Identity;
using Mdaresna.Schools.Contracts.Provisioning;

namespace Mdaresna.Schools.UnitTests;

public sealed class SchoolIdentityTests
{
    [Fact]
    public void Login_identifier_separates_username_and_normalized_school_code()
    {
        var value = SchoolLoginIdentifier.Parse("ahmed@sch-001");
        Assert.Equal("ahmed", value.UserName);
        Assert.Equal("SCH-001", value.SchoolCode);
    }

    [Theory]
    [InlineData("ahmed")]
    [InlineData("a@b@c")]
    [InlineData("@SCH-001")]
    public void Login_identifier_rejects_invalid_values(string value) =>
        Assert.Throws<ArgumentException>(() => SchoolLoginIdentifier.Parse(value));

    [Fact]
    public void School_admin_seed_has_explicit_access_management_permissions()
    {
        Assert.Equal("school-admin", SchoolIdentitySeed.SchoolAdminRoleCode);
        Assert.Contains(SchoolIdentitySeed.Permissions, x => x.Code == "school.users.manage");
        Assert.Contains(SchoolIdentitySeed.Permissions, x => x.Code == "school.roles.manage");
        Assert.Equal(SchoolIdentitySeed.Permissions.Length,
            SchoolIdentitySeed.Permissions.Select(x => x.Code).Distinct(StringComparer.Ordinal).Count());
    }

    [Fact]
    public void Provisioning_result_exposes_login_without_transporting_an_activation_code()
    {
        Assert.Equal((ushort)3, SchoolProvisionedV1.SchemaVersion);
        Assert.NotNull(typeof(SchoolProvisionedV1).GetProperty(nameof(SchoolProvisionedV1.OwnerFullUserName)));
        Assert.Null(typeof(SchoolProvisionedV1).GetProperty("OwnerActivationCode"));
    }
}
