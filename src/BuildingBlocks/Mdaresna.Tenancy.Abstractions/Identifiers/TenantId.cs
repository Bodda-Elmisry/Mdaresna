using System.Text.Json.Serialization;
using Mdaresna.Tenancy.Abstractions.Serialization;

namespace Mdaresna.Tenancy.Abstractions.Identifiers;

[JsonConverter(typeof(GuidIdJsonConverter<TenantId>))]
public readonly record struct TenantId : IGuidId<TenantId>
{
    private TenantId(Guid value) => Value = value;

    public Guid Value { get; }

    public bool IsEmpty => Value == Guid.Empty;

    public static TenantId New() => new(Guid.NewGuid());

    public static TenantId From(Guid value) => value == Guid.Empty
        ? throw new ArgumentException("TenantId cannot be empty.", nameof(value))
        : new TenantId(value);

    public static bool TryFrom(Guid value, out TenantId tenantId)
    {
        tenantId = default;

        if (value == Guid.Empty)
        {
            return false;
        }

        tenantId = new TenantId(value);
        return true;
    }

    public static explicit operator Guid(TenantId id) => id.Value;

    public static explicit operator TenantId(Guid value) => From(value);

    public override string ToString() => Value.ToString("D");
}
