using System.Text.Json.Serialization;
using Mdaresna.Tenancy.Abstractions.Serialization;

namespace Mdaresna.Tenancy.Abstractions.Identifiers;

[JsonConverter(typeof(GuidIdJsonConverter<BranchId>))]
public readonly record struct BranchId : IGuidId<BranchId>
{
    private BranchId(Guid value) => Value = value;

    public Guid Value { get; }

    public bool IsEmpty => Value == Guid.Empty;

    public static BranchId New() => new(Guid.NewGuid());

    public static BranchId From(Guid value) => value == Guid.Empty
        ? throw new ArgumentException("BranchId cannot be empty.", nameof(value))
        : new BranchId(value);

    public static bool TryFrom(Guid value, out BranchId branchId)
    {
        branchId = default;

        if (value == Guid.Empty)
        {
            return false;
        }

        branchId = new BranchId(value);
        return true;
    }

    public static explicit operator Guid(BranchId id) => id.Value;

    public static explicit operator BranchId(Guid value) => From(value);

    public override string ToString() => Value.ToString("D");
}
