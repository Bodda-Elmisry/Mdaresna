using System.Text.Json.Serialization;
using Mdaresna.Tenancy.Abstractions.Identifiers;
using Mdaresna.Tenancy.Abstractions.Serialization;

namespace Mdaresna.Platform.Domain.Access;

[JsonConverter(typeof(GuidIdJsonConverter<PlatformRoleId>))]
public readonly record struct PlatformRoleId : IGuidId<PlatformRoleId>
{
    private PlatformRoleId(Guid value) => Value = value;

    public Guid Value { get; }

    public bool IsEmpty => Value == Guid.Empty;

    public static PlatformRoleId New() => new(Guid.NewGuid());

    public static PlatformRoleId From(Guid value) => value == Guid.Empty
        ? throw new ArgumentException("PlatformRoleId cannot be empty.", nameof(value))
        : new PlatformRoleId(value);

    public static bool TryFrom(Guid value, out PlatformRoleId roleId)
    {
        roleId = default;

        if (value == Guid.Empty)
        {
            return false;
        }

        roleId = new PlatformRoleId(value);
        return true;
    }

    public static explicit operator Guid(PlatformRoleId id) => id.Value;

    public static explicit operator PlatformRoleId(Guid value) => From(value);

    public override string ToString() => Value.ToString("D");
}
