using System.Text.Json.Serialization;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Mdaresna.Tenancy.Abstractions.Serialization;

namespace Mdaresna.Platform.Domain.Access;

[JsonConverter(typeof(GuidIdJsonConverter<IdentityAccountId>))]
public readonly record struct IdentityAccountId : IGuidId<IdentityAccountId>
{
    private IdentityAccountId(Guid value) => Value = value;

    public Guid Value { get; }

    public bool IsEmpty => Value == Guid.Empty;

    public static IdentityAccountId New() => new(Guid.NewGuid());

    public static IdentityAccountId From(Guid value) => value == Guid.Empty
        ? throw new ArgumentException("IdentityAccountId cannot be empty.", nameof(value))
        : new IdentityAccountId(value);

    public static bool TryFrom(Guid value, out IdentityAccountId accountId)
    {
        accountId = default;

        if (value == Guid.Empty)
        {
            return false;
        }

        accountId = new IdentityAccountId(value);
        return true;
    }

    public static explicit operator Guid(IdentityAccountId id) => id.Value;

    public static explicit operator IdentityAccountId(Guid value) => From(value);

    public override string ToString() => Value.ToString("D");
}
