using System.Text.Json.Serialization;
using Mdaresna.Tenancy.Abstractions.Serialization;

namespace Mdaresna.Tenancy.Abstractions.Identifiers;

[JsonConverter(typeof(GuidIdJsonConverter<SchoolId>))]
public readonly record struct SchoolId : IGuidId<SchoolId>
{
    private SchoolId(Guid value) => Value = value;

    public Guid Value { get; }

    public bool IsEmpty => Value == Guid.Empty;

    public static SchoolId New() => new(Guid.NewGuid());

    public static SchoolId From(Guid value) => value == Guid.Empty
        ? throw new ArgumentException("SchoolId cannot be empty.", nameof(value))
        : new SchoolId(value);

    public static bool TryFrom(Guid value, out SchoolId schoolId)
    {
        schoolId = default;

        if (value == Guid.Empty)
        {
            return false;
        }

        schoolId = new SchoolId(value);
        return true;
    }

    public static explicit operator Guid(SchoolId id) => id.Value;

    public static explicit operator SchoolId(Guid value) => From(value);

    public override string ToString() => Value.ToString("D");
}
