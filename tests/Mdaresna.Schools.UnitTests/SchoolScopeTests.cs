using Mdaresna.Schools.Domain;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.Schools.UnitTests;

public sealed class SchoolScopeTests
{
    [Fact]
    public void Scope_keeps_the_tenant_and_school_database_boundary_explicit()
    {
        var tenantId = TenantId.New();
        var schoolId = SchoolId.New();

        var scope = SchoolScope.Create(tenantId, schoolId);

        Assert.Equal(tenantId, scope.TenantId);
        Assert.Equal(schoolId, scope.SchoolId);
    }

    [Fact]
    public void Empty_identifiers_are_rejected()
    {
        Assert.Throws<ArgumentException>(() => SchoolScope.Create(default, SchoolId.New()));
        Assert.Throws<ArgumentException>(() => SchoolScope.Create(TenantId.New(), default));
    }
}
