using System.Text.Json;
using Mdaresna.Tenancy.Abstractions.Context;
using Mdaresna.Tenancy.Abstractions.Identifiers;

namespace Mdaresna.ArchitectureTests;

public sealed class IdentifierContractTests
{
    [Fact]
    public void Strong_identifier_serializes_as_a_guid_string()
    {
        var value = Guid.NewGuid();
        var tenantId = TenantId.From(value);

        var json = JsonSerializer.Serialize(tenantId);
        var roundTrip = JsonSerializer.Deserialize<TenantId>(json);

        Assert.Equal($"\"{value:D}\"", json);
        Assert.Equal(tenantId, roundTrip);
    }

    [Fact]
    public void Strong_identifiers_reject_empty_guid()
    {
        Assert.Throws<ArgumentException>(() => TenantId.From(Guid.Empty));
        Assert.Throws<ArgumentException>(() => SchoolId.From(Guid.Empty));
        Assert.Throws<ArgumentException>(() => BranchId.From(Guid.Empty));
    }

    [Fact]
    public void Empty_identifier_cannot_be_written_to_or_read_from_json()
    {
        Assert.Throws<JsonException>(() => JsonSerializer.Serialize(default(TenantId)));
        Assert.Throws<JsonException>(
            () => JsonSerializer.Deserialize<TenantId>($"\"{Guid.Empty:D}\""));
    }

    [Fact]
    public void Branch_context_requires_a_school_context()
    {
        var tenantId = TenantId.New();
        var branchId = BranchId.New();

        Assert.Throws<ArgumentException>(
            () => new TenantContext(tenantId, branchId: branchId));
    }
}
